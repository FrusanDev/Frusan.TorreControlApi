using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace Utilitarios.InversionDeControl
{
    internal class RegistrarModulos : IRegistrarModulos
    {
        private readonly IUnityContainer _contenedor;

        public RegistrarModulos(IUnityContainer container)
        {
            this._contenedor = container;
        }

        public void RegistrarTipo<TOrigen, TDestino>(bool conIntercepcion = false) where TDestino : TOrigen
        {
            if (conIntercepcion)
                return;
            this._contenedor.RegisterType<TOrigen, TDestino>();
        }

        public void RegistrarTipoConVidaControlada<TOrigen, TDestino>(bool withInterception = false) where TDestino : TOrigen
        {
            this._contenedor.RegisterType<TOrigen, TDestino>(new ContainerControlledLifetimeManager(), new InjectionMember[0]);
        }
    }
}
