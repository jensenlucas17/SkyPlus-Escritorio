using System.Text.Json;

namespace SkyPlus.RestoreWorker.Services;

public static class ProductionRestoreStateService
{
    public sealed class ProductionRestoreState
    {
        public Guid OperationId { get; set; }
        public Guid BackupId { get; set; }

        public string Phase { get; set; } = "";

        public string PreventiveBackupPath { get; set; } = "";

        public bool ApplicationLockAttempted { get; set; }
        public bool ImportStarted { get; set; }
        public bool ImportCompleted { get; set; }
        public bool VerificationCompleted { get; set; }
        public bool ApplicationUnlocked { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public string? Error { get; set; }
        public string? PreventiveBackupSha256 { get; set; }
    }

    private static async Task GuardarAsync(
        string stateFile,
        ProductionRestoreState state)
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

        await File.WriteAllTextAsync(
            temporal,
            json);

        File.Move(
            temporal,
            stateFile,
            overwrite: true);
    }

    public static async Task<ProductionRestoreState?> LeerAsync(
        string stateFile)
    {
        if (!File.Exists(stateFile))
            return null;

        string json = await File.ReadAllTextAsync(stateFile);

        return JsonSerializer.Deserialize<ProductionRestoreState>(
            json);
    }

    public static async Task VerificarSinPendientesAsync(
        string stateFile)
    {
        var state = await LeerAsync(stateFile);

        if (state is null)
            return;

        if (state.Phase == "Completed" &&
            EstadoCompletadoValido(state))
        {
            return;
        }

        throw new InvalidOperationException(
            "Existe una restauración de producción pendiente " +
            "o un estado final inconsistente. " +
            $"Operación: {state.OperationId}. " +
            $"Fase: {state.Phase}. " +
            "Se requiere revisión administrativa.");
    }

    public static async Task<ProductionRestoreState>
        ValidarContinuacionAsync(
            string stateFile,
            Guid operationId,
            Guid backupId)
    {
        var state = await LeerAsync(stateFile);

        if (state is null)
            throw new InvalidOperationException(
                "No existe una operación de producción registrada.");

        if (state.OperationId != operationId)
            throw new InvalidOperationException(
                "El ID de operación no coincide.");

        if (state.BackupId != backupId)
            throw new InvalidOperationException(
                "El ID del backup no coincide.");

        if (state.Phase != "PreventiveBackupVerified")
            throw new InvalidOperationException(
                "La operación no está en la fase esperada. " +
                $"Fase actual: {state.Phase}");

        if (state.ApplicationLockAttempted ||
            state.ImportStarted ||
            state.ImportCompleted ||
            state.VerificationCompleted ||
            state.ApplicationUnlocked)
        {
            throw new InvalidOperationException(
                "El estado contiene indicadores de ejecución " +
                "incompatibles con la preparación.");
        }

        if (string.IsNullOrWhiteSpace(
            state.PreventiveBackupPath))
        {
            throw new InvalidOperationException(
                "No se registró la ruta del backup preventivo.");
        }

        if (string.IsNullOrWhiteSpace(
            state.PreventiveBackupSha256))
        {
            throw new InvalidOperationException(
                "No se registró el SHA-256 preventivo.");
        }

        if (!File.Exists(state.PreventiveBackupPath))
            throw new FileNotFoundException(
                "No existe el backup preventivo registrado.",
                state.PreventiveBackupPath);

        string sha256Actual =
            await PreventiveBackupVerificationService
                .VerificarAsync(state.PreventiveBackupPath);

        if (!string.Equals(
            sha256Actual,
            state.PreventiveBackupSha256,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "El backup preventivo fue modificado " +
                "después de su verificación.");
        }

        string rutaHash =
            state.PreventiveBackupPath + ".sha256";

        if (!File.Exists(rutaHash))
            throw new FileNotFoundException(
                "No existe el archivo SHA-256 preventivo.",
                rutaHash);

        string hashGuardado =
            (await File.ReadAllTextAsync(rutaHash)).Trim();

        if (!string.Equals(
            sha256Actual,
            hashGuardado,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "El archivo SHA-256 preventivo no coincide.");
        }

        Console.WriteLine(
            "Operación pendiente validada correctamente.");

        Console.WriteLine(
            $"Operación: {state.OperationId}");

        Console.WriteLine(
            $"Backup original: {state.BackupId}");

        Console.WriteLine(
            "Backup preventivo: integridad correcta.");

        return state;
    }

    public static async Task CrearAsync(
    string stateFile,
    ProductionRestoreState state)
    {
        var anterior = await LeerAsync(stateFile);

        if (anterior is not null)
        {
            if (anterior.Phase != "Completed")
            {
                throw new InvalidOperationException(
                    "Existe una restauración pendiente.");
            }

            if (!EstadoCompletadoValido(anterior))
            {
                throw new InvalidOperationException(
                    "La operación anterior figura como completada, " +
                    "pero sus indicadores no son coherentes.");
            }
        }

        if (state.OperationId == Guid.Empty ||
            state.BackupId == Guid.Empty ||
            state.Phase != "PreventiveBackupStarted" ||
            state.ApplicationLockAttempted ||
            state.ImportStarted ||
            state.ImportCompleted ||
            state.VerificationCompleted ||
            state.ApplicationUnlocked ||
            !string.IsNullOrWhiteSpace(state.PreventiveBackupPath) ||
            !string.IsNullOrWhiteSpace(state.PreventiveBackupSha256) ||
            !string.IsNullOrWhiteSpace(state.Error))
        {
            throw new InvalidOperationException(
                "El estado inicial de restauración no es válido.");
        }

        await GuardarAsync(stateFile, state);
    }

    // Las actualizaciones deben ejecutarse bajo el bloqueo exclusivo restore.lock
    // del RestoreWorker. Este servicio no sustituye ese bloqueo entre procesos.
    public static async Task ActualizarAsync(
        string stateFile,
        ProductionRestoreState nuevoEstado)
    {
        var actual = await LeerAsync(stateFile)
            ?? throw new InvalidOperationException(
                "No existe una restauración para actualizar.");

        if (actual.OperationId != nuevoEstado.OperationId ||
            actual.BackupId != nuevoEstado.BackupId ||
            actual.OperationId == Guid.Empty ||
            actual.BackupId == Guid.Empty)
            throw new InvalidOperationException(
                "La operación o el backup no coinciden con el estado registrado.");

        if (!string.IsNullOrWhiteSpace(actual.PreventiveBackupPath) &&
            !string.Equals(actual.PreventiveBackupPath,
                nuevoEstado.PreventiveBackupPath,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "No se puede modificar la ruta del backup preventivo.");

        if (!string.IsNullOrWhiteSpace(actual.PreventiveBackupSha256) &&
            !string.Equals(actual.PreventiveBackupSha256,
                nuevoEstado.PreventiveBackupSha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "No se puede modificar el SHA-256 preventivo.");

        if ((actual.ApplicationLockAttempted && !nuevoEstado.ApplicationLockAttempted) ||
            (actual.ImportStarted && !nuevoEstado.ImportStarted) ||
            (actual.ImportCompleted && !nuevoEstado.ImportCompleted) ||
            (actual.VerificationCompleted && !nuevoEstado.VerificationCompleted) ||
            (actual.ApplicationUnlocked && !nuevoEstado.ApplicationUnlocked))
            throw new InvalidOperationException(
                "No se pueden revertir indicadores de ejecución.");

        bool transicionValida = (actual.Phase, nuevoEstado.Phase) switch
        {
            ("PreventiveBackupStarted", "PreventiveBackupGenerated") => true,
            ("PreventiveBackupGenerated", "PreventiveBackupVerified") => true,
            ("PreventiveBackupVerified", "ApplicationLockAttempted") => true,
            ("ApplicationLockAttempted", "ApplicationIsolated") => true,
            ("ApplicationIsolated", "ImportStarted") => true,
            ("ImportStarted", "ImportCompleted") => true,
            ("ImportCompleted", "VerificationCompleted") => true,
            ("VerificationCompleted", "ApplicationUnlocked") => true,
            ("ApplicationUnlocked", "Completed") => true,
            ("PreventiveBackupStarted", "Failed") => true,
            ("PreventiveBackupGenerated", "Failed") => true,
            ("PreventiveBackupVerified", "Failed") => true,
            ("ApplicationLockAttempted", "RequiresRecovery") => true,
            ("ApplicationIsolated", "RequiresRecovery") => true,
            ("ImportStarted", "RequiresRecovery") => true,
            ("ImportCompleted", "RequiresRecovery") => true,
            ("VerificationCompleted", "RequiresRecovery") => true,
            ("ApplicationUnlocked", "RequiresRecovery") => true,
            _ => false
        };

        if (!transicionValida)
            throw new InvalidOperationException(
                $"Transición de fase no permitida: {actual.Phase} → {nuevoEstado.Phase}");

        // En una transición de error no se pueden inventar hitos completados.
        if (nuevoEstado.Phase is "Failed" or "RequiresRecovery")
        {
            if (actual.ApplicationLockAttempted != nuevoEstado.ApplicationLockAttempted ||
                actual.ImportStarted != nuevoEstado.ImportStarted ||
                actual.ImportCompleted != nuevoEstado.ImportCompleted ||
                actual.VerificationCompleted != nuevoEstado.VerificationCompleted ||
                actual.ApplicationUnlocked != nuevoEstado.ApplicationUnlocked ||
                string.IsNullOrWhiteSpace(nuevoEstado.Error))
                throw new InvalidOperationException(
                    "El estado de error debe conservar los indicadores y registrar el motivo.");
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(nuevoEstado.Error))
                throw new InvalidOperationException(
                    "Una fase normal no puede contener un error registrado.");

            // Cada fase declara exactamente qué hitos deben estar persistidos.
            var esperado = nuevoEstado.Phase switch
            {
                "PreventiveBackupGenerated" or "PreventiveBackupVerified"
                    => (false, false, false, false, false),
                "ApplicationLockAttempted" or "ApplicationIsolated"
                    => (true, false, false, false, false),
                "ImportStarted" => (true, true, false, false, false),
                "ImportCompleted" => (true, true, true, false, false),
                "VerificationCompleted" => (true, true, true, true, false),
                "ApplicationUnlocked" or "Completed"
                    => (true, true, true, true, true),
                _ => throw new InvalidOperationException("Fase desconocida.")
            };

            if ((nuevoEstado.ApplicationLockAttempted,
                 nuevoEstado.ImportStarted,
                 nuevoEstado.ImportCompleted,
                 nuevoEstado.VerificationCompleted,
                 nuevoEstado.ApplicationUnlocked) != esperado)
                throw new InvalidOperationException(
                    "Los indicadores no corresponden a la fase solicitada.");
        }

        // Un estado de emergencia debe poder registrarse incluso si el respaldo
        // preventivo se perdió o su integridad ya no puede verificarse.
        if (nuevoEstado.Phase is not ("Failed" or "RequiresRecovery"))
        {
            if (string.IsNullOrWhiteSpace(nuevoEstado.PreventiveBackupPath) ||
                !File.Exists(nuevoEstado.PreventiveBackupPath))
                throw new FileNotFoundException(
                    "No existe el backup preventivo registrado.",
                    nuevoEstado.PreventiveBackupPath);
        }

        if (nuevoEstado.Phase is not ("PreventiveBackupGenerated" or "Failed" or "RequiresRecovery"))
        {
            if (string.IsNullOrWhiteSpace(nuevoEstado.PreventiveBackupSha256))
                throw new InvalidOperationException(
                    "No se registró el SHA-256 preventivo.");

            string sha256Actual = await PreventiveBackupVerificationService
                .VerificarAsync(nuevoEstado.PreventiveBackupPath);
            string rutaHash = nuevoEstado.PreventiveBackupPath + ".sha256";

            if (!File.Exists(rutaHash))
                throw new FileNotFoundException(
                    "No existe el archivo SHA-256 preventivo.", rutaHash);

            string hashGuardado = (await File.ReadAllTextAsync(rutaHash)).Trim();
            if (!string.Equals(sha256Actual, nuevoEstado.PreventiveBackupSha256,
                    StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(sha256Actual, hashGuardado,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "La integridad del backup preventivo no coincide.");
        }

        await GuardarAsync(stateFile, nuevoEstado);
    }

    private static bool EstadoCompletadoValido(
    ProductionRestoreState state)
    {
        return state.Phase == "Completed" &&
               state.OperationId != Guid.Empty &&
               state.BackupId != Guid.Empty &&
               state.ApplicationLockAttempted &&
               state.ImportStarted &&
               state.ImportCompleted &&
               state.VerificationCompleted &&
               state.ApplicationUnlocked &&
               !string.IsNullOrWhiteSpace(state.PreventiveBackupPath) &&
               !string.IsNullOrWhiteSpace(state.PreventiveBackupSha256) &&
               string.IsNullOrWhiteSpace(state.Error);
    }
}