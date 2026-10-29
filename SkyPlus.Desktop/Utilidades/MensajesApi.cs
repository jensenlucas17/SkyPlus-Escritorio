using System.Net;

namespace SkyPlus.Desktop.Utilidades
{
    public static class MensajesApi
    {
        public static void MostrarError(HttpRequestException ex)
        {
            string mensaje = ex.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Tu sesión no es válida. Volvé a iniciar sesión.",
                HttpStatusCode.Forbidden => "Tu rol no tiene permiso para realizar esta acción.",
                null => "No se pudo conectar con el servidor. Verificá que la API esté en ejecución.",
                _ => ex.Message
            };

            MessageBox.Show(mensaje, "SkyPlus", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}