using System.ComponentModel.Composition;
using TorreControl.DAL;
using Utilitarios.InversionDeControl;

namespace TorreControl.DAL.Inicializador
{
    [Export(typeof(IModulo))]
    public class InicializadorModulo : IModulo
    {
        public void Initialize(IRegistrarModulos registrar)
        {
            registrar.RegistrarTipo<IAlertaDAL, AlertaDAL>(false);
            registrar.RegistrarTipo<ISpWtspSendMessageDAL, SpWtspSendMessageDAL>(false);
            registrar.RegistrarTipo<IOrigenAutorizadoDAL, OrigenAutorizadoDAL>(false);
        }
    }
}
