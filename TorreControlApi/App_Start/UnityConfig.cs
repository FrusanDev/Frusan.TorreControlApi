using System.Web.Http;
using Unity;
using Unity.WebApi;

namespace TorreControlApi
{
    public static class UnityConfig
    {
        public static void RegisterComponents(IUnityContainer container)
        {
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}
