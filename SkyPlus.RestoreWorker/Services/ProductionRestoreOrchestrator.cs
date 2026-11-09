namespace SkyPlus.RestoreWorker.Services;

public static class ProductionRestoreOrchestrator
{
    public static async Task PrepararAsync(Guid operationId, Guid backupId,
        string backupDirectory, string auditDirectory, string productionStateFile,
        string authorizationDocument, string publicKeyFile)
    {
        await ProductionRestoreStateService.VerificarSinPendientesAsync(productionStateFile);
        var autorizacion = RestoreAuthorizationValidationService.Validar(
            authorizationDocument, publicKeyFile, backupId);
        if (autorizacion.OperationId != operationId)
            throw new InvalidOperationException("La operación no coincide con la autorización.");
        await BackupVerifier.VerificarAsync(backupId, backupDirectory, auditDirectory);
        Console.WriteLine("Autorización válida e integridad del backup verificada.");
        Console.WriteLine("Preparación completada. No se modificó producción.");
    }

    public static async Task<string> GenerarBackupPreventivoAsync(Guid operationId,
        Guid backupId, string mariaDbBinPath, string credentialsFile,
        string preventiveBackupDirectory, string productionStateFile)
    {
        await ProductionRestoreStateService.VerificarSinPendientesAsync(productionStateFile);
        var estado = new ProductionRestoreStateService.ProductionRestoreState
        {
            OperationId = operationId,
            BackupId = backupId,
            Phase = "PreventiveBackupStarted",
            UpdatedAtUtc = DateTime.UtcNow
        };
        await ProductionRestoreStateService.CrearAsync(productionStateFile, estado);
        try
        {
            string ruta = await PreventiveBackupService.GenerarAsync(
                mariaDbBinPath, credentialsFile, "skyplus", preventiveBackupDirectory);
            estado.PreventiveBackupPath = ruta;
            estado.Phase = "PreventiveBackupGenerated";
            await ProductionRestoreStateService.ActualizarAsync(productionStateFile, estado);
            string hash = await PreventiveBackupVerificationService.VerificarAsync(ruta);
            string hashFile = ruta + ".sha256";
            if (!File.Exists(hashFile) || !string.Equals(
                hash, (await File.ReadAllTextAsync(hashFile)).Trim(),
                StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("El SHA-256 del backup preventivo no coincide.");
            estado.PreventiveBackupSha256 = hash;
            estado.Phase = "PreventiveBackupVerified";
            await ProductionRestoreStateService.ActualizarAsync(productionStateFile, estado);
            Console.WriteLine("Backup preventivo generado y verificado.");
            return ruta;
        }
        catch (Exception ex)
        {
            try
            {
                var actual = await ProductionRestoreStateService.LeerAsync(productionStateFile);
                if (actual is not null && actual.OperationId == operationId &&
                    actual.BackupId == backupId &&
                    (actual.Phase == "PreventiveBackupStarted" ||
                     actual.Phase == "PreventiveBackupGenerated"))
                {
                    actual.Phase = "Failed";
                    actual.Error = ex.Message;
                    await ProductionRestoreStateService.ActualizarAsync(productionStateFile, actual);
                }
            }
            catch (Exception registro)
            {
                Console.Error.WriteLine("No se pudo registrar el error: " + registro.Message);
            }
            throw;
        }
    }

    /// <summary>
    /// Operación destructiva. El llamador DEBE mantener RestoreExecutionLock durante
    /// toda la ejecución. El verificador suministrado debe comprobar realmente
    /// estructura, relaciones y datos de skyplus; nunca usar un callback vacío.
    /// No se debe invocar hasta integrar un verificador de producción confiable.
    /// </summary>
    public static async Task ContinuarProduccionAsync(
        Guid operationId, Guid backupId, string authorizationDocument,
        string publicKeyFile, string backupDirectory, string auditDirectory,
        string productionStateFile, string maintenanceFile,
        string mariaDbBinPath, string restoreCredentialsFile,
        string accountAdminCredentialsFile, string monitorCredentialsFile,
        Func<CancellationToken, Task> verificarProduccionAsync,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(verificarProduccionAsync);
        if (!File.Exists(maintenanceFile))
            throw new InvalidOperationException("SkyPlus no está en mantenimiento.");

        var estado = await ProductionRestoreStateService.ValidarContinuacionAsync(
            productionStateFile, operationId, backupId);
        RestoreAuthorizationValidationService.ValidarContinuacion(
            authorizationDocument, publicKeyFile, backupId, operationId);
        string ruta = await BackupVerifier.VerificarAsync(
            backupId, backupDirectory, auditDirectory);
        await ProductionRestoreService.PrepararAsync(ruta, "skyplus");
        await MariaDbProductionRestoreService.VerificarConexionAsync(
            mariaDbBinPath, restoreCredentialsFile, cancellationToken);
        IsolationVerifier.VerificarProcesos();
        await MariaDbIsolationVerifier.VerificarAsync(monitorCredentialsFile, "skyplus");
        if (await MariaDbApplicationLockService.EstaBloqueadaAsync(
            mariaDbBinPath, accountAdminCredentialsFile))
            throw new InvalidOperationException(
                "La cuenta ya está bloqueada; requiere revisión administrativa.");

        bool intentoRegistrado = false;
        try
        {
            estado.ApplicationLockAttempted = true;
            estado.Phase = "ApplicationLockAttempted";
            await ProductionRestoreStateService.ActualizarAsync(productionStateFile, estado);
            intentoRegistrado = true;
            await MariaDbApplicationLockService.BloquearAsync(
                mariaDbBinPath, accountAdminCredentialsFile);
            if (!await MariaDbApplicationLockService.EstaBloqueadaAsync(
                mariaDbBinPath, accountAdminCredentialsFile))
                throw new InvalidOperationException("No se confirmó el bloqueo de la cuenta.");
            IsolationVerifier.VerificarProcesos();
            await MariaDbIsolationVerifier.VerificarAsync(monitorCredentialsFile, "skyplus");
            estado.Phase = "ApplicationIsolated";
            await ProductionRestoreStateService.ActualizarAsync(productionStateFile, estado);

            // Revalidar integridad e identidad del respaldo antes de importar.
            string rutaRevalidada = await BackupVerifier.VerificarAsync(
                backupId, backupDirectory, auditDirectory);
            if (!string.Equals(Path.GetFullPath(ruta), Path.GetFullPath(rutaRevalidada),
                StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cambió el archivo de backup.");
            estado.ImportStarted = true;
            estado.Phase = "ImportStarted";
            await ProductionRestoreStateService.ActualizarAsync(productionStateFile, estado);
            await MariaDbProductionRestoreService.ImportarAsync(
                mariaDbBinPath, restoreCredentialsFile, rutaRevalidada,
                productionStateFile, operationId, backupId,
                accountAdminCredentialsFile, monitorCredentialsFile,
                cancellationToken);
            estado.ImportCompleted = true;
            estado.Phase = "ImportCompleted";
            await ProductionRestoreStateService.ActualizarAsync(productionStateFile, estado);

            // La verificación debe ser independiente del resultado del importador.
            await verificarProduccionAsync(cancellationToken);
            estado.VerificationCompleted = true;
            estado.Phase = "VerificationCompleted";
            await ProductionRestoreStateService.ActualizarAsync(productionStateFile, estado);
            await MariaDbApplicationLockService.DesbloquearAsync(
                mariaDbBinPath, accountAdminCredentialsFile);
            if (await MariaDbApplicationLockService.EstaBloqueadaAsync(
                mariaDbBinPath, accountAdminCredentialsFile))
                throw new InvalidOperationException("No se confirmó el desbloqueo.");
            estado.ApplicationUnlocked = true;
            estado.Phase = "ApplicationUnlocked";
            await ProductionRestoreStateService.ActualizarAsync(productionStateFile, estado);
            estado.Phase = "Completed";
            await ProductionRestoreStateService.ActualizarAsync(productionStateFile, estado);
            Console.WriteLine("Restauración de producción completada y verificada.");
        }
        catch (Exception ex)
        {
            // No intentar desbloquear ni bloquear automáticamente en una excepción:
            // podría existir una importación parcial o un estado incierto.
            if (intentoRegistrado)
            {
                string diagnosticoCuenta;
                try
                {
                    bool bloqueada = await MariaDbApplicationLockService.EstaBloqueadaAsync(
                        mariaDbBinPath, accountAdminCredentialsFile);
                    diagnosticoCuenta = bloqueada
                        ? "skyplus_app: BLOQUEADA."
                        : "skyplus_app: DESBLOQUEADA. Aislar la aplicación de inmediato.";
                }
                catch (Exception consulta)
                {
                    diagnosticoCuenta = "No se pudo determinar el bloqueo de skyplus_app: "
                        + consulta.Message;
                }

                Console.Error.WriteLine("ALERTA DE RECUPERACIÓN: " + diagnosticoCuenta);
                try
                {
                    var actual = await ProductionRestoreStateService.LeerAsync(
                        productionStateFile);
                    if (actual is not null && actual.OperationId == operationId &&
                        actual.BackupId == backupId &&
                        actual.Phase is "ApplicationLockAttempted" or "ApplicationIsolated"
                            or "ImportStarted" or "ImportCompleted"
                            or "VerificationCompleted" or "ApplicationUnlocked")
                    {
                        actual.Phase = "RequiresRecovery";
                        actual.Error = ex.Message + " | " + diagnosticoCuenta;
                        await ProductionRestoreStateService.ActualizarAsync(
                            productionStateFile, actual);
                    }
                    else
                    {
                        Console.Error.WriteLine(
                            "No se actualizó el estado: fase o identificadores inesperados. " +
                            "Requiere revisión administrativa.");
                    }
                }
                catch (Exception registro)
                {
                    Console.Error.WriteLine(
                        "CRÍTICO: No se pudo registrar RequiresRecovery: " + registro.Message);
                }
            }

            Console.Error.WriteLine(
                "Restauración interrumpida. No reiniciar ni habilitar SkyPlus " +
                "sin comprobar base, conexiones, cuenta y estado persistido.");
            throw;
        }
    }
}