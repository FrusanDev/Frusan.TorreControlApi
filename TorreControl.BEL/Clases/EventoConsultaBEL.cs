using System;

namespace TorreControl.BEL
{
    /// <summary>
    /// Fila de resultado del endpoint de consulta GET /api/alertas — resumen de un evento
    /// para consumo de dashboards externos, distinto de EventoBEL (usado en el flujo de inserción)
    /// </summary>
    public class EventoConsultaBEL
    {
        public int IdEvento { get; set; }
        public string CodigoTipoAlerta { get; set; }
        public string NombreTipoAlerta { get; set; }
        public string Area { get; set; }
        public string Estado { get; set; }
        public string Severidad { get; set; }
        public string DescripcionBreve { get; set; }
        public string OrigenSistema { get; set; }
        public DateTime FechaOcurrencia { get; set; }
        public DateTime? FechaGestion { get; set; }
        public string AccionRespuesta { get; set; }
        public string QuienGestiono { get; set; }
        public string Responsables { get; set; }
    }
}
