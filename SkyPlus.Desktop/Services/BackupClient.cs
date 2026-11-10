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

        // Administrador y Gerente.
        public async Task<List<BackupResponse>?> ObtenerHistorialAsync()
        {
            return await _apiClient.GetAsync<List<BackupResponse>>("api/backups");
        }

        // Solo Administrador.
        public async Task<GenerarBackupResponse?> GenerarBackupAsync()
        {
            return await _apiClient.PostAsync<object, GenerarBackupResponse>(
                "api/backups", new { });
        }

        // Solo Administrador.
        public async Task EliminarBackupAsync(Guid idBackup)
        {
            await _apiClient.DeleteAsync($"api/backups/{idBackup}");
        }

        // Administrador + segundo Administrador. Solo base temporal.
        public async Task<RestaurarBackupResponse?> RestaurarPruebaAsync(
            Guid idBackup,
            string emailSegundoAdministrador,
            string passwordSegundoAdministrador)
        {
            var request = new
            {
                EmailSegundoAdministrador = emailSegundoAdministrador,
                PasswordSegundoAdministrador = passwordSegundoAdministrador,
                ConfirmarRestauracion = true
            };

            return await _apiClient.PostAsync<object, RestaurarBackupResponse>(
                $"api/backups/{idBackup}/restaurar-prueba", request);
        }

        // Emite autorización firmada para producción; NO ejecuta la restauración.
        // El backup debe seleccionarse en Desktop y validarse después en RestoreWorker.
        public async Task<AutorizacionProduccionResponse?> AutorizarRestauracionProduccionAsync(
            Guid idBackup,
            string emailSegundoAdministrador,
            string passwordSegundoAdministrador)
        {
            if (idBackup == Guid.Empty)
                throw new ArgumentException("Debe seleccionarse un backup válido.", nameof(idBackup));

            if (string.IsNullOrWhiteSpace(emailSegundoAdministrador) ||
                string.IsNullOrWhiteSpace(passwordSegundoAdministrador))
                throw new ArgumentException("Se requieren las credenciales del segundo administrador.");

            var request = new
            {
                BackupId = idBackup,
                EmailSegundoAdministrador = emailSegundoAdministrador.Trim(),
                PasswordSegundoAdministrador = passwordSegundoAdministrador,
                ConfirmarRestauracion = true
            };

            return await _apiClient.PostAsync<object, AutorizacionProduccionResponse>(
                "api/backups/autorizar-restauracion-produccion", request);
        }


        public async Task<AutorizacionProduccionResponse?>
            AutorizarContinuacionProduccionAsync(
                Guid operationId,
                Guid backupId,
                string emailSegundoAdministrador,
                string passwordSegundoAdministrador)
        {
            var request = new
            {
                OperationId = operationId,
                BackupId = backupId,
                ConfirmarRestauracion = true,
                EmailSegundoAdministrador = emailSegundoAdministrador,
                PasswordSegundoAdministrador = passwordSegundoAdministrador
            };

            return await _apiClient.PostAsync<object,
                AutorizacionProduccionResponse>(
                "api/backups/autorizar-continuacion-produccion",
                request);
        }
    }
}
