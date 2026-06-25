using System;
using TorreControl.BEL;

namespace TorreControl.BLL
{
    public interface IAlertaBLL : IDisposable
    {
        /// <summary>
        /// Valida el tipo de alerta, inserta el evento en la tabla central y notifica vía WhatsApp
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        int IngresarAlerta(IngresarAlertaRequest request);
    }
}
