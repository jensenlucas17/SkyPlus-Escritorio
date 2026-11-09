using System.Security.Cryptography;
using System.Text.Json;

namespace SkyPlus.RestoreWorker.Services
{
    public static class BackupVerifier
    {
        public static async Task<string> VerificarAsync(
            Guid idBackup,
            string backupDirectory,
            string auditDirectory,
            CancellationToken ct = default)
        {
            string rutaHistorial = Path.Combine(
                auditDirectory, "backups.json");

            if (!File.Exists(rutaHistorial))
                throw new InvalidOperationException(
                    "No existe el historial de backups.");

            using var documento = JsonDocument.Parse(
                await File.ReadAllTextAsync(rutaHistorial, ct));

            if (documento.RootElement.ValueKind !=
                JsonValueKind.Array)
                throw new InvalidOperationException(
                    "El historial tiene un formato inválido.");

            JsonElement? encontrado = null;

            foreach (var item in documento.RootElement.EnumerateArray())
            {
                if (item.GetProperty("Id").GetGuid() == idBackup)
                {
                    encontrado = item;
                    break;
                }
            }

            if (encontrado == null)
                throw new InvalidOperationException(
                    "El backup solicitado no existe.");

            var backup = encontrado.Value;

            string estado = backup.GetProperty("Estado")
                .GetString() ?? "";

            if (estado != "Disponible")
                throw new InvalidOperationException(
                    "El backup no está disponible.");

            string nombre = backup.GetProperty("Archivo")
                .GetString() ?? "";

            if (string.IsNullOrWhiteSpace(nombre) ||
                Path.GetFileName(nombre) != nombre ||
                !nombre.EndsWith(".sql",
                    StringComparison.OrdinalIgnoreCase) ||
                nombre.Contains("..") ||
                nombre.Contains('/') ||
                nombre.Contains('\\') ||
                Path.IsPathRooted(nombre))
            {
                throw new InvalidOperationException(
                    "Nombre de backup inválido.");
            }

            string rutaArchivo = Path.Combine(
                backupDirectory, nombre);

            if (!File.Exists(rutaArchivo))
                throw new InvalidOperationException(
                    "No existe el archivo del backup.");

            long tamanoEsperado = backup
                .GetProperty("TamanoBytes").GetInt64();

            string hashEsperado = backup
                .GetProperty("Sha256").GetString() ?? "";

            await using var archivo = new FileStream(
                rutaArchivo,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            if (archivo.Length == 0 ||
                archivo.Length != tamanoEsperado)
                throw new InvalidOperationException(
                    "El tamaño del backup no coincide.");

            byte[] hash = await SHA256.HashDataAsync(
                archivo, ct);

            string hashActual = Convert.ToHexString(hash);

            if (!string.Equals(
                hashActual,
                hashEsperado,
                StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "El SHA-256 del backup no coincide.");

            return rutaArchivo;
        }
    }
}