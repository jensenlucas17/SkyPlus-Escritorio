using System.Diagnostics;

namespace SkyPlus.RestoreWorker.Services;

/// <summary>Importador de producción. Solo se ejecuta desde el orquestador con restore.lock adquirido.</summary>
public static class MariaDbProductionRestoreService
{
    private const string BaseProduccion = "skyplus";
    private const string UsuarioProduccion = "skyplus_restore_prod@localhost";

    public static async Task VerificarConexionAsync(string mariaDbBinPath, string credentialsFile,
        CancellationToken cancellationToken = default)
    {
        string ejecutable = ValidarConfiguracion(mariaDbBinPath, credentialsFile);
        var inicio = CrearInicio(ejecutable, credentialsFile);
        inicio.ArgumentList.Add("--batch");
        inicio.ArgumentList.Add("--skip-column-names");
        inicio.ArgumentList.Add("--execute=SELECT CURRENT_USER(), DATABASE()");
        inicio.ArgumentList.Add(BaseProduccion);
        using var proceso = new Process { StartInfo = inicio };
        if (!proceso.Start()) throw new InvalidOperationException("No se pudo iniciar MariaDB.");
        try
        {
            Task<string> salida = proceso.StandardOutput.ReadToEndAsync();
            Task<string> errores = proceso.StandardError.ReadToEndAsync();
            await proceso.WaitForExitAsync(cancellationToken);
            string texto = (await salida).Trim();
            string error = await errores;
            if (proceso.ExitCode != 0)
                throw new InvalidOperationException("No se pudo verificar la conexión de producción: " + error.Trim());
            string[] columnas = texto.Split('\t');
            if (columnas.Length != 2 ||
                !string.Equals(columnas[0], UsuarioProduccion, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(columnas[1], BaseProduccion, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("La identidad o base de producción no coincide con la esperada.");
            Console.WriteLine("Conexión de restauración de producción verificada.");
        }
        catch { await DetenerAsync(proceso); throw; }
    }

    /// <summary>
    /// Destructivo. El orquestador debe mantener restore.lock, validar autorización, SHA-256,
    /// aislamiento y registrar ImportStarted antes de llamar este método.
    /// </summary>
    public static async Task ImportarAsync(string mariaDbBinPath, string credentialsFile,
        string rutaBackup, string productionStateFile, Guid operationId, Guid backupId,
        string accountAdminCredentialsFile, string monitorCredentialsFile,
        CancellationToken cancellationToken = default)
    {
        string ejecutable = ValidarConfiguracion(mariaDbBinPath, credentialsFile);
        if (operationId == Guid.Empty || backupId == Guid.Empty)
            throw new InvalidOperationException("Identificadores de restauración inválidos.");

        var estado = await ProductionRestoreStateService.LeerAsync(productionStateFile)
            ?? throw new InvalidOperationException("No existe estado persistido de producción.");
        if (estado.OperationId != operationId || estado.BackupId != backupId ||
            estado.Phase != "ImportStarted" || !estado.ApplicationLockAttempted ||
            !estado.ImportStarted || estado.ImportCompleted || estado.VerificationCompleted ||
            estado.ApplicationUnlocked)
            throw new InvalidOperationException("El estado no autoriza el inicio de la importación.");

        // Si existe cualquier marcador (incluso incompleto), nunca repetir la importación.
        if (File.Exists(ProductionImportAttemptService.ObtenerRuta(productionStateFile, operationId)))
            throw new InvalidOperationException("Ya existe un intento de importación registrado para esta operación.");

        if (!await MariaDbApplicationLockService.EstaBloqueadaAsync(mariaDbBinPath, accountAdminCredentialsFile))
            throw new InvalidOperationException("La cuenta de aplicación no está bloqueada.");
        await MariaDbIsolationVerifier.VerificarAsync(monitorCredentialsFile, BaseProduccion);
        await VerificarConexionAsync(mariaDbBinPath, credentialsFile, cancellationToken);

        if (!File.Exists(rutaBackup) ||
            !string.Equals(Path.GetExtension(rutaBackup), ".sql", StringComparison.OrdinalIgnoreCase) ||
            new FileInfo(rutaBackup).Length == 0)
            throw new InvalidOperationException("El archivo SQL de producción no es válido.");

        // Registrar ANTES de lanzar mariadb.exe. Un error posterior requiere recuperación manual.
        await ProductionImportAttemptService.RegistrarAsync(productionStateFile, operationId, backupId);

        var inicio = CrearInicio(ejecutable, credentialsFile);
        inicio.RedirectStandardInput = true;
        inicio.ArgumentList.Add(BaseProduccion);
        using var proceso = new Process { StartInfo = inicio };
        if (!proceso.Start())
            throw new InvalidOperationException("No se pudo iniciar la importación de MariaDB.");
        try
        {
            Task<string> errores = proceso.StandardError.ReadToEndAsync();
            Task<string> salida = proceso.StandardOutput.ReadToEndAsync();
            await using (var archivo = new FileStream(rutaBackup, FileMode.Open, FileAccess.Read, FileShare.Read))
                await archivo.CopyToAsync(proceso.StandardInput.BaseStream, cancellationToken);
            proceso.StandardInput.Close();
            await proceso.WaitForExitAsync(cancellationToken);
            string error = await errores;
            await salida;
            if (proceso.ExitCode != 0)
                throw new InvalidOperationException(
                    "La importación de producción falló y puede haber dejado datos parciales. " + error.Trim());
            Console.WriteLine("Importación SQL de producción finalizada; falta verificación posterior.");
        }
        catch { await DetenerAsync(proceso); throw; }
    }

    private static string ValidarConfiguracion(string mariaDbBinPath, string credentialsFile)
    {
        string ejecutable = Path.Combine(mariaDbBinPath, "mariadb.exe");
        if (!File.Exists(ejecutable)) throw new FileNotFoundException("No se encontró mariadb.exe.", ejecutable);
        if (!File.Exists(credentialsFile) ||
            !string.Equals(Path.GetFileName(credentialsFile), "restore-prod.cnf", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Se requiere restore-prod.cnf.");
        return ejecutable;
    }

    private static ProcessStartInfo CrearInicio(string ejecutable, string credentialsFile)
    {
        var inicio = new ProcessStartInfo
        {
            FileName = ejecutable,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        inicio.ArgumentList.Add($"--defaults-file={credentialsFile}");
        inicio.ArgumentList.Add("--default-character-set=utf8mb4");
        return inicio;
    }

    private static async Task DetenerAsync(Process proceso)
    {
        try
        {
            if (!proceso.HasExited)
            {
                proceso.Kill(entireProcessTree: true);
                await proceso.WaitForExitAsync(CancellationToken.None);
            }
        }
        catch { /* Preservar la excepción original. */ }
    }
}
