using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class BackupClient
    {
        private readonly ApiClient _apiClient;

        public BackupClient()
        {
            _apiClient = new ApiClient();
        }

        // =====================================
        // CONSULTAR HISTORIAL
        // Administrador y Gerente
        // =====================================

        public async Task<List<BackupResponse>?> ObtenerHistorialAsync()
        {
            return await _apiClient.GetAsync<List<BackupResponse>>(
                "api/backups");
        }

        // =====================================
        // GENERAR BACKUP
        // Solo Administrador
        // =====================================

        public async Task<GenerarBackupResponse?> GenerarBackupAsync()
        {
            return await _apiClient.PostAsync<object, GenerarBackupResponse>(
                "api/backups",
                new { });
        }

        // =====================================
        // ELIMINAR BACKUP
        // Solo Administrador
        // =====================================

        public async Task EliminarBackupAsync(Guid idBackup)
        {
            await _apiClient.DeleteAsync(
                $"api/backups/{idBackup}");
        }

        // =====================================
        // RESTAURAR EN BASE DE PRUEBA
        // Administrador + segundo Administrador
        // =====================================

        public async Task<RestaurarBackupResponse?> RestaurarPruebaAsync(
            Guid idBackup,
            string emailSegundoAdministrador,
            string passwordSegundoAdministrador)
        {
            var request = new
            {
                EmailSegundoAdministrador =
                    emailSegundoAdministrador,

                PasswordSegundoAdministrador =
                    passwordSegundoAdministrador,

                ConfirmarRestauracion = true
            };

            return await _apiClient.PostAsync<object, RestaurarBackupResponse>(
                $"api/backups/{idBackup}/restaurar-prueba",
                request);
        }
    }
}
