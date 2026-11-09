namespace SkyPlus.RestoreWorker.Models;

public sealed class RestoreState
{
    public Guid OperationId { get; set; }

    public Guid BackupId { get; set; }

    public string Phase { get; set; } = "Created";

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool ReadOnlyActivated { get; set; }

    public bool RestorationStarted { get; set; }

    public bool RestorationCompleted { get; set; }

    public string? Error { get; set; }

    public bool PreliminaryChecksCompleted { get; set; }

    public string OperationType { get; set; } = "PreliminaryCheck";
}
