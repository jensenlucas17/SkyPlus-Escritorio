using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface IBackupService
    {
        Task<string> GenerarBackupAsync(
            int idAdministrador,
            CancellationToken cancellationToken = default);

        Task<List<BackupHistorialDto>> ObtenerHistorialAsync(
            CancellationToken cancellationToken = default);

        Task<bool> EliminarBackupAsync(
            Guid idBackup,
            int idAdministrador,
            CancellationToken cancellationToken = default);

        Task<BackupHistorialDto?> ObtenerBackupVerificadoAsync(
            Guid idBackup,
            CancellationToken cancellationToken = default);

        Task<ResultadoRestauracionDto> RestaurarPruebaAsync(
            Guid idBackup,
            int idAdministradorSolicitante,
            int idAdministradorAutorizante,
            CancellationToken cancellationToken = default);
    }
}