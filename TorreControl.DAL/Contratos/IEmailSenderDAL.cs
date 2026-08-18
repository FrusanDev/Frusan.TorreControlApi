using System;

namespace TorreControl.DAL
{
    /// <summary>
    /// Envío de correo electrónico HTML a un único destinatario vía SMTP. Usado por AlertaBLL para
    /// notificar a los responsables de un tipo de alerta (análogo a ISpWtspSendMessageDAL para WhatsApp).
    /// </summary>
    public interface IEmailSenderDAL : IDisposable
    {
        /// <summary>
        /// Envía un correo HTML a un único destinatario
        /// </summary>
        /// <param name="destinatario"></param>
        /// <param name="asunto"></param>
        /// <param name="cuerpoHtml"></param>
        void EnviarCorreo(string destinatario, string asunto, string cuerpoHtml);
    }
}
