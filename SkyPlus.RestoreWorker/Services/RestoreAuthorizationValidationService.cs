using SkyPlus.Domain.Backup;

namespace SkyPlus.RestoreWorker.Services;

public static class RestoreAuthorizationValidationService
{
    public static RestoreAuthorizationPayload Validar(
        string documentoFirmado,
        string rutaClavePublica,
        Guid idBackup)
    {
        return ValidarInternamente(
            documentoFirmado,
            rutaClavePublica,
            idBackup,
            "ProductionRestore");
    }

    public static RestoreAuthorizationPayload ValidarContinuacion(
        string documentoFirmado,
        string rutaClavePublica,
        Guid idBackup,
        Guid operationId)
    {
        if (operationId == Guid.Empty)
            throw new InvalidOperationException(
                "El identificador de continuación es inválido.");

        var autorizacion = ValidarInternamente(
            documentoFirmado,
            rutaClavePublica,
            idBackup,
            "ProductionRestoreContinuation");

        if (autorizacion.OperationId != operationId)
            throw new InvalidOperationException(
                "La autorización no corresponde a la operación pendiente.");

        return autorizacion;
    }

    private static RestoreAuthorizationPayload ValidarInternamente(
        string documentoFirmado,
        string rutaClavePublica,
        Guid idBackup,
        string propositoEsperado)
    {
        var autorizacion =
            RestoreSignatureVerificationService.Verificar(
                documentoFirmado,
                rutaClavePublica);

        DateTime ahora = DateTime.UtcNow;

        if (autorizacion.OperationId == Guid.Empty)
            throw new InvalidOperationException(
                "El identificador de operación no es válido.");

        if (idBackup == Guid.Empty ||
            autorizacion.BackupId != idBackup)
            throw new InvalidOperationException(
                "La autorización no corresponde al backup.");

        if (!string.Equals(
            autorizacion.Purpose,
            propositoEsperado,
            StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "El propósito de la autorización no corresponde " +
                "a la operación solicitada.");
        }

        if (autorizacion.TargetDatabase != "skyplus")
            throw new InvalidOperationException(
                "La base de datos autorizada no es skyplus.");

        if (autorizacion.FirstAdministratorId <= 0 ||
            autorizacion.SecondAdministratorId <= 0 ||
            autorizacion.FirstAdministratorId ==
                autorizacion.SecondAdministratorId)
        {
            throw new InvalidOperationException(
                "Los administradores autorizantes no son válidos.");
        }

        if (autorizacion.ApprovedAtUtc.Kind != DateTimeKind.Utc ||
            autorizacion.ExpiresAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException(
                "Las fechas de autorización deben estar en UTC.");
        }

        if (autorizacion.ApprovedAtUtc > ahora)
            throw new InvalidOperationException(
                "La autorización tiene una fecha futura.");

        if (autorizacion.ExpiresAtUtc <= ahora)
            throw new InvalidOperationException(
                "La autorización está vencida.");

        if (autorizacion.ExpiresAtUtc <=
            autorizacion.ApprovedAtUtc)
        {
            throw new InvalidOperationException(
                "El período de autorización es inválido.");
        }

        if (autorizacion.ExpiresAtUtc -
            autorizacion.ApprovedAtUtc >
            TimeSpan.FromMinutes(10))
        {
            throw new InvalidOperationException(
                "La autorización supera la vigencia permitida.");
        }

        Console.WriteLine(
            "Firma RSA y datos de autorización válidos.");

        Console.WriteLine(
            $"Operación autorizada: {autorizacion.OperationId}");

        return autorizacion;
    }
}