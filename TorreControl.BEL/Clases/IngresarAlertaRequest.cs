namespace TorreControl.BEL
{
    public class IngresarAlertaRequest
    {
        public string CodigoTipoAlerta { get; set; }
        public object Payload { get; set; }

        /// <summary>
        /// Severidad del evento ("Critica" | "Alta" | "Media" | "Baja"), opcional. La calcula el sistema
        /// origen según su propia regla de desviación; si no se envía, queda sin clasificar.
        /// </summary>
        public string Severidad { get; set; }

        /// <summary>
        /// Descripción breve y legible del evento, opcional. La arma el sistema origen para
        /// mostrarla en la grilla y en las notificaciones sin tener que parsear el Payload.
        /// </summary>
        public string DescripcionBreve { get; set; }
    }
}
