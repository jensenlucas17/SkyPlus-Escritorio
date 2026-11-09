using System.Diagnostics;

namespace SkyPlus.RestoreWorker.Services;

public static class MariaDbImportService
{
    public static async Task ImportarAsync(
        string mariaDbBinPath,
        string archivoCredenciales,
        string rutaBackup,
        string baseDatos)
    {
        if (baseDatos != "skyplus")
            throw new InvalidOperationException(
                "La restauración solo está permitida en skyplus.");

        if (!File.Exists(archivoCredenciales))
            throw new FileNotFoundException(
                "No existe el archivo de credenciales.",
                archivoCredenciales);

        if (!File.Exists(rutaBackup))
            throw new FileNotFoundException(
                "No existe el backup seleccionado.",
                rutaBackup);

        string ejecutable = Path.Combine(
            mariaDbBinPath, "mariadb.exe");

        if (!File.Exists(ejecutable))
            throw new FileNotFoundException(
                "No se encontró mariadb.exe.",
                ejecutable);

        var inicio = new ProcessStartInfo
        {
            FileName = ejecutable,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        inicio.ArgumentList.Add(
            $"--defaults-extra-file={archivoCredenciales}");

        inicio.ArgumentList.Add(
            $"--database={baseDatos}");

        inicio.ArgumentList.Add("--binary-mode");

        await using var archivo = File.OpenRead(rutaBackup);

        using var proceso = new Process
        {
            StartInfo = inicio
        };

        if (!proceso.Start())
            throw new InvalidOperationException(
                "No se pudo iniciar el cliente MariaDB.");

        // Enviar el SQL directamente al cliente MariaDB.
        Task copiar = archivo.CopyToAsync(
            proceso.StandardInput.BaseStream);

        Task<string> leerErrores =
            proceso.StandardError.ReadToEndAsync();

        try
        {
            await copiar;

            proceso.StandardInput.Close();

            await proceso.WaitForExitAsync();

            string errores = await leerErrores;

            if (proceso.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    "La importación SQL falló: " + errores);
            }

            Console.WriteLine(
                "Importación SQL finalizada correctamente.");
        }
        catch
        {
            if (!proceso.HasExited)
            {
                proceso.Kill(entireProcessTree: true);
                await proceso.WaitForExitAsync();
            }

            throw;
        }
    }
}