using System.Text.Json;
using SkyPlus.RestoreWorker.Models;

namespace SkyPlus.RestoreWorker.Services;

public static class RestoreStateService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task<RestoreState?> LeerAsync(
        string rutaEstado)
    {
        if (!File.Exists(rutaEstado))
            return null;

        string contenido = await File.ReadAllTextAsync(rutaEstado);

        if (string.IsNullOrWhiteSpace(contenido))
            throw new InvalidOperationException(
                "El archivo de estado está vacío. " +
                "Se requiere revisión administrativa.");

        var estado = JsonSerializer.Deserialize<RestoreState>(
            contenido,
            JsonOptions);

        if (estado is null ||
            estado.OperationId == Guid.Empty ||
            estado.BackupId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "El archivo de estado es inválido. " +
                "Se requiere revisión administrativa.");
        }

        return estado;
    }

    public static async Task GuardarAsync(
        string rutaEstado,
        RestoreState estado)
    {
        estado.UpdatedAtUtc = DateTime.UtcNow;

        string? directorio = Path.GetDirectoryName(rutaEstado);

        if (string.IsNullOrWhiteSpace(directorio))
            throw new InvalidOperationException(
                "La ruta del estado no es válida.");

        Directory.CreateDirectory(directorio);

        string rutaTemporal = rutaEstado + ".tmp";

        string contenido = JsonSerializer.Serialize(
            estado,
            JsonOptions);

        await File.WriteAllTextAsync(
            rutaTemporal,
            contenido);

        File.Move(
            rutaTemporal,
            rutaEstado,
            overwrite: true);
    }


    public static async Task VerificarOperacionPendienteAsync(
        string rutaEstado)
    {
        RestoreState? estado = await LeerAsync(rutaEstado);

        if (estado is null)
            return;

        bool preliminarFinalizada =
            estado.OperationType == "PreliminaryCheck" &&
            estado.PreliminaryChecksCompleted &&
            estado.Phase == "PreliminaryChecksCompleted" &&
            !estado.ReadOnlyActivated &&
            !estado.RestorationStarted;

        bool restauracionFinalizada =
            estado.OperationType == "ProductionRestore" &&
            estado.RestorationCompleted &&
            estado.Phase == "Completed" &&
            !estado.ReadOnlyActivated;

        bool pruebaReadOnlyFinalizada =
            estado.OperationType == "ReadOnlyTest" &&
            estado.Phase == "ReadOnlyTestCompleted" &&
            !estado.ReadOnlyActivated &&
            !estado.RestorationStarted;

        bool pruebaReadOnlyRecuperada =
            estado.OperationType == "ReadOnlyTest" &&
            estado.Phase == "ReadOnlyTestFailedRecovered" &&
            !estado.ReadOnlyActivated &&
            !estado.RestorationStarted;

        if (preliminarFinalizada ||
            restauracionFinalizada ||
            pruebaReadOnlyFinalizada)
        {
            return;
        }

        throw new InvalidOperationException(
            "Existe una operación pendiente o interrumpida. " +
            $"Operación: {estado.OperationId}. " +
            $"Tipo: {estado.OperationType}. " +
            $"Fase: {estado.Phase}. " +
            "Se requiere revisión administrativa.");
    }


    public static async Task ActualizarFaseAsync(
        string rutaEstado,
        RestoreState estado,
        string nuevaFase)
    {
        estado.Phase = nuevaFase;

        await GuardarAsync(rutaEstado, estado);

        Console.WriteLine(
            $"Estado de restauración: {nuevaFase}");
    }
}