namespace SkyPlus.Domain.Entities
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        public int IdRol { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string EmailCorporativo { get; set; } = string.Empty;

        public string ContrasenaHash { get; set; } = string.Empty;

        public string EstadoCuenta { get; set; } = string.Empty;

        public DateTime? UltimaSesion { get; set; }

        // Relación con Rol
        public Rol? Rol { get; set; }
    }
}