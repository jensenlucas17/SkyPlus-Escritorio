using System.Text.Json;

namespace SkyPlus.RestoreWorker.Services;

public static class RestoreAuthorizationConsumptionService
{
    public static async Task ConsumirAsync(
        Guid operationId,
        Guid backupId,
        string auditDirectory)
    {
        if (operationId == Guid.Empty)
            throw new InvalidOperationException(
                "El identificador de operación es inválido.");

        if (backupId == Guid.Empty)
            throw new InvalidOperationException(
                "El identificador del backup es inválido.");

        string directory = Path.Combine(
            auditDirectory,
            "AutorizacionesConsumidas");

        Directory.CreateDirectory(directory);

        string filePath = Path.Combine(
            directory,
            $"{operationId:D}.used");

        var registro = new
        {
            OperationId = operationId,
            BackupId = backupId,
            ConsumedAtUtc = DateTime.UtcNow,
            Purpose = "ProductionRestore"
        };

        string json = JsonSerializer.Serialize(
            registro,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        try
        {
            await using var stream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                4096,
                FileOptions.WriteThrough);

            await using var writer = new StreamWriter(stream);

            await writer.WriteAsync(json);
            await writer.FlushAsync();
            stream.Flush(flushToDisk: true);
        }
        catch (IOException ex) when (File.Exists(filePath))
        {
            throw new InvalidOperationException(
                "Esta autorización ya fue utilizada. " +
                "No puede reutilizarse para otra restauración.",
                ex);
        }

        Console.WriteLine(
            "Autorización de producción consumida: " +
            operationId);
    }
}