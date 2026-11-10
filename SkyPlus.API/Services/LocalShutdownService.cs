using System.IO.Pipes;
using System.Security.Cryptography;

namespace SkyPlus.API.Services;

// Canal exclusivamente local: no expone un endpoint HTTP para apagar la API.
public sealed class LocalShutdownService : BackgroundService
{
    public const string PipeName = "SkyPlus.Api.RestoreShutdown.v1";
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<LocalShutdownService> _logger;

    public LocalShutdownService(IHostApplicationLifetime lifetime, ILogger<LocalShutdownService> logger)
    {
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SkyPlus");
        Directory.CreateDirectory(dir);
        string tokenPath = Path.Combine(dir, "api-shutdown.token");
        string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        try
        {
            await File.WriteAllTextAsync(tokenPath, token, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                using var pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await pipe.WaitForConnectionAsync(stoppingToken);
                using var reader = new StreamReader(pipe, leaveOpen: true);
                using var writer = new StreamWriter(pipe, leaveOpen: true) { AutoFlush = true };
                string? received = await reader.ReadLineAsync(stoppingToken);
                if (received is not null && CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(received), System.Text.Encoding.UTF8.GetBytes(token)))
                {
                    await writer.WriteLineAsync("OK");
                    _logger.LogWarning("Cierre local solicitado por RestoreLauncher.");
                    _lifetime.StopApplication();
                    break;
                }
                await writer.WriteLineAsync("DENIED");
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally
        {
            try { if (File.Exists(tokenPath) && await File.ReadAllTextAsync(tokenPath) == token) File.Delete(tokenPath); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
