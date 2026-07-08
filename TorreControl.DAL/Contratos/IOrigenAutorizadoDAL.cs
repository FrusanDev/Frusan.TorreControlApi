using System;
using TorreControl.BEL;

namespace TorreControl.DAL
{
    public interface IOrigenAutorizadoDAL : IDisposable
    {
        /// <summary>
        /// Valida una API Key en texto plano contra los orígenes autorizados en TC_OrigenAutorizado
        /// </summary>
        /// <param name="apiKeyPlano"></param>
        /// <returns></returns>
        OrigenAutorizadoBEL ValidarApiKey(string apiKeyPlano);
    }
}
