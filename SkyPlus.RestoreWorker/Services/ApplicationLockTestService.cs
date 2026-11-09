
namespace SkyPlus.RestoreWorker.Services;

public static class ApplicationLockTestService
{
    public static async Task EjecutarAsync(
        string mariaDbBinPath,
        string credentialsFile,
        string stateFile)
    {
        Console.WriteLine();
        Console.WriteLine(
            "=== PRUEBA CONTROLADA DE BLOQUEO ===");

        Console.WriteLine(
            "Cuenta: skyplus_app@localhost");

        bool intentoRegistrado = false;
        Exception? errorPrincipal = null;

        // Verificar el estado inicial antes de modificar la cuenta.
        bool bloqueadaInicialmente =
            await MariaDbApplicationLockService.EstaBloqueadaAsync(
                mariaDbBinPath,
                credentialsFile);

        if (bloqueadaInicialmente)
        {
            throw new InvalidOperationException(
                "La cuenta skyplus_app ya estaba bloqueada. " +
                "No se ejecutará la prueba para evitar " +
                "desbloquear una cuenta bloqueada previamente.");
        }

        Console.WriteLine(
            "Estado inicial verificado: cuenta desbloqueada.");

        try
        {
            // Registrar el intento ANTES de bloquear.
            await ApplicationLockRecoveryService
                .RegistrarIntentoAsync(stateFile);

            intentoRegistrado = true;

            Console.WriteLine(
                "Intento de bloqueo registrado.");

            Console.WriteLine(
                "Bloqueando cuenta de aplicación...");

            await MariaDbApplicationLockService.BloquearAsync(
                mariaDbBinPath,
                credentialsFile);

            Console.WriteLine(
                "Comando de bloqueo ejecutado.");

            bool estaBloqueada =
                await MariaDbApplicationLockService.EstaBloqueadaAsync(
                    mariaDbBinPath,
                    credentialsFile);

            if (!estaBloqueada)
            {
                throw new InvalidOperationException(
                    "La cuenta no quedó bloqueada.");
            }

            Console.WriteLine(
                "Bloqueo verificado correctamente.");
        }
        catch (Exception ex)
        {
            errorPrincipal = ex;

            Console.Error.WriteLine(
                "Error durante el bloqueo: " + ex.Message);
        }
        finally
        {
            if (intentoRegistrado)
            {
                try
                {
                    Console.WriteLine(
                        "Desbloqueando cuenta de aplicación...");

                    await MariaDbApplicationLockService.DesbloquearAsync(
                        mariaDbBinPath,
                        credentialsFile);

                    Console.WriteLine(
                        "Comando de desbloqueo ejecutado.");

                    await ApplicationLockRecoveryService
                        .VerificarDesbloqueoYRegistrarAsync(
                            stateFile,
                            mariaDbBinPath,
                            credentialsFile);

                    Console.WriteLine(
                        "Desbloqueo verificado y registrado.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(
                        "ERROR CRÍTICO: no se pudo confirmar " +
                        "el desbloqueo de skyplus_app.");

                    throw new InvalidOperationException(
                        "Falló la recuperación de la cuenta. " +
                        "El estado pendiente quedó registrado.",
                        ex);
                }
            }
        }

        if (errorPrincipal != null)
        {
            throw new InvalidOperationException(
                "La prueba de bloqueo falló.",
                errorPrincipal);
        }

        Console.WriteLine();
        Console.WriteLine(
            "Prueba controlada finalizada correctamente.");
    }
}
