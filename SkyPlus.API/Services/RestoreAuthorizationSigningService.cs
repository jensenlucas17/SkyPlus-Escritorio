using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SkyPlus.Domain.Backup;

namespace SkyPlus.API.Services;

public sealed class RestoreAuthorizationSigningService
{
    private readonly string _privateKeyPath;

    public RestoreAuthorizationSigningService(
        IConfiguration configuration)
    {
        _privateKeyPath = configuration[
            "RestoreAuthorization:PrivateKeyPath"]
            ?? throw new InvalidOperationException(
                "No se configuró la clave privada de restauración.");
    }

    public string Firmar(RestoreAuthorizationPayload payload)
    {
        if (!File.Exists(_privateKeyPath))
        {
            throw new FileNotFoundException(
                "No existe la clave privada de restauración.");
        }

        byte[] datos = JsonSerializer.SerializeToUtf8Bytes(
            payload);

        using RSA rsa = RSA.Create();

        rsa.ImportFromPem(
            File.ReadAllText(_privateKeyPath));

        byte[] firma = rsa.SignData(
            datos,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pss);

        var documento = new
        {
            Payload = Convert.ToBase64String(datos),
            Signature = Convert.ToBase64String(firma)
        };

        return JsonSerializer.Serialize(documento);
    }
}