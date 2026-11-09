using SkyPlus.RestoreWorker.Models;

namespace SkyPlus.RestoreWorker.Services;

public static class ProductionRestoreExecutionService
{
    public static async Task PrepararRegistroAsync(
        string rutaEstado,
        Guid idBackup)
    {
        // Debe ejecutarse con RestoreExecutionLock adquirido.
        await RestoreStateService.VerificarOperacionPendienteAsync(
            rutaEstado);

        var estado = new RestoreState
        {
            OperationId = Guid.NewGuid(),
            BackupId = idBackup,
            OperationType = "ProductionRestore",
            Phase = "Created",
            StartedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            ReadOnlyActivated = false,
            RestorationStarted = false,
            RestorationCompleted = false
        };

        await RestoreStateService.GuardarAsync(
            rutaEstado,
            estado);

        await RestoreStateService.ActualizarFaseAsync(
            rutaEstado,
            estado,
            "ProductionRestorePrepared");

        Console.WriteLine(
            "Operación de restauración preparada.");

        Console.WriteLine(
            "OperationId: " + estado.OperationId);

        Console.WriteLine(
            "BackupId: " + estado.BackupId);

        Console.WriteLine(
            "No se ejecutó ninguna importación SQL.");
    }
}
