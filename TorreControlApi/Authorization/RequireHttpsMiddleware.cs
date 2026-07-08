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
        public RequireHttpsMiddleware(OwinMiddleware next) : base(next)
        {
        }

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
