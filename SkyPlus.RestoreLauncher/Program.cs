using System.Management;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text.Json;
using SkyPlus.RestoreWorker.Services;

[assembly: SupportedOSPlatform("windows")]

Console.WriteLine("=== SkyPlus RestoreLauncher ===");
Console.WriteLine("GESTIÓN DE AUTORIZACIONES DE RESTAURACIÓN.");
Console.WriteLine();

bool flujoCompleto = args.Length == 3 && args[0] == "--flujo-completo";
bool iniciarProduccion = args.Length == 2 && args[0] == "--iniciar-produccion";
bool continuarProduccion = args.Length == 2 && args[0] == "--continuar-produccion";
bool diagnosticoInicial = args.Length == 2 && args[0] == "--diagnostico-autorizacion";
bool diagnosticoContinuacion = args.Length == 2 && args[0] == "--diagnostico-continuacion";
// Recibe la respuesta JSON de la API guardada temporalmente, sin usar Desktop.
// Sigue siendo SOLO LECTURA y requiere una operación pendiente real.
bool diagnosticoRespuesta = args.Length == 2 && args[0] == "--diagnostico-continuacion-respuesta";
diagnosticoContinuacion |= diagnosticoRespuesta;

if (!diagnosticoInicial && !diagnosticoContinuacion && !iniciarProduccion && !continuarProduccion && !flujoCompleto)
{
    Console.Error.WriteLine("Uso: RestoreLauncher --flujo-completo <AUTORIZACION_INICIAL.bin> <AUTORIZACION_CONTINUACION.bin>");
    Console.Error.WriteLine("   o: RestoreLauncher --iniciar-produccion <ARCHIVO_PROTEGIDO>");
    Console.Error.WriteLine("   o: RestoreLauncher --continuar-produccion <ARCHIVO_PROTEGIDO>");
    Console.Error.WriteLine("   o: RestoreLauncher --diagnostico-autorizacion <ARCHIVO_PROTEGIDO>");
    Console.Error.WriteLine("   o: RestoreLauncher --diagnostico-continuacion <ARCHIVO_PROTEGIDO>");
    Console.Error.WriteLine("   o: RestoreLauncher --diagnostico-continuacion-respuesta <RESPUESTA_API_JSON>");
    Environment.ExitCode = 1;
    EsperarSalida();
    return;
}

string ruta = Path.GetFullPath(args[1]);
string directorioPermitido = Path.GetFullPath(Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "SkyPlus", "RestoreHandoffs"));

const string clavePublica = @"C:\SkyPlus\Config\restore-public.pem";
const string monitorCredentialsFile = @"C:\SkyPlus\Config\monitor.cnf";
const string maintenanceFile = @"C:\SkyPlus\Config\mantenimiento.lock";
const string productionStateFile = @"C:\SkyPlus\Auditoria\production-restore-state.json";
const string backupDirectory = @"C:\SkyPlus\Backups";
const string auditDirectory = @"C:\SkyPlus\Auditoria";
const string database = "skyplus";

Guid operationId;
Guid backupId;
DateTime expiresAtUtc;
string autorizacionFirmada;
string? autorizacionContinuacion = null;
DateTime vencimientoContinuacion = DateTime.MinValue;

try
{
    // La respuesta JSON de Swagger contiene una autorización de corta duración.
    // Por seguridad solo se acepta desde el directorio privado de transferencia.
    if (!string.Equals(Path.GetDirectoryName(ruta), directorioPermitido,
            StringComparison.OrdinalIgnoreCase) ||
        !string.Equals(Path.GetExtension(ruta), diagnosticoRespuesta ? ".json" : ".bin",
            StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Ruta de transferencia no permitida.");

    var archivoInfo = new FileInfo(ruta);
    if (!archivoInfo.Exists || archivoInfo.Length is <= 0 or > 65536)
        throw new InvalidOperationException("Archivo de autorización inexistente o con tamaño inválido.");

    if (diagnosticoRespuesta)
    {
        // JSON de respuesta de POST /api/backups/autorizar-continuacion-produccion.
        // No contiene contraseñas; nunca se muestra la autorización firmada.
        using var respuesta = JsonDocument.Parse(await File.ReadAllTextAsync(ruta));
        var datos = respuesta.RootElement;
        operationId = LeerGuid(datos, "operationId");
        backupId = LeerGuid(datos, "backupId");
        expiresAtUtc = LeerFecha(datos, "expiresAtUtc");
        autorizacionFirmada = LeerTexto(datos, "autorizacionFirmada");
    }
    else
    {
        byte[] cifrado = await File.ReadAllBytesAsync(ruta);
        byte[] plano;
        try
        {
            plano = ProtectedData.Unprotect(cifrado, null, DataProtectionScope.CurrentUser);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(cifrado);
        }

        try
        {
            using var documento = JsonDocument.Parse(plano);
            var datos = documento.RootElement;
            operationId = LeerGuid(datos, "OperationId");
            backupId = LeerGuid(datos, "BackupId");
            expiresAtUtc = LeerFecha(datos, "ExpiresAtUtc");
            autorizacionFirmada = LeerTexto(datos, "AutorizacionFirmada");
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plano);
        }
    }

    if (operationId == Guid.Empty || backupId == Guid.Empty ||
        string.IsNullOrWhiteSpace(autorizacionFirmada))
        throw new InvalidOperationException("La transferencia está incompleta.");

    var autorizacion = (diagnosticoContinuacion || continuarProduccion)
        ? RestoreAuthorizationValidationService.ValidarContinuacion(
            autorizacionFirmada, clavePublica, backupId, operationId)
        : RestoreAuthorizationValidationService.Validar(
            autorizacionFirmada, clavePublica, backupId);

    if (autorizacion.OperationId != operationId ||
        autorizacion.BackupId != backupId ||
        autorizacion.ExpiresAtUtc != expiresAtUtc)
        throw new InvalidOperationException(
            "Los datos de transferencia no coinciden con la autorización firmada.");

    if (flujoCompleto)
    {
        string ruta2 = Path.GetFullPath(args[2]);
        if (string.Equals(ruta, ruta2, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(Path.GetDirectoryName(ruta2), directorioPermitido, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(Path.GetExtension(ruta2), ".bin", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Segunda transferencia inválida.");
        try
        {
            var info2 = new FileInfo(ruta2);
            if (!info2.Exists || info2.Length is <= 0 or > 65536)
                throw new InvalidOperationException("Segunda autorización ausente o inválida.");
            byte[] cifrado2 = await File.ReadAllBytesAsync(ruta2);
            byte[] plano2;
            try { plano2 = ProtectedData.Unprotect(cifrado2, null, DataProtectionScope.CurrentUser); }
            finally { CryptographicOperations.ZeroMemory(cifrado2); }
            try
            {
                using var doc2 = JsonDocument.Parse(plano2);
                var datos2 = doc2.RootElement;
                Guid op2 = LeerGuid(datos2, "OperationId");
                Guid backup2 = LeerGuid(datos2, "BackupId");
                vencimientoContinuacion = LeerFecha(datos2, "ExpiresAtUtc");
                autorizacionContinuacion = LeerTexto(datos2, "AutorizacionFirmada");
                var validada2 = RestoreAuthorizationValidationService.ValidarContinuacion(
                    autorizacionContinuacion, clavePublica, backup2, op2);
                if (op2 != operationId || backup2 != backupId ||
                    validada2.ExpiresAtUtc != vencimientoContinuacion ||
                    vencimientoContinuacion <= DateTime.UtcNow)
                    throw new InvalidOperationException("Las autorizaciones no corresponden a la misma operación o vencieron.");
            }
            finally { CryptographicOperations.ZeroMemory(plano2); }
        }
        finally
        {
            try { File.Delete(ruta2); }
            catch { Console.Error.WriteLine("ADVERTENCIA: no se pudo borrar la segunda transferencia."); }
        }
        Console.WriteLine("Ambas autorizaciones RSA verificadas y vinculadas a la misma operación.");
    }

    Console.WriteLine($"Operación recibida: {operationId}");
    Console.WriteLine($"Backup recibido: {backupId}");
    Console.WriteLine($"Vencimiento UTC: {expiresAtUtc:O}");
    Console.WriteLine((diagnosticoContinuacion || continuarProduccion)
        ? "Firma RSA y autorización de CONTINUACIÓN: VÁLIDAS."
        : "Firma RSA y autorización INICIAL: VÁLIDAS.");
}
catch (Exception ex)
{
    Console.Error.WriteLine("Autorización rechazada: " + ex.Message);
    Environment.ExitCode = 1;
    EsperarSalida();
    return;
}
finally
{
    // Una autorización firmada de producción es sensible: eliminar el
    // archivo temporal tanto si se aprueba como si se rechaza.
    try { File.Delete(ruta); }
    catch { Console.Error.WriteLine("ADVERTENCIA: no se pudo borrar el documento temporal."); }
}

// Flujo integrado desde Desktop: cierre, mantenimiento, importación y reinicio.
if (flujoCompleto)
{
    try
    {
        await RestoreAutomation.RunAsync(operationId, backupId, autorizacionFirmada,
            autorizacionContinuacion!, expiresAtUtc, vencimientoContinuacion);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine("AUTOMATIZACIÓN DETENIDA: " + ex.Message);
        Environment.ExitCode = 1;
    }
    EsperarSalida();
    return;
}

Console.WriteLine();
Console.WriteLine("Cerrá SkyPlus.API y SkyPlus.BackupWorker MANUALMENTE.");
Console.WriteLine("El Launcher no cerrará ni finalizará procesos.");
Console.WriteLine("Se comprobarán procesos, conexiones MariaDB, mantenimiento y backup.");

while (true)
{
    Console.WriteLine();

    if (DateTime.UtcNow >= expiresAtUtc || (flujoCompleto && DateTime.UtcNow >= vencimientoContinuacion))
    {
        Console.Error.WriteLine("La autorización venció. Generá una nueva desde Desktop.");
        Environment.ExitCode = 3;
        break;
    }

    if (!IntentarVerificarProcesos(out bool hayProcesos))
    {
        Console.Error.WriteLine("No se pudo completar el diagnóstico de procesos.");
        Environment.ExitCode = 1;
        break;
    }

    if (!hayProcesos)
    {
        Console.WriteLine("Verificación de procesos: APROBADA.");

        try
        {
            await MariaDbIsolationVerifier.VerificarAsync(
                monitorCredentialsFile, database);

            if (DateTime.UtcNow >= expiresAtUtc)
            {
                Console.Error.WriteLine("La autorización venció durante el diagnóstico.");
                Environment.ExitCode = 3;
                break;
            }

            if (!IntentarVerificarProcesos(out bool reaparecieronProcesos))
            {
                Console.Error.WriteLine("No se pudo repetir la comprobación de procesos.");
                Environment.ExitCode = 1;
                break;
            }

            if (!reaparecieronProcesos)
            {
                Console.WriteLine("Aislamiento preliminar: APROBADO.");

                if (!File.Exists(maintenanceFile))
                {
                    throw new InvalidOperationException(
                        "SkyPlus no está en modo mantenimiento (falta mantenimiento.lock).");
                }

                if (diagnosticoContinuacion || continuarProduccion)
                {
                    // Comprobar que existe precisamente ESTA operación pendiente.
                    // Esto verifica fase PreventiveBackupVerified, hash del preventivo
                    // y que no se inició el bloqueo de aplicación ni la importación.
                    await ProductionRestoreStateService.ValidarContinuacionAsync(
                        productionStateFile, operationId, backupId);
                    await BackupVerifier.VerificarAsync(
                        backupId, backupDirectory, auditDirectory);

                    if (DateTime.UtcNow >= expiresAtUtc)
                        throw new InvalidOperationException(
                            "La autorización de continuación venció durante las verificaciones.");

                    Console.WriteLine("Continuación: operación pendiente y backups verificados.");
                    if (continuarProduccion)
                    {
                        Console.WriteLine("ATENCIÓN: esta operación reemplazará datos de skyplus.");
                        Console.WriteLine("El backup preventivo existente se conservará.");
                        Console.Write("Escribí CONTINUAR RESTAURACION para iniciar RestoreWorker: ");
                        if (Console.ReadLine() != "CONTINUAR RESTAURACION")
                        {
                            Console.WriteLine("No se inició la restauración.");
                            Environment.ExitCode = 2;
                            break;
                        }
                        if (DateTime.UtcNow >= expiresAtUtc)
                            throw new InvalidOperationException("Autorización vencida.");
                        await InvocarWorkerContinuacionAsync(backupId, autorizacionFirmada);
                        Console.WriteLine("RestoreWorker finalizó. Verificá el estado persistente de producción.");
                    }
                    else
                    {
                        Console.WriteLine("DIAGNÓSTICO DE CONTINUACIÓN: APROBADO (SOLO LECTURA).");
                        Console.WriteLine("No se ejecutó --continuar-produccion ni se modificó MariaDB.");
                    }
                    break;
                }

                // Primer modo: la operación anterior debe estar completada.
                await ProductionRestoreStateService.VerificarSinPendientesAsync(
                    productionStateFile);

                if (DateTime.UtcNow >= expiresAtUtc)
                {
                    Console.Error.WriteLine("La autorización venció antes de la preparación.");
                    Environment.ExitCode = 3;
                    break;
                }

                // No registra operaciones, no hace backup preventivo ni importa SQL.
                await ProductionRestoreOrchestrator.PrepararAsync(
                    operationId,
                    backupId,
                    backupDirectory,
                    auditDirectory,
                    productionStateFile,
                    autorizacionFirmada,
                    clavePublica);

                Console.WriteLine("Preparación de solo lectura: APROBADA.");
                Console.WriteLine("Invocando verificación independiente de RestoreWorker...");
                await InvocarWorkerSoloLecturaAsync(backupId, autorizacionFirmada);
                Console.WriteLine("Prueba Launcher → RestoreWorker: APROBADA.");
                if (iniciarProduccion || flujoCompleto)
                {
                    Console.WriteLine();
                    Console.WriteLine("Se generará un BACKUP PREVENTIVO de skyplus.");
                    Console.WriteLine("Se registrará una operación pendiente y NO se importará SQL.");
                    Console.WriteLine(flujoCompleto ? "La continuación ya fue autorizada anticipadamente." : "Para continuar luego será necesaria una NUEVA autorización cruzada.");
                    Console.Write("Escribí GENERAR BACKUP para confirmar: ");
                    if (Console.ReadLine() != "GENERAR BACKUP")
                    {
                        Console.WriteLine("Cancelado: no se generó el backup preventivo.");
                        Environment.ExitCode = 2;
                        break;
                    }
                    if (DateTime.UtcNow >= expiresAtUtc)
                        throw new InvalidOperationException("La autorización inicial venció.");
                    // El Worker vuelve a verificar firma, mantenimiento, aislamiento y estado.
                    await InvocarWorkerBackupPreventivoAsync(backupId, autorizacionFirmada);
                    Console.WriteLine("Backup preventivo verificado. Operación pendiente de continuación.");
                    if (flujoCompleto)
                    {
                        // La validación de estado ocurre DESPUÉS de crear el preventivo.
                        // No reabrir la API ni permitir procesos SkyPlus durante la importación.
                        await ProductionRestoreStateService.ValidarContinuacionAsync(
                            productionStateFile, operationId, backupId);
                        if (DateTime.UtcNow >= vencimientoContinuacion)
                            throw new InvalidOperationException(
                                "Venció la autorización de continuación. Se conserva el preventivo; NO se importó SQL.");
                        if (!IntentarVerificarProcesos(out bool procesosActivos) || procesosActivos)
                            throw new InvalidOperationException("Reaparecieron procesos SkyPlus. No se importará SQL.");
                        Console.WriteLine("ATENCIÓN: se reemplazará la base de datos skyplus.");
                        Console.Write("Escribí CONTINUAR RESTAURACION para pasar al Worker: ");
                        if (Console.ReadLine() != "CONTINUAR RESTAURACION")
                        {
                            Console.WriteLine("No se importó SQL. La operación permanece pendiente.");
                            Environment.ExitCode = 2;
                            break;
                        }
                        if (DateTime.UtcNow >= vencimientoContinuacion)
                            throw new InvalidOperationException("Autorización de continuación vencida. No se importó SQL.");
                        await InvocarWorkerContinuacionAsync(backupId, autorizacionContinuacion!);
                        Console.WriteLine("RestoreWorker terminó. Revisá el estado persistente de producción.");
                    }
                    else Console.WriteLine("NO se restauró producción.");
                }
                else
                {
                    Console.WriteLine("No se creó una operación pendiente ni un backup preventivo.");
                    Console.WriteLine("No se restauró producción.");
                }
                break;
            }

            Console.WriteLine("AISLAMIENTO NO APROBADO: reaparecieron procesos SkyPlus.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("OPERACIÓN NO COMPLETADA: " + ex.Message);
            if (continuarProduccion || flujoCompleto)
            {
                Environment.ExitCode = 1;
                Console.Error.WriteLine("No reintentes sin revisar production-restore-state.json.");
                break;
            }
        }
    }
    else
    {
        Console.WriteLine("AISLAMIENTO NO APROBADO: hay procesos SkyPlus activos.");
    }

    Console.WriteLine("Corregí las condiciones indicadas antes de reintentar.");
    Console.Write("Escribí R para reintentar o C para cancelar: ");
    string? opcion = Console.ReadLine()?.Trim();

    if (!string.Equals(opcion, "R", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Diagnóstico cancelado.");
        Environment.ExitCode = 2;
        break;
    }
}

Console.WriteLine();
Console.WriteLine("FIN. Revisá los mensajes anteriores y el estado persistente.");
EsperarSalida();

static bool IntentarVerificarProcesos(out bool hayProcesos)
{
    hayProcesos = false;
    string[] aplicaciones = { "SkyPlus.API", "SkyPlus.Desktop", "SkyPlus.BackupWorker" };

    try
    {
        using var buscador = new ManagementObjectSearcher(
            "SELECT Name, ProcessId, CommandLine FROM Win32_Process");
        using var resultados = buscador.Get();

        foreach (ManagementObject proceso in resultados)
        {
            using (proceso)
            {
                string nombre = proceso["Name"]?.ToString() ?? "";
                string? comando = proceso["CommandLine"]?.ToString();

                if (nombre.Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase) &&
                    comando is null)
                {
                    hayProcesos = true;
                    Console.WriteLine(
                        "dotnet.exe sin línea de comandos: aislamiento no aprobado.");
                    continue;
                }

                bool esSkyPlus = aplicaciones.Any(app =>
                    nombre.Equals(app + ".exe", StringComparison.OrdinalIgnoreCase) ||
                    (nombre.Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase) &&
                     (comando ?? "").Contains(app, StringComparison.OrdinalIgnoreCase)));

                if (!esSkyPlus) continue;

                hayProcesos = true;
                Console.WriteLine(
                    $"Proceso activo: {nombre} (PID {proceso["ProcessId"]})");
            }
        }

        return true;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine("Error verificando procesos: " + ex.Message);
        hayProcesos = true;
        return false;
    }
}

static void EsperarSalida()
{
    Console.WriteLine("Presioná Enter para cerrar.");
    Console.ReadLine();
}



// Única invocación permitida en esta etapa: diagnóstico de SOLO LECTURA.
// No acepta el nombre del comando como entrada dinámica del usuario.
static async Task InvocarWorkerSoloLecturaAsync(
    Guid backupId, string autorizacionFirmada)
{
    string raizSolucion = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", ".."));
    string directorioWorker = Path.Combine(
        raizSolucion, "SkyPlus.RestoreWorker", "bin");

    if (!Directory.Exists(directorioWorker))
        throw new FileNotFoundException(
            "No existe el directorio compilado de RestoreWorker.");

    string[] candidatos = Directory.GetFiles(
        directorioWorker, "SkyPlus.RestoreWorker.exe",
        SearchOption.AllDirectories);

    if (candidatos.Length != 1)
        throw new InvalidOperationException(
            "Se esperaba exactamente un RestoreWorker.exe compilado. " +
            "Eliminá versiones duplicadas o configurá una ruta única.");

    string ejecutable = Path.GetFullPath(candidatos[0]);
    var inicio = new System.Diagnostics.ProcessStartInfo
    {
        FileName = ejecutable,
        WorkingDirectory = Path.GetDirectoryName(ejecutable)!,
        UseShellExecute = false,
        RedirectStandardInput = true,
        CreateNoWindow = false
    };
    inicio.ArgumentList.Add("--preparar-produccion-stdin");
    inicio.ArgumentList.Add(backupId.ToString());

    using var proceso = System.Diagnostics.Process.Start(inicio)
        ?? throw new InvalidOperationException("No se pudo iniciar RestoreWorker.");

    // Solo una línea JSON al canal privado del proceso hijo. No imprimir,
    // registrar ni colocar la autorización RSA en argumentos del proceso.
    await proceso.StandardInput.WriteLineAsync(autorizacionFirmada);
    proceso.StandardInput.Close();
    await proceso.WaitForExitAsync();

    if (proceso.ExitCode != 0)
        throw new InvalidOperationException(
            $"RestoreWorker rechazó la preparación (código {proceso.ExitCode}).");
}

// Nombres de propiedades tolerantes a mayúsculas/minúsculas en la respuesta JSON.
static JsonElement Propiedad(JsonElement elemento, string nombre)
{
    foreach (var propiedad in elemento.EnumerateObject())
        if (string.Equals(propiedad.Name, nombre, StringComparison.OrdinalIgnoreCase))
            return propiedad.Value;
    throw new InvalidOperationException($"Falta el campo {nombre} en la respuesta.");
}
static Guid LeerGuid(JsonElement elemento, string nombre) => Propiedad(elemento, nombre).GetGuid();
static DateTime LeerFecha(JsonElement elemento, string nombre) => Propiedad(elemento, nombre).GetDateTime();
static string LeerTexto(JsonElement elemento, string nombre) =>
    Propiedad(elemento, nombre).GetString() ??
    throw new InvalidOperationException($"El campo {nombre} está vacío.");

// Etapa 1: respaldo preventivo. No invoca importación ni continuación.
static async Task InvocarWorkerBackupPreventivoAsync(Guid backupId, string autorizacionFirmada)
{
    string raizSolucion = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", ".."));
    string directorioWorker = Path.Combine(raizSolucion, "SkyPlus.RestoreWorker", "bin");
    if (!Directory.Exists(directorioWorker))
        throw new DirectoryNotFoundException("No se encontró el directorio de RestoreWorker.");
    string[] candidatos = Directory.GetFiles(directorioWorker,
        "SkyPlus.RestoreWorker.exe", SearchOption.AllDirectories);
    if (candidatos.Length != 1)
        throw new InvalidOperationException("Se requiere un único RestoreWorker.exe compilado.");
    string exe = Path.GetFullPath(candidatos[0]);
    var inicio = new System.Diagnostics.ProcessStartInfo
    {
        FileName = exe,
        WorkingDirectory = Path.GetDirectoryName(exe)!,
        UseShellExecute = false,
        RedirectStandardInput = true,
        CreateNoWindow = false
    };
    inicio.ArgumentList.Add("--backup-preventivo-stdin");
    inicio.ArgumentList.Add(backupId.ToString());
    using var proceso = System.Diagnostics.Process.Start(inicio)
        ?? throw new InvalidOperationException("No se pudo iniciar RestoreWorker.");
    await proceso.StandardInput.WriteLineAsync(autorizacionFirmada);
    proceso.StandardInput.Close();
    await proceso.WaitForExitAsync();
    if (proceso.ExitCode != 0)
        throw new InvalidOperationException(
            $"RestoreWorker no completó el respaldo preventivo (código {proceso.ExitCode}). " +
            "Comprobá el estado persistente antes de reintentar.");
}

// Continuación real: autorización RSA por stdin; la confirmación destructiva
// permanece dentro del Worker y la operación mantiene su bloqueo exclusivo.
static async Task InvocarWorkerContinuacionAsync(Guid backupId, string autorizacionFirmada)
{
    string raiz = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    string dir = Path.Combine(raiz, "SkyPlus.RestoreWorker", "bin");
    if (!Directory.Exists(dir))
        throw new DirectoryNotFoundException("No se encontró RestoreWorker.");
    string[] candidatos = Directory.GetFiles(dir, "SkyPlus.RestoreWorker.exe", SearchOption.AllDirectories);
    if (candidatos.Length != 1)
        throw new InvalidOperationException("Se requiere un único RestoreWorker.exe compilado.");
    string exe = Path.GetFullPath(candidatos[0]);
    var inicio = new System.Diagnostics.ProcessStartInfo
    {
        FileName = exe,
        WorkingDirectory = Path.GetDirectoryName(exe)!,
        UseShellExecute = false,
        RedirectStandardInput = true,
        CreateNoWindow = false
    };
    inicio.ArgumentList.Add("--continuar-produccion-stdin");
    inicio.ArgumentList.Add(backupId.ToString());
    using var proceso = System.Diagnostics.Process.Start(inicio)
        ?? throw new InvalidOperationException("No se pudo iniciar RestoreWorker.");
    await proceso.StandardInput.WriteLineAsync(autorizacionFirmada);
    Console.Write("Confirmación final: escribí RESTAURAR SKYPLUS: ");
    string? confirmacion = Console.ReadLine();
    await proceso.StandardInput.WriteLineAsync(confirmacion == "RESTAURAR SKYPLUS" ? confirmacion : "CANCELAR");
    proceso.StandardInput.Close();
    await proceso.WaitForExitAsync();
    if (proceso.ExitCode != 0)
        throw new InvalidOperationException($"RestoreWorker terminó con código {proceso.ExitCode}. Revisá el estado persistente.");
}
