using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace TorreControl.DAL
{
    public class EmailSenderDAL : IEmailSenderDAL
    {
        #region IEmailSenderDAL Implementation

        /// <summary>
        /// Envía un correo HTML a un único destinatario mediante SMTP, usando las credenciales
        /// configuradas en Web.config (SmtpServer/SmtpPort/SmtpUserName/SmtpPassword)
        /// </summary>
        /// <param name="destinatario"></param>
        /// <param name="asunto"></param>
        /// <param name="cuerpoHtml"></param>
        public void EnviarCorreo(string destinatario, string asunto, string cuerpoHtml)
        {
            var smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
            var smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
            var smtpUserName = ConfigurationManager.AppSettings["SmtpUserName"];
            var smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];

            using (var mensaje = new MailMessage())
            {
                mensaje.From = new MailAddress(smtpUserName);
                mensaje.To.Add(destinatario);
                mensaje.Subject = asunto;
                mensaje.Body = cuerpoHtml;
                mensaje.IsBodyHtml = true;

                using (var cliente = new SmtpClient(smtpServer, smtpPort))
                {
                    cliente.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
                    cliente.EnableSsl = true;
                    cliente.Send(mensaje);
                }
            }
        }

        #endregion

        #region IDisposable Implementation

        ~EmailSenderDAL()
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
        }

        #endregion
    }
}
