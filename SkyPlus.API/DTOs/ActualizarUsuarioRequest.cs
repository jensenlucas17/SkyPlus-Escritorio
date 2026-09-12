namespace SkyPlus.API.DTOs
{
    public class ActualizarUsuarioRequest
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string EmailCorporativo { get; set; } = string.Empty;
        public string EstadoCuenta { get; set; } = string.Empty;
    }
}