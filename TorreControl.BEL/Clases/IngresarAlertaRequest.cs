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

        /// <summary>
        /// Texto exacto del mensaje WhatsApp a enviar, opcional. Lo arma el sistema origen con su propio
        /// formato de negocio (ej. resumen con emojis); si no se envía, se usa un mensaje genérico.
        /// Se usa tanto para el grupo Torre de Control como para el envío individual a responsables.
        /// </summary>
        public string MensajeWhatsapp { get; set; }
    }
}
