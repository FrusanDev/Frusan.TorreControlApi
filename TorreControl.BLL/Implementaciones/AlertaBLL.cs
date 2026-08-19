using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Newtonsoft.Json;
using TorreControl.BEL;
using TorreControl.DAL;

namespace TorreControl.BLL
{
    public class AlertaBLL : IAlertaBLL
    {
        #region Atributos

        private IAlertaDAL alertaDAL;
        private ISpWtspSendMessageDAL spWtspSendMessageDAL;
        private IRegistroLogEventoErrorDAL registroLogEventoErrorDAL;
        private IEmailSenderDAL emailSenderDAL;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor que recibe las dependencias DAL mediante inyección de Unity
        /// </summary>
        /// <param name="alertaDAL"></param>
        /// <param name="spWtspSendMessageDAL"></param>
        /// <param name="registroLogEventoErrorDAL"></param>
        /// <param name="emailSenderDAL"></param>
        public AlertaBLL(IAlertaDAL alertaDAL, ISpWtspSendMessageDAL spWtspSendMessageDAL, IRegistroLogEventoErrorDAL registroLogEventoErrorDAL, IEmailSenderDAL emailSenderDAL)
        {
            this.alertaDAL = alertaDAL;
            this.spWtspSendMessageDAL = spWtspSendMessageDAL;
            this.registroLogEventoErrorDAL = registroLogEventoErrorDAL;
            this.emailSenderDAL = emailSenderDAL;
        }

        #endregion

        #region IAlertaBLL Implementations

        /// <summary>
        /// Valida el tipo de alerta, inserta el evento en la tabla central TC_Evento y dispara la notificación WhatsApp
        /// al grupo Torre de Control y, si corresponde, a los responsables del tipo de alerta
        /// </summary>
        /// <param name="request">DTO con código de tipo de alerta y payload JSON</param>
        /// <param name="origenAutenticado">Código del sistema origen autenticado por API Key; es el único valor de confianza para OrigenSistema</param>
        /// <returns>Id del evento generado en TC_Evento más área/código/severidad, para que el controller difunda la notificación de alerta nueva</returns>
        public AlertaRegistradaBEL IngresarAlerta(IngresarAlertaRequest request, string origenAutenticado)
        {
            var tipoAlerta = this.alertaDAL.ObtenerTipoAlerta(request.CodigoTipoAlerta);
            if (tipoAlerta == null || !tipoAlerta.Activo)
            {
                string mensajeValidacion = $"Tipo de alerta '{request.CodigoTipoAlerta}' no existe o no está activo.";
                RegistrarLogError(origenAutenticado, mensajeValidacion, notificarEmergencia: false);
                throw new Exception(mensajeValidacion);
            }

            try
            {
                string payloadJson = JsonConvert.SerializeObject(request.Payload);

                var evento = new EventoBEL
                {
                    IdTipoAlerta = tipoAlerta.IdTipoAlerta,
                    Payload = payloadJson,
                    Estado = "No atendido",
                    FechaOcurrencia = DateTime.Now,
                    OrigenSistema = origenAutenticado,
                    Severidad = request.Severidad,
                    DescripcionBreve = request.DescripcionBreve,
                    MensajeWhatsapp = request.MensajeWhatsapp,
                    AsuntoCorreo = request.AsuntoCorreo,
                    MensajeCorreo = request.MensajeCorreo
                };

                int idEvento = this.alertaDAL.InsertarEvento(evento);

                string mensajeWhatsapp = !string.IsNullOrWhiteSpace(evento.MensajeWhatsapp)
                    ? evento.MensajeWhatsapp
                    : ConstruirMensajeGenerico(tipoAlerta, evento);

                NotificarGrupo(tipoAlerta, mensajeWhatsapp);
                NotificarResponsables(tipoAlerta, mensajeWhatsapp);

                // Envío de correo a responsables — implementado y listo, pero la llamada queda comentada
                // a propósito (decisión de Gonzalo, 18/08/2026) hasta confirmar credenciales SMTP vigentes
                // y activarlo explícitamente. Ver NotificarResponsablesEmail más abajo.
                // string asuntoCorreo = !string.IsNullOrWhiteSpace(evento.AsuntoCorreo)
                //     ? evento.AsuntoCorreo
                //     : ConstruirAsuntoCorreoGenerico(tipoAlerta);
                // string mensajeCorreo = !string.IsNullOrWhiteSpace(evento.MensajeCorreo)
                //     ? evento.MensajeCorreo
                //     : ConstruirMensajeCorreoGenerico(tipoAlerta, evento);
                // NotificarResponsablesEmail(tipoAlerta, asuntoCorreo, mensajeCorreo);

                return new AlertaRegistradaBEL
                {
                    IdEvento = idEvento,
                    Area = tipoAlerta.Area,
                    CodigoTipoAlerta = tipoAlerta.Codigo,
                    Severidad = evento.Severidad,
                    DescripcionBreve = evento.DescripcionBreve
                };
            }
            catch (Exception ex)
            {
                RegistrarLogError(origenAutenticado, ex.Message, notificarEmergencia: true);
                throw;
            }
        }

        /// <summary>
        /// Obtiene los eventos registrados dentro de un rango de fechas y, opcionalmente, filtrados por estado,
        /// para consumo de dashboards externos (GET /api/alertas)
        /// </summary>
        /// <param name="fechaDesde">Límite inferior del rango; si es null, el SP acota por defecto a los últimos 7 días</param>
        /// <param name="fechaHasta">Límite superior del rango; si es null, el SP usa el día de hoy</param>
        /// <param name="estado">Filtro opcional por Estado ('No atendido', 'Atendido' o 'Cerrado')</param>
        /// <returns>Lista de eventos que cumplen los filtros, más recientes primero</returns>
        public List<EventoConsultaBEL> ObtenerEventos(DateTime? fechaDesde, DateTime? fechaHasta, string estado)
        {
            if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde.Value > fechaHasta.Value)
                throw new Exception("FechaDesde no puede ser posterior a FechaHasta.");

            if (!string.IsNullOrWhiteSpace(estado) && estado != "No atendido" && estado != "Atendido" && estado != "Cerrado")
                throw new Exception("Estado debe ser 'No atendido', 'Atendido' o 'Cerrado'.");

            return this.alertaDAL.ObtenerEventos(fechaDesde, fechaHasta, estado);
        }

        #endregion

        #region Métodos privados

        /// <summary>
        /// Registra el error en la tabla de log compartida de Frusan y, si corresponde, avisa por WhatsApp
        /// al grupo de emergencia. Los errores de validación (ej. código de tipo de alerta inexistente) se
        /// registran sin notificar para no generar ruido por errores de uso de quien consume la API; los
        /// errores técnicos (ej. falla al insertar en TC_Evento) sí notifican.
        /// </summary>
        /// <param name="origenSistema">Sistema origen autenticado que hizo la request, para contexto en el log</param>
        /// <param name="errorDescripcion">Mensaje de error a registrar</param>
        /// <param name="notificarEmergencia">Si es true, además envía WhatsApp al grupo de emergencia</param>
        private void RegistrarLogError(string origenSistema, string errorDescripcion, bool notificarEmergencia)
        {
            try
            {
                var servidor = ConfigurationManager.AppSettings["executeServer"];
                var descripcionConOrigen = string.IsNullOrEmpty(origenSistema) ? errorDescripcion : $"[{origenSistema}] {errorDescripcion}";

                this.registroLogEventoErrorDAL.InsertarRegistroLogEvento(new RegistroLogEventoBEL
                {
                    FechaHora = DateTime.Now,
                    Servidor = servidor,
                    NombreTarea = "IngresarAlerta",
                    ErrorDescripcion = descripcionConOrigen
                });

                if (notificarEmergencia)
                {
                    var grupoIdEmergencia = ConfigurationManager.AppSettings["grupoIdWhatsappGrupoEmergencia"];
                    if (!string.IsNullOrEmpty(grupoIdEmergencia))
                    {
                        var body = new StringBuilder();
                        body.AppendFormat("*Torre de Control API - Error*{0}", Environment.NewLine);
                        body.AppendFormat("Servidor: {0}{1}", servidor, Environment.NewLine);
                        body.AppendFormat("Tarea: IngresarAlerta{0}", Environment.NewLine);
                        body.AppendFormat("Error: {0}", descripcionConOrigen);

                        this.spWtspSendMessageDAL.EnviarMensajeWhatsapp(grupoIdEmergencia, body.ToString());
                    }
                }
            }
            catch
            {
                // No propagar errores de logging/notificación — la excepción original ya se relanza
            }
        }

        /// <summary>
        /// Construye el mensaje genérico de alerta (Área/Alerta/Origen/Fecha) usado cuando el sistema
        /// origen no envía un MensajeWhatsapp propio en el request
        /// </summary>
        /// <param name="tipoAlerta">Datos del tipo de alerta (área, nombre)</param>
        /// <param name="evento">Datos del evento recién insertado (origen, fecha)</param>
        /// <returns>Texto del mensaje a enviar por WhatsApp</returns>
        private string ConstruirMensajeGenerico(TipoAlertaBEL tipoAlerta, EventoBEL evento)
        {
            var body = new StringBuilder();
            body.AppendFormat("*Torre de Control*{0}", Environment.NewLine);
            body.AppendFormat("Área: {0}{1}", tipoAlerta.Area, Environment.NewLine);
            body.AppendFormat("Alerta: {0}{1}", tipoAlerta.Nombre, Environment.NewLine);
            body.AppendFormat("Origen: {0}{1}", evento.OrigenSistema, Environment.NewLine);
            body.AppendFormat("Fecha: {0}", evento.FechaOcurrencia.ToString("dd/MM/yyyy HH:mm:ss"));
            return body.ToString();
        }

        /// <summary>
        /// Envía el mensaje al grupo único WhatsApp Torre de Control mediante el SP SpWtspSendMessage,
        /// solo si el tipo de alerta tiene habilitada la publicación al grupo (PublicaGrupoWzap)
        /// </summary>
        /// <param name="tipoAlerta">Datos del tipo de alerta (define si corresponde publicar al grupo)</param>
        /// <param name="mensaje">Texto ya armado del mensaje a enviar</param>
        private void NotificarGrupo(TipoAlertaBEL tipoAlerta, string mensaje)
        {
            try
            {
                if (!tipoAlerta.PublicaGrupoWzap)
                    return;

                var grupoId = ConfigurationManager.AppSettings["grupoIdWhatsappGrupoTorreControl"];

                if (string.IsNullOrEmpty(grupoId))
                    return;

                this.spWtspSendMessageDAL.EnviarMensajeWhatsapp(grupoId, mensaje);
            }
            catch
            {
                // No propagar excepciones de notificación — el insert ya se realizó
            }
        }

        /// <summary>
        /// Envía el mensaje individualmente a cada responsable activo del tipo de alerta, solo si el tipo
        /// tiene habilitado el envío a responsables (AlertaResponsableWzap) y tiene responsables asociados
        /// </summary>
        /// <param name="tipoAlerta">Datos del tipo de alerta (define si corresponde notificar a responsables)</param>
        /// <param name="mensaje">Texto ya armado del mensaje a enviar</param>
        private void NotificarResponsables(TipoAlertaBEL tipoAlerta, string mensaje)
        {
            try
            {
                if (!tipoAlerta.AlertaResponsableWzap)
                    return;

                var responsables = this.alertaDAL.ObtenerResponsables(tipoAlerta.IdTipoAlerta);

                if (responsables == null || responsables.Count == 0)
                    return;

                foreach (var responsable in responsables)
                {
                    if (string.IsNullOrEmpty(responsable.Telefono))
                        continue;

                    try
                    {
                        this.spWtspSendMessageDAL.EnviarMensajeWhatsapp(responsable.Telefono, mensaje);
                    }
                    catch
                    {
                        // No interrumpir el envío al resto de responsables por la falla de uno
                    }
                }
            }
            catch
            {
                // No propagar excepciones de notificación — el insert ya se realizó
            }
        }

        /// <summary>
        /// Construye el asunto genérico de correo usado cuando el sistema origen no envía un AsuntoCorreo
        /// propio en el request
        /// </summary>
        /// <param name="tipoAlerta">Datos del tipo de alerta (nombre)</param>
        /// <returns>Texto del asunto del correo</returns>
        private string ConstruirAsuntoCorreoGenerico(TipoAlertaBEL tipoAlerta)
        {
            return $"Torre de Control — {tipoAlerta.Nombre}";
        }

        /// <summary>
        /// Construye el cuerpo HTML genérico de correo (Área/Alerta/Origen/Fecha) usado cuando el sistema
        /// origen no envía un MensajeCorreo propio en el request
        /// </summary>
        /// <param name="tipoAlerta">Datos del tipo de alerta (área, nombre)</param>
        /// <param name="evento">Datos del evento recién insertado (origen, fecha)</param>
        /// <returns>Cuerpo HTML del correo a enviar</returns>
        private string ConstruirMensajeCorreoGenerico(TipoAlertaBEL tipoAlerta, EventoBEL evento)
        {
            var body = new StringBuilder();
            body.Append("<div style='font-family:Arial,Helvetica,sans-serif;color:#333333'>");
            body.Append("<h3 style='margin:0 0 8px'>Torre de Control</h3>");
            body.AppendFormat("<p><strong>Área:</strong> {0}</p>", tipoAlerta.Area);
            body.AppendFormat("<p><strong>Alerta:</strong> {0}</p>", tipoAlerta.Nombre);
            body.AppendFormat("<p><strong>Origen:</strong> {0}</p>", evento.OrigenSistema);
            body.AppendFormat("<p><strong>Fecha:</strong> {0}</p>", evento.FechaOcurrencia.ToString("dd/MM/yyyy HH:mm:ss"));
            body.Append("</div>");
            return body.ToString();
        }

        /// <summary>
        /// Envía el correo individualmente a cada responsable activo del tipo de alerta, solo si el tipo
        /// tiene habilitado el envío de correo a responsables (AlertaResponsableEmail) y tiene responsables
        /// con email asociado. Sin llamador activo todavía — ver comentario en IngresarAlerta.
        /// </summary>
        /// <param name="tipoAlerta">Datos del tipo de alerta (define si corresponde notificar a responsables)</param>
        /// <param name="asunto">Asunto ya armado del correo a enviar</param>
        /// <param name="cuerpoHtml">Cuerpo HTML ya armado del correo a enviar</param>
        private void NotificarResponsablesEmail(TipoAlertaBEL tipoAlerta, string asunto, string cuerpoHtml)
        {
            try
            {
                if (!tipoAlerta.AlertaResponsableEmail)
                    return;

                var responsables = this.alertaDAL.ObtenerResponsables(tipoAlerta.IdTipoAlerta);

                if (responsables == null || responsables.Count == 0)
                    return;

                foreach (var responsable in responsables)
                {
                    if (string.IsNullOrEmpty(responsable.Email))
                        continue;

                    try
                    {
                        this.emailSenderDAL.EnviarCorreo(responsable.Email, asunto, cuerpoHtml);
                    }
                    catch
                    {
                        // No interrumpir el envío al resto de responsables por la falla de uno
                    }
                }
            }
            catch
            {
                // No propagar excepciones de notificación — el insert ya se realizó
            }
        }

        #endregion

        #region IDisposable Implementation

        ~AlertaBLL()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Libera los recursos administrados utilizados por la instancia
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera los recursos administrados y no administrados según el parámetro disposing
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            this.alertaDAL?.Dispose();
            this.spWtspSendMessageDAL?.Dispose();
            this.registroLogEventoErrorDAL?.Dispose();
            this.emailSenderDAL?.Dispose();
        }

        #endregion
    }
}
