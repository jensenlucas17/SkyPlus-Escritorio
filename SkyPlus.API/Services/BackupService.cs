using SkyPlus.API.DTOs;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SkyPlus.API.Services
{
    public class BackupService : IBackupService
    {
        private readonly IConfiguration _configuration;

        // Compartido entre todas las instancias del servicio.
        private static readonly SemaphoreSlim _semaforo = new(1, 1);

        private static readonly JsonSerializerOptions JsonOptions =
            new() { WriteIndented = true };

        public BackupService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // =========================================
        // RUTAS DE CONFIGURACIÓN
        // =========================================

        private string DirectorioBackups =>
            _configuration["Backup:BackupDirectory"]
            ?? throw new InvalidOperationException(
                "Falta BackupDirectory.");

        private string RutaHistorial =>
            Path.Combine(
                _configuration["Backup:AuditDirectory"]
                ?? throw new InvalidOperationException(
                    "Falta AuditDirectory."),
                "backups.json");

        // =========================================
        // LEER HISTORIAL
        // =========================================

        private async Task<List<BackupHistorialDto>> LeerHistorialAsync(
            CancellationToken cancellationToken)
        {
            if (!File.Exists(RutaHistorial))
                return new List<BackupHistorialDto>();

            await using var stream = File.OpenRead(RutaHistorial);

            return await JsonSerializer.DeserializeAsync<
                List<BackupHistorialDto>>(
                    stream,
                    JsonOptions,
                    cancellationToken)
                ?? new List<BackupHistorialDto>();
        }

        // =========================================
        // GUARDAR HISTORIAL
        // =========================================

        private async Task GuardarHistorialAsync(
            List<BackupHistorialDto> historial,
            CancellationToken cancellationToken)
        {
            string ruta = RutaHistorial;

            Directory.CreateDirectory(
                Path.GetDirectoryName(ruta)!);

            string temporal = ruta + "." +
                Guid.NewGuid().ToString("N") + ".tmp";

            try
            {
                await using (var stream = new FileStream(
                    temporal,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await JsonSerializer.SerializeAsync(
                        stream,
                        historial,
                        JsonOptions,
                        cancellationToken);

                    await stream.FlushAsync(cancellationToken);
                }

                File.Move(
                    temporal,
                    ruta,
                    overwrite: true);
            }
            finally
            {
                if (File.Exists(temporal))
                    File.Delete(temporal);
            }
        }

        // =========================================
        // VALIDAR NOMBRE DE ARCHIVO
        // =========================================

        private static void ValidarNombreArchivo(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo) ||
                Path.GetFileName(nombreArchivo) != nombreArchivo ||
                !nombreArchivo.EndsWith(
                    ".sql",
                    StringComparison.OrdinalIgnoreCase) ||
                nombreArchivo.Contains("..") ||
                nombreArchivo.Contains('/') ||
                nombreArchivo.Contains('\\') ||
                Path.IsPathRooted(nombreArchivo))
            {
                throw new InvalidOperationException(
                    "El nombre del archivo de backup no es válido.");
            }
        }

        // =========================================
        // CONSULTAR HISTORIAL
        // =========================================

        public async Task<List<BackupHistorialDto>> ObtenerHistorialAsync(
            CancellationToken cancellationToken = default)
        {
            await _semaforo.WaitAsync(cancellationToken);

            try
            {
                var historial =
                    await LeerHistorialAsync(cancellationToken);

                foreach (var item in historial)
                {
                    if (item.Estado == "Disponible")
                    {
                        ValidarNombreArchivo(item.Archivo);

                        string ruta = Path.Combine(
                            DirectorioBackups,
                            item.Archivo);

                        if (!File.Exists(ruta))
                            item.Estado = "NoDisponible";
                    }
                }

                return historial
                    .OrderByDescending(x => x.FechaUtc)
                    .ToList();
            }
            finally
            {
                _semaforo.Release();
            }
        }

        // =========================================
        // VERIFICAR BACKUP
        // =========================================

        public async Task<BackupHistorialDto?> ObtenerBackupVerificadoAsync(
            Guid idBackup,
            CancellationToken cancellationToken = default)
        {
            await _semaforo.WaitAsync(cancellationToken);

            try
            {
                var historial =
                    await LeerHistorialAsync(cancellationToken);

                var backup = historial.FirstOrDefault(x =>
                    x.Id == idBackup &&
                    x.Estado == "Disponible");

                if (backup == null)
                    return null;

                ValidarNombreArchivo(backup.Archivo);

                string rutaArchivo = Path.Combine(
                    DirectorioBackups,
                    backup.Archivo);

                if (!File.Exists(rutaArchivo))
                {
                    throw new InvalidOperationException(
                        "El archivo de backup no existe.");
                }

                await using var archivo = new FileStream(
                    rutaArchivo,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);

                if (archivo.Length == 0 ||
                    archivo.Length != backup.TamanoBytes)
                {
                    throw new InvalidOperationException(
                        "El tamaño del backup no coincide con el historial.");
                }

                byte[] hash = await SHA256.HashDataAsync(
                    archivo,
                    cancellationToken);

                string hashActual = Convert.ToHexString(hash);

                if (!string.Equals(
                    hashActual,
                    backup.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "La integridad SHA-256 del backup no coincide.");
                }

                return backup;
            }
            finally
            {
                _semaforo.Release();
            }
        }

        // =========================================
        // ELIMINAR BACKUP
        // =========================================

        public async Task<bool> EliminarBackupAsync(
            Guid idBackup,
            int idAdministrador,
            CancellationToken cancellationToken = default)
        {
            if (idAdministrador <= 0)
            {
                throw new ArgumentException(
                    "Administrador inválido.");
            }

            await _semaforo.WaitAsync(cancellationToken);

            try
            {
                var historial =
                    await LeerHistorialAsync(cancellationToken);

                var backup = historial.FirstOrDefault(
                    x => x.Id == idBackup);

                if (backup == null ||
                    backup.Estado != "Disponible")
                {
                    return false;
                }

                ValidarNombreArchivo(backup.Archivo);

                string rutaArchivo = Path.Combine(
                    DirectorioBackups,
                    backup.Archivo);

                if (!File.Exists(rutaArchivo))
                    return false;

                // Conservar información de la eliminación.
                backup.Estado = "Eliminado";
                backup.FechaEliminacionUtc = DateTime.UtcNow;
                backup.IdAdministradorEliminacion = idAdministrador;

                // Primero registrar la operación.
                await GuardarHistorialAsync(
                    historial,
                    cancellationToken);

                // Después eliminar el archivo físico.
                File.Delete(rutaArchivo);

                return true;
            }
            finally
            {
                _semaforo.Release();
            }
        }

        // =========================================
        // GENERAR BACKUP
        // =========================================

        public async Task<string> GenerarBackupAsync(
            int idAdministrador,
            CancellationToken cancellationToken = default)
        {
            if (idAdministrador <= 0)
            {
                throw new ArgumentException(
                    "Administrador inválido.");
            }

            await _semaforo.WaitAsync(cancellationToken);

            string? rutaArchivo = null;
            Process? proceso = null;
            bool registrado = false;

            try
            {
                string binPath =
                    _configuration["Backup:MariaDbBinPath"]
                    ?? throw new InvalidOperationException(
                        "Falta MariaDbBinPath.");

                string database =
                    _configuration["Backup:Database"]
                    ?? throw new InvalidOperationException(
                        "Falta Database.");

                string host =
                    _configuration["Backup:Host"] ?? "localhost";

                string port =
                    _configuration["Backup:Port"] ?? "3306";

                string ejecutable = Path.Combine(
                    binPath,
                    "mariadb-dump.exe");

                string credenciales =
                    @"C:\SkyPlus\Config\backup.cnf";

                if (!File.Exists(ejecutable))
                {
                    throw new FileNotFoundException(
                        "No se encontró mariadb-dump.exe.",
                        ejecutable);
                }

                if (!File.Exists(credenciales))
                {
                    throw new FileNotFoundException(
                        "No se encontró backup.cnf.",
                        credenciales);
                }

                Directory.CreateDirectory(DirectorioBackups);

                string nombreArchivo =
                    $"skyplus_{DateTime.UtcNow:yyyyMMdd_HHmmss}_" +
                    $"{Guid.NewGuid():N}.sql";

                rutaArchivo = Path.Combine(
                    DirectorioBackups,
                    nombreArchivo);

                var inicio = new ProcessStartInfo
                {
                    FileName = ejecutable,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                inicio.ArgumentList.Add(
                    $"--defaults-file={credenciales}");

                inicio.ArgumentList.Add($"--host={host}");
                inicio.ArgumentList.Add($"--port={port}");
                inicio.ArgumentList.Add("--single-transaction");
                inicio.ArgumentList.Add("--quick");
                inicio.ArgumentList.Add("--routines");
                inicio.ArgumentList.Add("--events");
                inicio.ArgumentList.Add("--triggers");
                inicio.ArgumentList.Add(
                    "--default-character-set=utf8mb4");
                inicio.ArgumentList.Add(database);

                proceso = new Process
                {
                    StartInfo = inicio
                };

                if (!proceso.Start())
                {
                    throw new InvalidOperationException(
                        "No se pudo iniciar mariadb-dump.");
                }

                await using (var archivo = new FileStream(
                    rutaArchivo,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    var copiar = proceso.StandardOutput.BaseStream
                        .CopyToAsync(
                            archivo,
                            cancellationToken);

                    var leerErrores = proceso.StandardError
                        .ReadToEndAsync(cancellationToken);

                    await Task.WhenAll(
                        copiar,
                        leerErrores);

                    await proceso.WaitForExitAsync(
                        cancellationToken);

                    if (proceso.ExitCode != 0)
                    {
                        throw new InvalidOperationException(
                            "MariaDB no pudo generar el respaldo.");
                    }

                    await archivo.FlushAsync(
                        cancellationToken);
                }

                var info = new FileInfo(rutaArchivo);

                if (info.Length == 0)
                {
                    throw new InvalidOperationException(
                        "El archivo de respaldo está vacío.");
                }

                // Calcular SHA-256.
                string hash;

                await using (var archivo = File.OpenRead(rutaArchivo))
                {
                    byte[] bytes = await SHA256.HashDataAsync(
                        archivo,
                        cancellationToken);

                    hash = Convert.ToHexString(bytes);
                }

                // Registrar en historial.
                var historial =
                    await LeerHistorialAsync(cancellationToken);

                historial.Add(new BackupHistorialDto
                {
                    Id = Guid.NewGuid(),
                    Archivo = nombreArchivo,
                    FechaUtc = DateTime.UtcNow,
                    IdAdministrador = idAdministrador,
                    Tipo = "Manual",
                    Estado = "Disponible",
                    TamanoBytes = info.Length,
                    Sha256 = hash
                });

                await GuardarHistorialAsync(
                    historial,
                    cancellationToken);

                registrado = true;

                return nombreArchivo;
            }
            catch
            {
                // Detener el proceso si sigue ejecutándose.
                if (proceso != null)
                {
                    try
                    {
                        if (!proceso.HasExited)
                        {
                            proceso.Kill(
                                entireProcessTree: true);

                            await proceso.WaitForExitAsync(
                                CancellationToken.None);
                        }
                    }
                    catch
                    {
                        // Conservar la excepción original.
                    }
                }

                // Eliminar archivos incompletos.
                if (!registrado &&
                    rutaArchivo != null &&
                    File.Exists(rutaArchivo))
                {
                    try
                    {
                        File.Delete(rutaArchivo);
                    }
                    catch
                    {
                        // Conservar la excepción original.
                    }
                }

                throw;
            }
            finally
            {
                proceso?.Dispose();
                _semaforo.Release();
            }
        }

        // =========================================
        // RESTAURAR BACKUP DE PRUEBA
        // =========================================

        public async Task<ResultadoRestauracionDto> RestaurarPruebaAsync(
            Guid idBackup,
            int idAdministradorSolicitante,
            int idAdministradorAutorizante,
            CancellationToken cancellationToken = default)
        {
            const string baseDestino = "skyplus_restauracion_test";

            if (idAdministradorSolicitante <= 0 ||
                idAdministradorAutorizante <= 0 ||
                idAdministradorSolicitante == idAdministradorAutorizante)
            {
                throw new ArgumentException(
                    "Se requieren dos administradores diferentes.");
            }

            // Límite máximo para esta restauración de prueba.
            using var tiempoLimite =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            tiempoLimite.CancelAfter(TimeSpan.FromMinutes(5));

            CancellationToken ct = tiempoLimite.Token;

            await _semaforo.WaitAsync(ct);

            string? copiaTemporal = null;
            Process? proceso = null;

            string estado = "Fallido";
            string? tipoError = null;
            bool auditoriaIniciada = false;

            string directorioAuditoria =
                _configuration["Backup:AuditDirectory"]
                ?? throw new InvalidOperationException(
                    "Falta AuditDirectory.");

            string rutaAuditoria = Path.Combine(
                directorioAuditoria,
                "restauraciones.jsonl");

            // Escritura obligatoria y sincronizada a disco.
            // No incluye contraseñas ni contenido SQL.
            void RegistrarEvento(string evento, string? error = null)
            {
                Directory.CreateDirectory(directorioAuditoria);

                var registro = new
                {
                    IdOperacion = idBackup,
                    FechaUtc = DateTime.UtcNow,
                    Evento = evento,
                    IdBackup = idBackup,
                    IdAdministradorSolicitante =
                        idAdministradorSolicitante,
                    IdAdministradorAutorizante =
                        idAdministradorAutorizante,
                    BaseDestino = baseDestino,
                    TipoError = error
                };

                byte[] contenido = System.Text.Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(registro) +
                    Environment.NewLine);

                using var archivo = new FileStream(
                    rutaAuditoria,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.Read);

                archivo.Write(contenido);
                archivo.Flush(flushToDisk: true);
            }

            try
            {
                // Si no se puede auditar el inicio,
                // no se ejecutará ningún SQL.
                RegistrarEvento("Inicio");
                auditoriaIniciada = true;

                var historial = await LeerHistorialAsync(ct);

                var backup = historial.FirstOrDefault(x =>
                    x.Id == idBackup &&
                    x.Estado == "Disponible");

                if (backup == null)
                    throw new InvalidOperationException(
                        "El backup no existe o no está disponible.");

                ValidarNombreArchivo(backup.Archivo);

                string rutaOriginal = Path.Combine(
                    DirectorioBackups,
                    backup.Archivo);

                string carpetaTemporal = Path.Combine(
                    DirectorioBackups,
                    "RestauracionesTemporales");

                Directory.CreateDirectory(carpetaTemporal);

                copiaTemporal = Path.Combine(
                    carpetaTemporal,
                    $"{Guid.NewGuid():N}.sql");

                // Copiar y verificar exactamente el archivo
                // que se utilizará para la restauración.
                await using (var origen = new FileStream(
                    rutaOriginal,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                await using (var destino = new FileStream(
                    copiaTemporal,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await origen.CopyToAsync(destino, ct);
                    await destino.FlushAsync(ct);
                }

                await using (var archivo = new FileStream(
                    copiaTemporal,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                {
                    if (archivo.Length == 0 ||
                        archivo.Length != backup.TamanoBytes)
                    {
                        throw new InvalidOperationException(
                            "El tamaño del backup no coincide.");
                    }

                    byte[] hash = await SHA256.HashDataAsync(
                        archivo, ct);

                    string hashActual = Convert.ToHexString(hash);

                    if (!string.Equals(
                        hashActual,
                        backup.Sha256,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "El SHA-256 del respaldo no coincide.");
                    }
                }

                string binPath =
                    _configuration["Backup:MariaDbBinPath"]
                    ?? throw new InvalidOperationException(
                        "Falta MariaDbBinPath.");

                string ejecutable = Path.Combine(
                    binPath, "mariadb.exe");

                string credenciales =
                    @"C:\SkyPlus\Config\restore.cnf";

                if (!File.Exists(ejecutable) ||
                    !File.Exists(credenciales))
                {
                    throw new InvalidOperationException(
                        "No se encontró mariadb.exe o restore.cnf.");
                }

                string host =
                    _configuration["Backup:Host"] ?? "localhost";

                string port =
                    _configuration["Backup:Port"] ?? "3306";

                var inicio = new ProcessStartInfo
                {
                    FileName = ejecutable,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                inicio.ArgumentList.Add(
                    $"--defaults-file={credenciales}");

                inicio.ArgumentList.Add($"--host={host}");
                inicio.ArgumentList.Add($"--port={port}");

                inicio.ArgumentList.Add(
                    "--default-character-set=utf8mb4");

                inicio.ArgumentList.Add("--binary-mode");
                inicio.ArgumentList.Add("--skip-reconnect");

                // Destino fijo, no recibido del cliente.
                inicio.ArgumentList.Add(baseDestino);

                // Auditar antes de iniciar MariaDB.
                RegistrarEvento("EjecucionSQL");

                proceso = new Process { StartInfo = inicio };

                if (!proceso.Start())
                {
                    throw new InvalidOperationException(
                        "No se pudo iniciar mariadb.exe.");
                }

                // Consumir ambas salidas para evitar bloqueos.
                Task<string> errores =
                    proceso.StandardError.ReadToEndAsync(ct);

                Task<string> salida =
                    proceso.StandardOutput.ReadToEndAsync(ct);

                await using (var archivo = new FileStream(
                    copiaTemporal,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                {
                    await archivo.CopyToAsync(
                        proceso.StandardInput.BaseStream,
                        ct);
                }

                proceso.StandardInput.Close();

                await proceso.WaitForExitAsync(ct);

                string resultadoErrores = await errores;
                await salida;

                if (proceso.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        "MariaDB devolvió un error durante la restauración.");
                }

                estado = "Exitosa";

                // El éxito debe quedar auditado antes de
                // devolver HTTP 200.
                RegistrarEvento(estado);

                return new ResultadoRestauracionDto
                {
                    IdBackup = idBackup,
                    BaseDestino = baseDestino,
                    IdAdministradorSolicitante =
                        idAdministradorSolicitante,
                    IdAdministradorAutorizante =
                        idAdministradorAutorizante,
                    FechaUtc = DateTime.UtcNow,
                    Estado = estado
                };
            }
            catch (Exception ex)
            {
                tipoError = ex.GetType().Name;

                if (auditoriaIniciada)
                {
                    try
                    {
                        RegistrarEvento("Fallido", tipoError);
                    }
                    catch
                    {
                        System.Diagnostics.Trace.TraceError(
                            "Fallo crítico: no se pudo registrar " +
                            "el resultado de la restauración.");
                    }
                }

                throw;
            }
            finally
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
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError(
                            "Error al detener MariaDB: {0}",
                            ex.GetType().Name);
                    }

                    proceso.Dispose();
                }

                try
                {
                    if (copiaTemporal != null &&
                        File.Exists(copiaTemporal))
                    {
                        File.Delete(copiaTemporal);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(
                        "Error al limpiar archivo temporal: {0}",
                        ex.GetType().Name);
                }

                _semaforo.Release();
            }
        }
    }
}