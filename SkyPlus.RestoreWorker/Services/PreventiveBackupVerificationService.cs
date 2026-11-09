
using System.Security.Cryptography;

namespace SkyPlus.RestoreWorker.Services;

public static class PreventiveBackupVerificationService
{
    public static async Task<string> VerificarAsync(
        string rutaBackup,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rutaBackup))
            throw new ArgumentException(
                "La ruta del backup preventivo es obligatoria.");

        if (!File.Exists(rutaBackup))
            throw new FileNotFoundException(
                "No existe el backup preventivo.",
                rutaBackup);

        var archivo = new FileInfo(rutaBackup);

        if (archivo.Length == 0)
            throw new InvalidOperationException(
                "El backup preventivo está vacío.");

        await using var stream = new FileStream(
            rutaBackup,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 1024 * 1024,
            useAsync: true);

        using var sha256 = SHA256.Create();

        byte[] hash = await sha256.ComputeHashAsync(
            stream,
            cancellationToken);

        string hashHex = Convert.ToHexString(hash);

        Console.WriteLine(
            $"Backup preventivo: {archivo.Name}");

        Console.WriteLine(
            $"Tamaño: {archivo.Length:N0} bytes");

        Console.WriteLine(
            $"SHA256: {hashHex}");

        Console.WriteLine(
            "Archivo preventivo verificado.");

        return hashHex;
    }
}
