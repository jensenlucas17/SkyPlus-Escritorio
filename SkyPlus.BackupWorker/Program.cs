using Microsoft.Extensions.Configuration;
using SkyPlus.API.Services;

try
{
    Console.WriteLine("SkyPlus - Backup automático");
    Console.WriteLine($"Inicio: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

    string rutaConfiguracion = Path.Combine(
        AppContext.BaseDirectory,
        "backupworker.json");

    if (!File.Exists(rutaConfiguracion))
    {
        throw new FileNotFoundException(
            "No se encontró backupworker.json.",
            rutaConfiguracion);
    }

    IConfiguration configuracion = new ConfigurationBuilder()
        .AddJsonFile(rutaConfiguracion, optional: false)
        .Build();

    Console.WriteLine($"Configuración: {rutaConfiguracion}");

    Console.WriteLine(
        "Límite de backups automáticos: " +
        (configuracion["Backup:MaxAutomaticBackups"] ?? "NO CONFIGURADO"));

    IBackupService backupService =
        new BackupService(configuracion);

    await backupService.GenerarBackupAutomaticoAsync();

    Console.WriteLine("Backup automático generado correctamente.");
    Console.WriteLine($"Fin: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
}
catch (Exception ex)
{
    Console.Error.WriteLine(
        $"Error al generar backup automático: {ex}");

    Environment.ExitCode = 1;
}
