using System;
using System.Collections.Generic;
using TorreControl.BEL;

namespace TorreControl.DAL
{
    public interface IAlertaDAL : IDisposable
    {
        /// <summary>
        /// Obtiene el tipo de alerta desde TC_TipoAlerta según su código
        /// </summary>
        /// <param name="codigoTipoAlerta"></param>
        /// <returns></returns>
        TipoAlertaBEL ObtenerTipoAlerta(string codigoTipoAlerta);

        /// <summary>
        /// Inserta un nuevo evento de alerta en la tabla TC_Evento y retorna el ID generado
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        int InsertarEvento(EventoBEL evento);

        /// <summary>
        /// Obtiene la lista de responsables asignados a un tipo de alerta desde TC_Responsable
        /// </summary>
        /// <param name="idTipoAlerta"></param>
        /// <returns></returns>
        List<ResponsableBEL> ObtenerResponsables(int idTipoAlerta);

        /// <summary>
        /// Obtiene los eventos registrados en TC_Evento dentro de un rango de fechas y, opcionalmente,
        /// filtrados por estado. Si no se especifican fechas, el SP acota por defecto a los últimos 7 días
        /// </summary>
        /// <param name="fechaDesde"></param>
        /// <param name="fechaHasta"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        List<EventoConsultaBEL> ObtenerEventos(DateTime? fechaDesde, DateTime? fechaHasta, string estado);
    }
}
