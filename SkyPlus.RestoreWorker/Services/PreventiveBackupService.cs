
using System.Diagnostics;
using System.Security.Cryptography;

namespace SkyPlus.RestoreWorker.Services
{
    public static class PreventiveBackupService
    {
        public static async Task<string> GenerarAsync(
            string mariaDbBinPath,
            string credentialsFile,
            string database,
            string destinationDirectory,
            CancellationToken ct = default)
        {
            string ejecutable = Path.Combine(
                mariaDbBinPath, "mariadb-dump.exe");

            if (!File.Exists(ejecutable))
                throw new FileNotFoundException(
                    "No se encontró mariadb-dump.exe.");

            if (!File.Exists(credentialsFile))
                throw new FileNotFoundException(
                    "No se encontró backup.cnf.");

            if (database != "skyplus")
                throw new InvalidOperationException(
                    "Base de datos no permitida.");

            Directory.CreateDirectory(destinationDirectory);

            string nombreArchivo =
                $"skyplus_preventivo_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.sql";

            string rutaArchivo = Path.Combine(
                destinationDirectory, nombreArchivo);

            var inicio = new ProcessStartInfo
            {
                FileName = ejecutable,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            inicio.ArgumentList.Add(
                $"--defaults-file={credentialsFile}");

            inicio.ArgumentList.Add("--single-transaction");
            inicio.ArgumentList.Add("--quick");
            inicio.ArgumentList.Add("--routines");
            inicio.ArgumentList.Add("--events");
            inicio.ArgumentList.Add("--triggers");
            inicio.ArgumentList.Add(
                "--default-character-set=utf8mb4");
            inicio.ArgumentList.Add(database);

            Process? proceso = null;

            try
            {
                proceso = new Process { StartInfo = inicio };

                if (!proceso.Start())
                    throw new InvalidOperationException(
                        "No se pudo iniciar mariadb-dump.");

                Task<string> errores =
                    proceso.StandardError.ReadToEndAsync(ct);

                await using (var archivo = new FileStream(
                    rutaArchivo,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await proceso.StandardOutput.BaseStream
                        .CopyToAsync(archivo, ct);

                    await archivo.FlushAsync(ct);
                }

                await proceso.WaitForExitAsync(ct);
                await errores;

                if (proceso.ExitCode != 0)
                    throw new InvalidOperationException(
                        "Falló la generación del backup preventivo.");

                await using var lectura = File.OpenRead(rutaArchivo);

                if (lectura.Length == 0)
                    throw new InvalidOperationException(
                        "El backup preventivo está vacío.");

                byte[] hash = await SHA256.HashDataAsync(
                    lectura, ct);

                string sha256 = Convert.ToHexString(hash);

                await File.WriteAllTextAsync(
                    rutaArchivo + ".sha256",
                    sha256,
                    ct);

                return rutaArchivo;
            }
            catch
            {
                if (proceso != null)
                {
                    try
                    {
                        if (!proceso.HasExited)
                        {
                            proceso.Kill(entireProcessTree: true);
                            await proceso.WaitForExitAsync(
                                CancellationToken.None);
                        }
                    }
                    catch
                    {
                        // Conservar el error original.
                    }
                }

                if (File.Exists(rutaArchivo))
                    File.Delete(rutaArchivo);

                throw;
            }
            finally
            {
                proceso?.Dispose();
            }
        }
    }
}
