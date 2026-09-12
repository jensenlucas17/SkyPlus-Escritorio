namespace SkyPlus.Desktop.Models
{
    public class UsuarioResponse
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string EmailCorporativo { get; set; } = string.Empty;
        public string EstadoCuenta { get; set; } = string.Empty;
        public DateTime? UltimaSesion { get; set; }
        public string Rol { get; set; } = string.Empty;
    }
}