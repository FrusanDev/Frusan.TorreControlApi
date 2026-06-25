namespace TorreControl.BEL
{
    public class IngresarAlertaRequest
    {
        public string CodigoTipoAlerta { get; set; }
        public string OrigenSistema { get; set; }
        public object Payload { get; set; }
    }
}
