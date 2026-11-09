using System.Text.Json;

namespace SkyPlus.RestoreWorker.Services;

public static class ApplicationLockRecoveryService
{
    private sealed class LockState
    {
        public string Status { get; set; } = "";
        public DateTime UpdatedAtUtc { get; set; }
    }

    public static async Task RegistrarIntentoAsync(
        string stateFile)
    {
        if (await RequiereRecuperacionAsync(stateFile))
        {
            throw new InvalidOperationException(
                "Ya existe un bloqueo pendiente de recuperación.");
        }

        await GuardarAsync(stateFile, new LockState
        {
            Status = "LockAttempted"
        });
    }

    public static async Task RegistrarDesbloqueoAsync(
        string stateFile)
    {
        await GuardarAsync(stateFile, new LockState
        {
            Status = "Unlocked"
        });
    }

    public static async Task<bool> RequiereRecuperacionAsync(
        string stateFile)
    {
        if (!File.Exists(stateFile))
            return false;

        string json = await File.ReadAllTextAsync(stateFile);

        var state = JsonSerializer.Deserialize<LockState>(json)
            ?? throw new InvalidOperationException(
                "Estado de bloqueo inválido.");

        return state.Status switch
        {
            "Unlocked" => false,
            "LockAttempted" => true,
            _ => throw new InvalidOperationException(
                "Estado de bloqueo desconocido: " +
                state.Status)
        };
    }

    public static async Task VerificarDesbloqueoYRegistrarAsync(
        string stateFile,
        string mariaDbBinPath,
        string credentialsFile)
    {
        bool bloqueada =
            await MariaDbApplicationLockService.EstaBloqueadaAsync(
                mariaDbBinPath,
                credentialsFile);

        if (bloqueada)
        {
            throw new InvalidOperationException(
                "La cuenta skyplus_app continúa bloqueada. " +
                "No se puede registrar el desbloqueo.");
        }

        await RegistrarDesbloqueoAsync(stateFile);
    }

    private static async Task GuardarAsync(
        string stateFile,
        LockState state)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(
                Path.GetFullPath(stateFile))!);

        state.UpdatedAtUtc = DateTime.UtcNow;

        string json = JsonSerializer.Serialize(
            state,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        string temporal = stateFile + ".tmp";

        await File.WriteAllTextAsync(temporal, json);

        File.Move(
            temporal,
            stateFile,
            overwrite: true);
    }
}