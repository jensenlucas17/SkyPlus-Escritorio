
using SkyPlus.RestoreWorker.Models;

namespace SkyPlus.RestoreWorker.Services;

public static class ReadOnlyTestService
{
    public static async Task EjecutarAsync(
        string archivoCredenciales,
        string rutaEstado,
        Guid idBackup)
    {
        // Debe ejecutarse con RestoreExecutionLock adquirido.
        await RestoreStateService.VerificarOperacionPendienteAsync(
            rutaEstado);

        bool estadoInicial =
            await MariaDbReadOnlyService.ObtenerEstadoAsync(
                archivoCredenciales);

        if (estadoInicial)
        {
            throw new InvalidOperationException(
                "MariaDB ya tiene read_only = ON. " +
                "No se iniciará la prueba.");
        }

        var estado = new RestoreState
        {
            OperationId = Guid.NewGuid(),
            BackupId = idBackup,
            OperationType = "ReadOnlyTest",
            Phase = "Created",
            ReadOnlyActivated = false,
            RestorationStarted = false,
            RestorationCompleted = false
        };

        await RestoreStateService.GuardarAsync(
            rutaEstado, estado);

        try
        {
            // Registrar intención antes de modificar MariaDB.
            await RestoreStateService.ActualizarFaseAsync(
                rutaEstado, estado, "EnablingReadOnly");

            await MariaDbReadOnlyService.ActivarAsync(
                archivoCredenciales);

            estado.ReadOnlyActivated = true;

            await RestoreStateService.ActualizarFaseAsync(
                rutaEstado, estado, "ReadOnlyEnabled");

            bool activado =
                await MariaDbReadOnlyService.ObtenerEstadoAsync(
                    archivoCredenciales);

            if (!activado)
            {
                throw new InvalidOperationException(
                    "No se pudo confirmar read_only = ON.");
            }

            Console.WriteLine(
                "Prueba: read_only activado correctamente.");

            await RestoreStateService.ActualizarFaseAsync(
                rutaEstado, estado, "DisablingReadOnly");

            await MariaDbReadOnlyService.DesactivarAsync(
                archivoCredenciales);

            bool sigueActivo =
                await MariaDbReadOnlyService.ObtenerEstadoAsync(
                    archivoCredenciales);

            if (sigueActivo)
            {
                throw new InvalidOperationException(
                    "MariaDB continúa en modo solo lectura.");
            }

            estado.ReadOnlyActivated = false;

            await RestoreStateService.ActualizarFaseAsync(
                rutaEstado, estado, "ReadOnlyTestCompleted");

            Console.WriteLine(
                "Prueba finalizada: read_only desactivado.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                "Error durante la prueba: " + ex.Message);

            estado.Error = ex.Message;

            // Intentar recuperar MariaDB incluso si falló
            // la activación o la actualización del estado.
            try
            {
                bool activo =
                    await MariaDbReadOnlyService.ObtenerEstadoAsync(
                        archivoCredenciales);

                if (activo)
                {
                    Console.WriteLine(
                        "Intentando desactivar read_only...");

                    await MariaDbReadOnlyService.DesactivarAsync(
                        archivoCredenciales);
                }

                bool sigueActivo =
                    await MariaDbReadOnlyService.ObtenerEstadoAsync(
                        archivoCredenciales);

                if (sigueActivo)
                {
                    throw new InvalidOperationException(
                        "No se pudo desactivar read_only.");
                }

                estado.ReadOnlyActivated = false;

                await RestoreStateService.ActualizarFaseAsync(
                    rutaEstado, estado, "ReadOnlyTestFailedRecovered");

                Console.WriteLine(
                    "MariaDB recuperada: read_only = OFF.");
            }
            catch (Exception errorRecuperacion)
            {
                estado.Error =
                    ex.Message + " | Recuperación: " +
                    errorRecuperacion.Message;

                try
                {
                    await RestoreStateService.ActualizarFaseAsync(
                        rutaEstado, estado,
                        "ReadOnlyRecoveryRequired");
                }
                catch (Exception errorRegistro)
                {
                    Console.Error.WriteLine(
                        "No se pudo registrar la recuperación: " +
                        errorRegistro.Message);
                }

                Console.Error.WriteLine(
                    "ATENCIÓN: se requiere recuperación administrativa. " +
                    "No reanudar SkyPlus.");
            }

            throw;
        }
    }
}
