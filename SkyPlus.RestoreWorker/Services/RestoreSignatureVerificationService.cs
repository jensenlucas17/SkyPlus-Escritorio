using System.Security.Cryptography;
using System.Text.Json;
using SkyPlus.Domain.Backup;

namespace SkyPlus.RestoreWorker.Services;

public static class RestoreSignatureVerificationService
{
    public static RestoreAuthorizationPayload Verificar(
        string documentoFirmado,
        string rutaClavePublica)
    {
        if (!File.Exists(rutaClavePublica))
        {
            throw new FileNotFoundException(
                "No existe la clave pública de restauración.");
        }

        using JsonDocument documento =
            JsonDocument.Parse(documentoFirmado);

        string payloadBase64 = documento.RootElement
            .GetProperty("Payload").GetString()
            ?? throw new InvalidOperationException(
                "El documento no contiene datos.");

        string signatureBase64 = documento.RootElement
            .GetProperty("Signature").GetString()
            ?? throw new InvalidOperationException(
                "El documento no contiene firma.");

        byte[] datos = Convert.FromBase64String(
            payloadBase64);

        byte[] firma = Convert.FromBase64String(
            signatureBase64);

        using RSA rsa = RSA.Create();

        rsa.ImportFromPem(
            File.ReadAllText(rutaClavePublica));

        bool firmaValida = rsa.VerifyData(
            datos,
            firma,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pss);

        if (!firmaValida)
        {
            throw new InvalidOperationException(
                "La firma digital de restauración no es válida.");
        }

        var autorizacion =
            JsonSerializer.Deserialize<RestoreAuthorizationPayload>(
                datos);

        if (autorizacion == null)
        {
            throw new InvalidOperationException(
                "No se pudo interpretar la autorización.");
        }

        return autorizacion;
    }
}