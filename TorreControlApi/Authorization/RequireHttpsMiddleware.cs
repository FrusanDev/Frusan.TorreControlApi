using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace TorreControlApi.Authorization
{
    /// <summary>
    /// Rechaza cualquier request que no venga por HTTPS, salvo loopback (IIS Express / desarrollo local)
    /// </summary>
    public class RequireHttpsMiddleware : OwinMiddleware
    {
        /// <summary>
        /// Constructor requerido por OwinMiddleware para encadenar este middleware con el siguiente en el pipeline OWIN
        /// </summary>
        /// <param name="next">Siguiente middleware en el pipeline</param>
        public RequireHttpsMiddleware(OwinMiddleware next) : base(next)
        {
        }

        /// <summary>
        /// Evalúa cada request OWIN entrante: si no es HTTPS y no es loopback, corta la ejecución con 403.
        /// En caso contrario, deja pasar el request al siguiente middleware del pipeline.
        /// </summary>
        /// <param name="context">Contexto OWIN del request entrante</param>
        public override async Task Invoke(IOwinContext context)
        {
            bool esHttps = context.Request.Scheme == Uri.UriSchemeHttps;
            bool esLoopbackLocal = context.Request.Uri.IsLoopback;

            if (!esHttps && !esLoopbackLocal)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("HTTPS es requerido.");
                return;
            }

            await Next.Invoke(context);
        }
    }
}
