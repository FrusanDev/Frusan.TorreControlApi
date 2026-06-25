namespace TorreControl.BEL
{
    public class ResponsableBEL
    {
        public int IdResponsable { get; set; }
        public int IdTipoAlerta { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public bool Activo { get; set; }
    }
}
