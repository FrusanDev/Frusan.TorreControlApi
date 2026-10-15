using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TorreControl.DAL
{
    public class EmailSenderDAL : IEmailSenderDAL
    {
        private static readonly HttpClient httpClient = new HttpClient();

        #region IEmailSenderDAL Implementation

        /// <summary>
        /// Envía un correo HTML a un único destinatario por Microsoft Graph (Outlook 365), desde el buzón
        /// configurado en GraphMailRemitente y con el App Registration AzureAdMail:* de Web.config
        /// </summary>
        /// <param name="destinatario"></param>
        /// <param name="asunto"></param>
        /// <param name="cuerpoHtml"></param>
        public void EnviarCorreo(string destinatario, string asunto, string cuerpoHtml)
        {
            EnviarCorreoAsync(destinatario, asunto, cuerpoHtml).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Versión asíncrona del envío: valida remitente y destinatario, obtiene el token de Graph y
        /// hace el POST a sendMail. Lanza excepción si falta configuración o Graph responde con error
        /// </summary>
        /// <param name="destinatario">Correo del único destinatario</param>
        /// <param name="asunto">Asunto del correo</param>
        /// <param name="cuerpoHtml">Cuerpo del correo en HTML</param>
        /// <returns>Tarea que completa cuando Graph acepta el envío</returns>
        private static async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            var remitente = ConfigurationManager.AppSettings["GraphMailRemitente"];

            if (string.IsNullOrWhiteSpace(remitente))
                throw new ConfigurationErrorsException("Falta la clave GraphMailRemitente en la configuración.");

            if (string.IsNullOrWhiteSpace(destinatario))
                throw new ArgumentException("El destinatario es obligatorio.", "destinatario");

            string token = await GraphAuthHelper.ObtenerAccessTokenAsync().ConfigureAwait(false);

            var payload = new JObject
            {
                { "message", new JObject
                    {
                        { "subject", asunto },
                        { "body", new JObject { { "contentType", "HTML" }, { "content", cuerpoHtml } } },
                        { "toRecipients", new JArray(
                            new JObject { { "emailAddress", new JObject { { "address", destinatario } } } }) }
                    }
                },
                { "saveToSentItems", true }
            };

            var url = string.Format("https://graph.microsoft.com/v1.0/users/{0}/sendMail", Uri.EscapeDataString(remitente));

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

                using (var respuesta = await httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    if (!respuesta.IsSuccessStatusCode)
                    {
                        string detalle = await respuesta.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new Exception(string.Format("Error al enviar correo vía Microsoft Graph ({0}): {1}", (int)respuesta.StatusCode, detalle));
                    }
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
