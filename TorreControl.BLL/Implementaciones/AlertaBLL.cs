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

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor que recibe las dependencias DAL mediante inyección de Unity
        /// </summary>
        /// <param name="alertaDAL"></param>
        /// <param name="spWtspSendMessageDAL"></param>
        public AlertaBLL(IAlertaDAL alertaDAL, ISpWtspSendMessageDAL spWtspSendMessageDAL)
        {
            this.alertaDAL = alertaDAL;
            this.spWtspSendMessageDAL = spWtspSendMessageDAL;
        }

        #endregion

        #region IAlertaBLL Implementations

        /// <summary>
        /// Valida el tipo de alerta, inserta el evento en la tabla central TC_Evento y dispara la notificación WhatsApp al grupo Torre de Control
        /// </summary>
        /// <param name="request">DTO con código de tipo de alerta y payload JSON</param>
        /// <param name="origenAutenticado">Código del sistema origen autenticado por API Key; es el único valor de confianza para OrigenSistema</param>
        /// <returns>ID del evento generado en TC_Evento</returns>
        public int IngresarAlerta(IngresarAlertaRequest request, string origenAutenticado)
        {
            var tipoAlerta = this.alertaDAL.ObtenerTipoAlerta(request.CodigoTipoAlerta);
            if (tipoAlerta == null || !tipoAlerta.Activo)
                throw new Exception($"Tipo de alerta '{request.CodigoTipoAlerta}' no existe o no está activo.");

            string payloadJson = JsonConvert.SerializeObject(request.Payload);

            var evento = new EventoBEL
            {
                IdTipoAlerta = tipoAlerta.IdTipoAlerta,
                Payload = payloadJson,
                Estado = "Pendiente",
                FechaOcurrencia = DateTime.Now,
                OrigenSistema = origenAutenticado
            };

            int idEvento = this.alertaDAL.InsertarEvento(evento);

            //NotificarWhatsApp(tipoAlerta, evento);

            return idEvento;
        }

        #endregion

        #region Métodos privados

        /// <summary>
        /// Construye el mensaje de alerta y lo envía al grupo WhatsApp Torre de Control mediante el SP SpWtspSendMessage
        /// </summary>
        /// <param name="tipoAlerta">Datos del tipo de alerta (área, nombre)</param>
        /// <param name="evento">Datos del evento recién insertado (origen, fecha)</param>
        private void NotificarWhatsApp(TipoAlertaBEL tipoAlerta, EventoBEL evento)
        {
            try
            {
                var grupoId = ConfigurationManager.AppSettings["grupoIdWhatsappGrupoTorreControl"];

                if (string.IsNullOrEmpty(grupoId))
                    return;

                var body = new StringBuilder();
                body.AppendFormat("*Torre de Control*{0}", Environment.NewLine);
                body.AppendFormat("Área: {0}{1}", tipoAlerta.Area, Environment.NewLine);
                body.AppendFormat("Alerta: {0}{1}", tipoAlerta.Nombre, Environment.NewLine);
                body.AppendFormat("Origen: {0}{1}", evento.OrigenSistema, Environment.NewLine);
                body.AppendFormat("Fecha: {0}", evento.FechaOcurrencia.ToString("dd/MM/yyyy HH:mm:ss"));

                this.spWtspSendMessageDAL.EnviarMensajeWhatsapp(grupoId, body.ToString());
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
        }

        #endregion
    }
}
