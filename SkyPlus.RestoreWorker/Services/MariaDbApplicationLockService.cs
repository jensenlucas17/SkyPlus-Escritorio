
using System.Diagnostics;

namespace SkyPlus.RestoreWorker.Services;

public static class MariaDbApplicationLockService
{
    private static async Task<string> EjecutarAsync(
        string mariaDbBinPath,
        string credentialsFile,
        string sql)
    {
        string ejecutable = Path.Combine(
            mariaDbBinPath,
            "mariadb.exe");

        if (!File.Exists(ejecutable))
            throw new FileNotFoundException(
                "No se encontró mariadb.exe.", ejecutable);

        if (!File.Exists(credentialsFile))
            throw new FileNotFoundException(
                "No existe el archivo de credenciales.",
                credentialsFile);

        var inicio = new ProcessStartInfo
        {
            FileName = ejecutable,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        inicio.ArgumentList.Add(
            $"--defaults-extra-file={credentialsFile}");

        inicio.ArgumentList.Add("--batch");
        inicio.ArgumentList.Add("--skip-column-names");
        inicio.ArgumentList.Add($"--execute={sql}");

        using var proceso = Process.Start(inicio)
            ?? throw new InvalidOperationException(
                "No se pudo iniciar MariaDB.");

        Task<string> salida =
            proceso.StandardOutput.ReadToEndAsync();

        Task<string> errores =
            proceso.StandardError.ReadToEndAsync();

        await proceso.WaitForExitAsync();

        string resultado = await salida;
        string error = await errores;

        if (proceso.ExitCode != 0)
        {
            throw new InvalidOperationException(
                "Falló la operación de MariaDB: " + error);
        }

        return resultado.Trim();
    }

    public static async Task BloquearAsync(
        string mariaDbBinPath,
        string credentialsFile)
    {
        await EjecutarAsync(
            mariaDbBinPath,
            credentialsFile,
            "ALTER USER 'skyplus_app'@'localhost' ACCOUNT LOCK;");
    }

    public static async Task DesbloquearAsync(
        string mariaDbBinPath,
        string credentialsFile)
    {
        await EjecutarAsync(
            mariaDbBinPath,
            credentialsFile,
            "ALTER USER 'skyplus_app'@'localhost' ACCOUNT UNLOCK;");
    }
    public static async Task<bool> EstaBloqueadaAsync(
        string mariaDbBinPath,
        string credentialsFile)
    {
        string resultado = await EjecutarAsync(
            mariaDbBinPath,
            credentialsFile,
            "SELECT JSON_VALUE(Priv, '$.account_locked') " +
            "FROM mysql.global_priv " +
            "WHERE User = 'skyplus_app' " +
            "AND Host = 'localhost';");

        string[] filas = resultado.Split(
            '\n',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        if (filas.Length != 1)
        {
            throw new InvalidOperationException(
                "No se encontró exactamente una cuenta " +
                "skyplus_app@localhost.");
        }

        return filas[0].ToLowerInvariant() switch
        {
            "true" or "1" => true,
            "false" or "0" => false,
            _ => throw new InvalidOperationException(
                "MariaDB devolvió un estado de bloqueo inválido: " +
                filas[0])
        };
    }
}
