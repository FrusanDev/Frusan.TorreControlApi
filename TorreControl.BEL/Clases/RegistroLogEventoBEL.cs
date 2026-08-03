using System;

namespace TorreControl.BEL
{
    public class RegistroLogEventoBEL
    {
        public DateTime FechaHora { get; set; }
        public string Servidor { get; set; }
        public string NombreTarea { get; set; }
        public string ErrorDescripcion { get; set; }
    }
}
