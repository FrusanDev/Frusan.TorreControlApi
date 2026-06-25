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
    }
}
