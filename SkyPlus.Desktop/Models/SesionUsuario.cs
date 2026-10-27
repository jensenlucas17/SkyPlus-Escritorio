namespace SkyPlus.Desktop.Models
{
    public static class SesionUsuario
    {
        public static int IdUsuario { get; set; }

        public static string Nombre { get; set; } = string.Empty;

        public static string Apellido { get; set; } = string.Empty;

        public static string Email { get; set; } = string.Empty;

        public static int IdRol { get; set; }

        public static string Rol { get; set; } = string.Empty;

        public static bool EstaLogueado =>
            IdUsuario > 0;
        public static string Token { get; set; } = string.Empty;

        public static DateTime ExpiracionToken { get; set; }

        public static void CerrarSesion()
        {
            IdUsuario = 0;
            Nombre = string.Empty;
            Apellido = string.Empty;
            Email = string.Empty;
            IdRol = 0;
            Rol = string.Empty;
            Token = string.Empty;
            ExpiracionToken = DateTime.MinValue;
        }

    }
}