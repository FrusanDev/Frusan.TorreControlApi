using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TorreControl.DAL
{
    /// <summary>
    /// Adquiere y cachea el access token de Microsoft Graph (client credentials / app-only) usado para
    /// enviar correo por Outlook 365. Port mínimo de Utilitarios.Base.GraphAuthHelper (EmailInformativoApi);
    /// usa el mismo App Registration de mail (AzureAdMail:*).
    /// </summary>
    internal static class GraphAuthHelper
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly SemaphoreSlim tokenLock = new SemaphoreSlim(1, 1);
        private static string cachedToken;
        private static DateTime cachedTokenExpiraUtc = DateTime.MinValue;

        /// <summary>
        /// Devuelve el access token de Graph (app-only). Reutiliza el token cacheado mientras no venza
        /// y solo pide uno nuevo a Azure AD cuando expira (con 5 min de holgura)
        /// </summary>
        /// <returns>Access token listo para usar como Bearer</returns>
        public static async Task<string> ObtenerAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(cachedToken) && DateTime.UtcNow < cachedTokenExpiraUtc)
                return cachedToken;

            await tokenLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!string.IsNullOrEmpty(cachedToken) && DateTime.UtcNow < cachedTokenExpiraUtc)
                    return cachedToken;

                string tenantId = ConfigurationManager.AppSettings["AzureAdMail:TenantId"];
                string clientId = ConfigurationManager.AppSettings["AzureAdMail:ClientId"];
                string clientSecret = ConfigurationManager.AppSettings["AzureAdMail:ClientSecret"];

                if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                    throw new ConfigurationErrorsException("Faltan las claves AzureAdMail:TenantId / AzureAdMail:ClientId / AzureAdMail:ClientSecret en la configuración.");

                var parametros = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "scope", "https://graph.microsoft.com/.default" }
                };

                using (var contenido = new FormUrlEncodedContent(parametros))
                using (var respuesta = await httpClient.PostAsync(
                    string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", tenantId), contenido).ConfigureAwait(false))
                {
                    string json = await respuesta.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!respuesta.IsSuccessStatusCode)
                        throw new Exception(string.Format("No se pudo obtener el access token de Azure AD ({0}): {1}", (int)respuesta.StatusCode, json));

                    JObject obj = JObject.Parse(json);
                    cachedToken = obj["access_token"].ToString();
                    int expiraEnSegundos = obj["expires_in"] != null ? obj["expires_in"].Value<int>() : 3600;

                    // Renovar 5 minutos antes de que expire
                    cachedTokenExpiraUtc = DateTime.UtcNow.AddSeconds(Math.Max(expiraEnSegundos - 300, 60));

                    return cachedToken;
                }
            }
            finally
            {
                tokenLock.Release();
            }
        }
    }
}
