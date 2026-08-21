using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Owin;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http;
using TorreControlApi.Authorization;
using Unity;
using Utilitarios.InversionDeControl;

[assembly: OwinStartup(typeof(TorreControlApi.Startup))]

namespace TorreControlApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use<RequireHttpsMiddleware>();

            IConfigurationSource configurationSource = ConfigurationSourceFactory.Create();

            LogWriterFactory logWriterFactory = new LogWriterFactory(configurationSource);
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(configurationSource));
            Logger.SetLogWriter(logWriterFactory.Create());

            ExceptionPolicyFactory exceptionFactory = new ExceptionPolicyFactory(configurationSource);
            ExceptionPolicy.SetExceptionManager(exceptionFactory.CreateManager());

            var container = new UnityContainer();
            RegisterTypes(container);
            UnityConfig.RegisterComponents(container);

            // Hub de notificacion de alertas nuevas para la Torre de Control (FrusanNet corre en
            // https://extranet.frusan.cl, otro origen/dominio). Difunde idEvento/area/codigo/
            // severidad, sin datos sensibles del payload. CORS restringido a ese origen exacto (no
            // AllowAll). Rama OWIN aparte, no pasa por ApiKeyAuthHandler (ese handler esta
            // registrado solo en el pipeline de Web API, ver WebApiConfig.Register).
            // SupportsCredentials=true: el cliente jQuery.signalR manda la negociacion cross-domain
            // con withCredentials=true (cookies incluidas) - sin esto el browser bloquea la respuesta
            // con "Access-Control-Allow-Credentials must be 'true'" (bug real 2026-08-19, probado en
            // extranet.frusan.cl). Con SupportsCredentials=true, AllowAnyOrigin/"*" no es valido -
            // por eso el origen debe ser exacto (ya lo era).
            var corsPolicyExtranet = new CorsPolicy { AllowAnyMethod = true, AllowAnyHeader = true, SupportsCredentials = true };
            corsPolicyExtranet.Origins.Add("https://extranet.frusan.cl");

            app.Map("/signalr", signalr =>
            {
                signalr.UseCors(new CorsOptions
                {
                    PolicyProvider = new CorsPolicyProvider
                    {
                        PolicyResolver = context => Task.FromResult(corsPolicyExtranet)
                    }
                });
                signalr.RunSignalR();
            });
        }

        public static void RegisterTypes(IUnityContainer container)
        {
            CargadorModulos.CargarContenedor(container, ".\\bin", "TorreControl.*.Inicializador.dll");
        }
    }
}
