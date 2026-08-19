using System;
using System.Collections.Generic;
using TorreControl.BEL;

namespace TorreControl.BLL
{
    public interface IAlertaBLL : IDisposable
    {
        /// <summary>
        /// Valida el tipo de alerta, inserta el evento en la tabla central y notifica vía WhatsApp
        /// </summary>
        /// <param name="request"></param>
        /// <param name="origenAutenticado">Código del sistema origen autenticado por API Key (ApiKeyAuthHandler); reemplaza cualquier origen declarado en el payload</param>
        /// <returns>Id del evento generado más los datos mínimos (área/código/severidad) que el controller usa para difundir la notificación de alerta nueva</returns>
        AlertaRegistradaBEL IngresarAlerta(IngresarAlertaRequest request, string origenAutenticado);

        /// <summary>
        /// Obtiene los eventos registrados dentro de un rango de fechas y, opcionalmente, filtrados por estado.
        /// Si no se especifican fechas, se acota por defecto a los últimos 7 días
        /// </summary>
        /// <param name="fechaDesde"></param>
        /// <param name="fechaHasta"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        List<EventoConsultaBEL> ObtenerEventos(DateTime? fechaDesde, DateTime? fechaHasta, string estado);
    }
}
