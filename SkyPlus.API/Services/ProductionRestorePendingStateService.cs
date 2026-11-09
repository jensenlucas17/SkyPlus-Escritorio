using System.Text.Json;

namespace SkyPlus.API.Services;

public sealed class ProductionRestorePendingStateService
{
    private readonly string _stateFile;

    public ProductionRestorePendingStateService(
        IConfiguration configuration)
    {
        _stateFile = configuration[
            "RestoreAuthorization:ProductionStateFile"]
            ?? throw new InvalidOperationException(
                "No se configuró ProductionStateFile.");
    }

    public async Task ValidarPendienteAsync(
        Guid operationId,
        Guid backupId,
        CancellationToken cancellationToken = default)
    {
        if (operationId == Guid.Empty ||
            backupId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Los identificadores no son válidos.");
        }

        if (!File.Exists(_stateFile))
            throw new InvalidOperationException(
                "No existe un estado de producción pendiente.");

        await using var stream = new FileStream(
            _stateFile,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        using var documento =
            await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);

        var estado = documento.RootElement;

        Guid operacionRegistrada =
            estado.GetProperty("OperationId").GetGuid();

        Guid backupRegistrado =
            estado.GetProperty("BackupId").GetGuid();

        string? fase =
            estado.GetProperty("Phase").GetString();

        if (operacionRegistrada != operationId ||
            backupRegistrado != backupId)
        {
            throw new InvalidOperationException(
                "La solicitud no corresponde a la operación pendiente.");
        }

        if (fase != "PreventiveBackupVerified")
            throw new InvalidOperationException(
                "La operación no está preparada para continuar.");

        string? rutaPreventiva =
            estado.GetProperty("PreventiveBackupPath").GetString();

        string? sha256Preventivo =
            estado.GetProperty("PreventiveBackupSha256").GetString();

        if (string.IsNullOrWhiteSpace(rutaPreventiva) ||
            string.IsNullOrWhiteSpace(sha256Preventivo))
        {
            throw new InvalidOperationException(
                "El estado no contiene los datos obligatorios " +
                "del backup preventivo.");
        }

        string[] indicadores =
        {
            "ApplicationLockAttempted",
            "ImportStarted",
            "ImportCompleted",
            "VerificationCompleted",
            "ApplicationUnlocked"
        };

        foreach (string indicador in indicadores)
        {
            if (estado.GetProperty(indicador).GetBoolean())
            {
                throw new InvalidOperationException(
                    $"El estado contiene un indicador incompatible: {indicador}.");
            }
        }
    }
}