using System.ComponentModel.Composition;
using Utilitarios.InversionDeControl;

namespace TorreControl.BLL.Inicializador
{
    [Export(typeof(IModulo))]
    public class InicializadorModulo : IModulo
    {
        public void Initialize(IRegistrarModulos registrar)
        {
            registrar.RegistrarTipo<IAlertaBLL, AlertaBLL>(false);
        }
    }
}
