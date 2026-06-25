using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilitarios.InversionDeControl
{
    public interface IModulo
    {
        void Initialize(IRegistrarModulos registrar);
    }
}
