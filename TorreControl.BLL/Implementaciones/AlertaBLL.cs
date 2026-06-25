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

        public AlertaBLL(IAlertaDAL alertaDAL, ISpWtspSendMessageDAL spWtspSendMessageDAL)
        {
            this.alertaDAL = alertaDAL;
            this.spWtspSendMessageDAL = spWtspSendMessageDAL;
        }

        #endregion

        #region IAlertaBLL Implementations

        public int IngresarAlerta(IngresarAlertaRequest request)
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
                OrigenSistema = request.OrigenSistema ?? "Desconocido"
            };

            int idEvento = this.alertaDAL.InsertarEvento(evento);

            NotificarWhatsApp(tipoAlerta, evento);

            return idEvento;
        }

        #endregion

        #region Métodos privados

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

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            this.alertaDAL?.Dispose();
            this.spWtspSendMessageDAL?.Dispose();
        }

        #endregion
    }
}
