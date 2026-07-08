using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using TorreControl.DAL;

namespace TorreControlApi.Authorization
{
    /// <summary>
    /// Valida el header X-Api-Key contra TC_OrigenAutorizado antes de que el request llegue al controller.
    /// Deja el código del origen autenticado en Request.Properties para que el controller lo use como
    /// OrigenSistema real del evento, en vez de confiar en lo que declare el payload.
    /// </summary>
    public class ApiKeyAuthHandler : DelegatingHandler
    {
        private const string HeaderName = "X-Api-Key";
        public const string OrigenAutenticadoKey = "TC_OrigenAutenticado";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IEnumerable<string> valores;
            if (!request.Headers.TryGetValues(HeaderName, out valores) || string.IsNullOrWhiteSpace(valores.FirstOrDefault()))
            {
                RegistrarIntentoFallido(request, "Header X-Api-Key ausente o vacío");
                return request.CreateResponse(HttpStatusCode.Unauthorized, new { mensaje = "Header X-Api-Key es requerido." });
            }

            string apiKey = valores.First();
            IOrigenAutorizadoDAL origenDAL = null;

            try
            {
                var scope = request.GetDependencyScope();
                origenDAL = (IOrigenAutorizadoDAL)scope.GetService(typeof(IOrigenAutorizadoDAL));
                var origen = origenDAL.ValidarApiKey(apiKey);

                if (origen == null || !origen.Activo)
                {
                    RegistrarIntentoFallido(request, "API Key inválida o inactiva (" + Prefijo(apiKey) + "...)");
                    return request.CreateResponse(HttpStatusCode.Unauthorized, new { mensaje = "API Key inválida o inactiva." });
                }

                request.Properties[OrigenAutenticadoKey] = origen.Codigo;
            }
            finally
            {
                origenDAL?.Dispose();
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private static string Prefijo(string apiKey)
        {
            return apiKey.Length <= 6 ? apiKey : apiKey.Substring(0, 6);
        }

        private void RegistrarIntentoFallido(HttpRequestMessage request, string motivo)
        {
            var logEntry = new LogEntry
            {
                Message = $"Intento de acceso no autorizado a TorreControlApi. Motivo: {motivo}. Path: {request.RequestUri.AbsolutePath}",
                Categories = { "TorreControl" },
                Severity = TraceEventType.Warning,
                Title = "TorreControlApi - Auth Fallida"
            };
            Logger.Write(logEntry);
        }
    }
}
