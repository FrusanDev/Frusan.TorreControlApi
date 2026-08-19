namespace TorreControl.BEL
{
    /// <summary>
    /// Resultado de IAlertaBLL.IngresarAlerta: ademas del Id, trae los datos minimos que el
    /// controller necesita para difundir la notificacion de alerta nueva (SignalR) sin tener que
    /// volver a consultar el tipo de alerta.
    /// </summary>
    public class AlertaRegistradaBEL
    {
        public int IdEvento { get; set; }
        public string Area { get; set; }
        public string CodigoTipoAlerta { get; set; }
        public string Severidad { get; set; }
        public string DescripcionBreve { get; set; }
    }
}
