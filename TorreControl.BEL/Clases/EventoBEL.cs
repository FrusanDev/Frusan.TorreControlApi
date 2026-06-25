using System;

namespace TorreControl.BEL
{
    public class EventoBEL
    {
        public int IdEvento { get; set; }
        public int IdTipoAlerta { get; set; }
        public string Payload { get; set; }
        public string Estado { get; set; }
        public string AccionRespuesta { get; set; }
        public string QuienGestiono { get; set; }
        public DateTime? FechaGestion { get; set; }
        public DateTime FechaOcurrencia { get; set; }
        public string OrigenSistema { get; set; }
    }
}
