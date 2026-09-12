namespace SkyPlus.API.DTOs
{
    public class CrearUsuarioRequest
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string EmailCorporativo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}