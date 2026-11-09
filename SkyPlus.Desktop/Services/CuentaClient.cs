using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    // Operaciones del usuario sobre su propia cuenta
    public class CuentaClient
    {
        private readonly ApiClient _apiClient = new ApiClient();

        private class RespuestaMensaje
        {
            public string Mensaje { get; set; } = string.Empty;
        }

        public async Task<string> CambiarPasswordAsync(string actual, string nueva)
        {
            var respuesta = await _apiClient.PostAsync<object, RespuestaMensaje>(
                "api/Cuenta/cambiar-password",
                new { PasswordActual = actual, PasswordNueva = nueva });

            return respuesta?.Mensaje ?? "Contraseña actualizada correctamente.";
        }
    }
}
