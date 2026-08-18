namespace TorreControl.BEL
{
    public class TipoAlertaBEL
    {
        public int IdTipoAlerta { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Area { get; set; }
        public bool Activo { get; set; }
        public bool PublicaGrupoWzap { get; set; }
        public bool AlertaResponsableWzap { get; set; }
        public bool AlertaResponsableEmail { get; set; }
    }
}
