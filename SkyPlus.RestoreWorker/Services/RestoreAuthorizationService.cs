using SkyPlus.RestoreWorker.Models;

namespace SkyPlus.RestoreWorker.Services;

public static class RestoreAuthorizationService
{
    public static void Validar(
        RestoreAuthorization autorizacion,
        Guid idBackup)
    {
        if (autorizacion.OperationId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "La autorización no tiene un identificador válido.");
        }

        if (autorizacion.BackupId != idBackup)
        {
            throw new InvalidOperationException(
                "La autorización no corresponde al backup seleccionado.");
        }

        if (autorizacion.FirstAdministratorId <= 0 ||
            autorizacion.SecondAdministratorId <= 0)
        {
            throw new InvalidOperationException(
                "Los administradores deben tener identificadores válidos.");
        }

        if (autorizacion.FirstAdministratorId ==
            autorizacion.SecondAdministratorId)
        {
            throw new InvalidOperationException(
                "La restauración requiere dos administradores distintos.");
        }

        if (autorizacion.ApprovedAtUtc > DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "La fecha de aprobación no es válida.");
        }

        if (autorizacion.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "La autorización está vencida.");
        }

        if (autorizacion.ExpiresAtUtc <=
            autorizacion.ApprovedAtUtc)
        {
            throw new InvalidOperationException(
                "El período de autorización es inválido.");
        }

        if (autorizacion.Consumed)
        {
            throw new InvalidOperationException(
                "Esta autorización ya fue utilizada.");
        }

        Console.WriteLine(
            "Los datos de autorización superaron las validaciones básicas.");
    }
}
