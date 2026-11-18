using System;

namespace TorreControl.DAL
{
    public interface ISpWtspSendMessageDAL : IDisposable
    {
        void EnviarMensajeWhatsapp(string grupoId, string body);

        /// <summary>
        /// Envía un mensaje de WhatsApp indicando instancia y token (SP SpWtspSendMessageEx)
        /// </summary>
        void EnviarMensajeWhatsappEx(string instancia, string token, string grupoId, string body);
    }
}
