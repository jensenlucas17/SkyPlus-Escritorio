using System.Management;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Sockets;
using SkyPlus.RestoreWorker.Services;

internal static class RestoreAutomation
{
    private const string MaintenanceFile = @"C:\SkyPlus\Config\mantenimiento.lock";
    private const string StateFile = @"C:\SkyPlus\Auditoria\production-restore-state.json";
    private const string PipeName = "SkyPlus.Api.RestoreShutdown.v1";

    public static async Task RunAsync(Guid operationId, Guid backupId, string initialAuthorization,
        string continuationAuthorization, DateTime initialExpiry, DateTime continuationExpiry)
    {
        string solution = FindSolutionRoot();
        string api = FindBuild(solution, "SkyPlus.API", "SkyPlus.API.dll");
        string desktop = FindBuild(solution, "SkyPlus.Desktop", "SkyPlus.Desktop.exe");
        string worker = FindBuild(solution, "SkyPlus.RestoreWorker", "SkyPlus.RestoreWorker.exe");

        // No sobrescribir un bloqueo existente ni continuar una operación pendiente.
        await ProductionRestoreStateService.VerificarSinPendientesAsync(StateFile);
        if (File.Exists(MaintenanceFile))
            throw new InvalidOperationException("Ya existe mantenimiento.lock. Revisá el estado antes de continuar.");
        if (DateTime.UtcNow >= initialExpiry || DateTime.UtcNow >= continuationExpiry)
            throw new InvalidOperationException("Vencieron las autorizaciones de restauración.");

        Console.WriteLine("Solicitando cierre ordenado de SkyPlus.API...");
        await ShutdownApiAsync();
        Console.WriteLine("Esperando cierre de API, Desktop y BackupWorker...");
        await WaitForSkyPlusExitAsync(TimeSpan.FromSeconds(45));

        // Se crea solo cuando los procesos dejaron de usar la base de datos.
        Directory.CreateDirectory(Path.GetDirectoryName(MaintenanceFile)!);
        await using (var file = new FileStream(MaintenanceFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes($"RestoreOperation={operationId}\nStartedUtc={DateTime.UtcNow:O}\n");
            await file.WriteAsync(data);
        }

        Console.WriteLine("Mantenimiento activado. Comprobando aislamiento...");
        try
        {
            await MariaDbIsolationVerifier.VerificarAsync(@"C:\SkyPlus\Config\monitor.cnf", "skyplus");
            if (DateTime.UtcNow >= initialExpiry || DateTime.UtcNow >= continuationExpiry)
                throw new InvalidOperationException("Autorización vencida antes del respaldo preventivo.");

            await RunWorkerAsync(worker, "--preparar-produccion-stdin", backupId, initialAuthorization);
            Console.WriteLine("Generando backup preventivo...");
            await RunWorkerAsync(worker, "--backup-preventivo-stdin", backupId, initialAuthorization);
            await ProductionRestoreStateService.ValidarContinuacionAsync(StateFile, operationId, backupId);
            if (DateTime.UtcNow >= continuationExpiry)
                throw new InvalidOperationException("Autorización de continuación vencida. La operación queda pendiente.");

            Console.WriteLine("ATENCIÓN: se reemplazarán los datos de skyplus.");
            Console.Write("Escribí RESTAURAR SKYPLUS para confirmar: ");
            if (Console.ReadLine() != "RESTAURAR SKYPLUS")
                throw new OperationCanceledException("Cancelado. El backup preventivo y la operación pendiente se conservan.");

            // El Worker conserva la validación RSA, el aislamiento y su confirmación destructiva.
            await RunWorkerAsync(worker, "--continuar-produccion-stdin", backupId,
                continuationAuthorization, "RESTAURAR SKYPLUS");
            var state = await ProductionRestoreStateService.LeerAsync(StateFile);
            if (state is null || state.OperationId != operationId || state.BackupId != backupId ||
                state.Phase != "Completed" || !state.ImportCompleted ||
                !state.VerificationCompleted || !state.ApplicationUnlocked)
                throw new InvalidOperationException("El Worker terminó sin un estado Completed verificable.");

            File.Delete(MaintenanceFile);
            Console.WriteLine("Restauración verificada. Reiniciando API...");
            StartApi(api);
            await WaitForApiAsync(TimeSpan.FromSeconds(35));
            Console.WriteLine("API iniciada. Abriendo Desktop...");
            _ = Process.Start(new ProcessStartInfo(desktop) { WorkingDirectory = Path.GetDirectoryName(desktop)!, UseShellExecute = true })
                ?? throw new InvalidOperationException("No se pudo iniciar Desktop.");
            Console.WriteLine("SkyPlus restaurado y reiniciado correctamente.");
        }
        catch
        {
            // Nunca levantar SkyPlus automáticamente con una restauración incierta.
            Console.Error.WriteLine("Restauración/reinicio incompleto. Revisá el estado persistente y mantenimiento.lock.");
            throw;
        }
    }

    private static async Task ShutdownApiAsync()
    {
        string tokenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SkyPlus", "api-shutdown.token");
        if (!File.Exists(tokenPath))
            throw new InvalidOperationException("No se encontró el canal de cierre de API. Recompilá y ejecutá la API actualizada.");
        string token = (await File.ReadAllTextAsync(tokenPath)).Trim();
        using var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await pipe.ConnectAsync(timeout.Token);
        using var writer = new StreamWriter(pipe, leaveOpen: true) { AutoFlush = true };
        using var reader = new StreamReader(pipe, leaveOpen: true);
        await writer.WriteLineAsync(token);
        if (await reader.ReadLineAsync(timeout.Token) != "OK")
            throw new InvalidOperationException("La API rechazó la solicitud de cierre.");
    }

    private static async Task WaitForSkyPlusExitAsync(TimeSpan timeout)
    {
        DateTime end = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < end)
        {
            bool running = false;
            using var search = new ManagementObjectSearcher("SELECT Name, CommandLine FROM Win32_Process");
            using var results = search.Get();
            foreach (ManagementObject process in results)
            {
                using (process)
                {
                    string name = process["Name"]?.ToString() ?? "";
                    string command = process["CommandLine"]?.ToString() ?? "";
                    if (name.Equals("SkyPlus.API.exe", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("SkyPlus.Desktop.exe", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("SkyPlus.BackupWorker.exe", StringComparison.OrdinalIgnoreCase) ||
                        (name.Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase) &&
                         (command.Contains("SkyPlus.API", StringComparison.OrdinalIgnoreCase) ||
                          command.Contains("SkyPlus.Desktop", StringComparison.OrdinalIgnoreCase) ||
                          command.Contains("SkyPlus.BackupWorker", StringComparison.OrdinalIgnoreCase))))
                    {
                        running = true;
                        break;
                    }
                }
            }
            if (!running) return;
            await Task.Delay(500);
        }
        throw new TimeoutException("SkyPlus sigue ejecutándose. Cerrá las sesiones de depuración y reintentá.");
    }

    private static async Task RunWorkerAsync(string exe, string command, Guid backupId, string signed, string? confirmation = null)
    {
        var psi = new ProcessStartInfo(exe) { WorkingDirectory = Path.GetDirectoryName(exe)!,
            UseShellExecute = false, RedirectStandardInput = true };
        psi.ArgumentList.Add(command);
        psi.ArgumentList.Add(backupId.ToString());
        using var process = Process.Start(psi) ?? throw new InvalidOperationException("No se pudo iniciar RestoreWorker.");
        await process.StandardInput.WriteLineAsync(signed);
        if (confirmation is not null) await process.StandardInput.WriteLineAsync(confirmation);
        process.StandardInput.Close();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"RestoreWorker falló en {command} (código {process.ExitCode}).");
    }

    private static void StartApi(string dll)
    {
        var psi = new ProcessStartInfo("dotnet") { WorkingDirectory = Path.GetDirectoryName(dll)!,
            UseShellExecute = false, CreateNoWindow = false };
        psi.ArgumentList.Add(dll);
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        psi.Environment["ASPNETCORE_URLS"] = "https://localhost:7198;http://localhost:5004";
        _ = Process.Start(psi) ?? throw new InvalidOperationException("No se pudo reiniciar API.");
    }

    private static async Task WaitForApiAsync(TimeSpan timeout)
    {
        DateTime end = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < end)
        {
            try
            {
                using var client = new TcpClient();
                using var ct = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                await client.ConnectAsync("127.0.0.1", 5004, ct.Token);
                return;
            }
            catch (SocketException) { }
            catch (OperationCanceledException) { }
            await Task.Delay(500);
        }
        throw new TimeoutException("La API no inició en http://localhost:5004.");
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "SkyPlus.sln"))) return dir.FullName;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("No se encontró SkyPlus.sln. Ejecutá desde la solución de Visual Studio.");
    }

    private static string FindBuild(string root, string project, string file)
    {
        string bin = Path.Combine(root, project, "bin");
        if (!Directory.Exists(bin)) throw new DirectoryNotFoundException($"Compilá {project} antes de restaurar.");
        string[] matches = Directory.GetFiles(bin, file, SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}publish{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.Contains($"{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(File.GetLastWriteTimeUtc).ToArray();
        return matches.FirstOrDefault() ?? throw new FileNotFoundException($"No se encontró {file} compilado.");
    }
}
