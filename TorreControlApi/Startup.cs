using Microsoft.Owin;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Owin;
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
        }

        public static void RegisterTypes(IUnityContainer container)
        {
            CargadorModulos.CargarContenedor(container, ".\\bin", "TorreControl.*.Inicializador.dll");
        }
    }
}
