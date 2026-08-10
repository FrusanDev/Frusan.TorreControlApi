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

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor que recibe las dependencias DAL mediante inyección de Unity
        /// </summary>
        /// <param name="alertaDAL"></param>
        /// <param name="spWtspSendMessageDAL"></param>
        /// <param name="registroLogEventoErrorDAL"></param>
        public AlertaBLL(IAlertaDAL alertaDAL, ISpWtspSendMessageDAL spWtspSendMessageDAL, IRegistroLogEventoErrorDAL registroLogEventoErrorDAL)
        {
            this.alertaDAL = alertaDAL;
            this.spWtspSendMessageDAL = spWtspSendMessageDAL;
            this.registroLogEventoErrorDAL = registroLogEventoErrorDAL;
        }

        #endregion

        #region IAlertaBLL Implementations

        /// <summary>
        /// Valida el tipo de alerta, inserta el evento en la tabla central TC_Evento y dispara la notificación WhatsApp
        /// al grupo Torre de Control y, si corresponde, a los responsables del tipo de alerta
        /// </summary>
        /// <param name="request">DTO con código de tipo de alerta y payload JSON</param>
        /// <param name="origenAutenticado">Código del sistema origen autenticado por API Key; es el único valor de confianza para OrigenSistema</param>
        /// <returns>ID del evento generado en TC_Evento</returns>
        public int IngresarAlerta(IngresarAlertaRequest request, string origenAutenticado)
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
                    Estado = "Pendiente",
                    FechaOcurrencia = DateTime.Now,
                    OrigenSistema = origenAutenticado,
                    Severidad = request.Severidad,
                    DescripcionBreve = request.DescripcionBreve,
                    MensajeWhatsapp = request.MensajeWhatsapp
                };

                int idEvento = this.alertaDAL.InsertarEvento(evento);

                string mensajeWhatsapp = !string.IsNullOrWhiteSpace(evento.MensajeWhatsapp)
                    ? evento.MensajeWhatsapp
                    : ConstruirMensajeGenerico(tipoAlerta, evento);

                NotificarGrupo(tipoAlerta, mensajeWhatsapp);
                NotificarResponsables(tipoAlerta, mensajeWhatsapp);

                return idEvento;
            }
            catch (Exception ex)
            {
                RegistrarLogError(origenAutenticado, ex.Message, notificarEmergencia: true);
                throw;
            }
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
        }

        #endregion
    }
}
