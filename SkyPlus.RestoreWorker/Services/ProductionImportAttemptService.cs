using System.Text.Json;

namespace SkyPlus.RestoreWorker.Services;

/// <summary>
/// Marca de un solo uso. NO borrar ni restablecer automáticamente el archivo.
/// Debe residir en un directorio protegido por ACL y persistente.
/// </summary>
public static class ProductionImportAttemptService
{
    public static string ObtenerRuta(string productionStateFile, Guid operationId)
    {
        if (operationId == Guid.Empty)
            throw new InvalidOperationException("OperationId inválido.");
        return Path.GetFullPath(productionStateFile) + ".import-attempt-" + operationId.ToString("N") + ".json";
    }

    public static async Task RegistrarAsync(string productionStateFile, Guid operationId, Guid backupId)
    {
        if (backupId == Guid.Empty)
            throw new InvalidOperationException("BackupId inválido.");

        string ruta = ObtenerRuta(productionStateFile, operationId);
        // No se crea el directorio aquí: debe existir y tener ACL seguras.
        if (!Directory.Exists(Path.GetDirectoryName(ruta)))
            throw new DirectoryNotFoundException("No existe el directorio de auditoría protegido.");

        byte[] contenido = JsonSerializer.SerializeToUtf8Bytes(new
        {
            OperationId = operationId,
            BackupId = backupId,
            AttemptedAtUtc = DateTime.UtcNow
        });

        try
        {
            // CreateNew garantiza que solo un proceso pueda consumir el intento.
            await using var stream = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write,
                FileShare.None, bufferSize: 4096, options: FileOptions.WriteThrough);
            await stream.WriteAsync(contenido);
            stream.Flush(flushToDisk: true);
        }
        catch (IOException ex) when (File.Exists(ruta))
        {
            throw new InvalidOperationException(
                "Ya existe un intento de importación registrado para esta operación.", ex);
        }
        // Un fallo de escritura deja la operación en estado incierto.
        // Nunca borrar el marcador automáticamente para reintentar.
    }
}
