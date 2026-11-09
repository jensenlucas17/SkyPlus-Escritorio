namespace SkyPlus.RestoreWorker.Models;

public sealed class RestoreAuthorization
{
    public Guid OperationId { get; set; }

    public Guid BackupId { get; set; }

    public int FirstAdministratorId { get; set; }

    public int SecondAdministratorId { get; set; }

    public DateTime ApprovedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public bool Consumed { get; set; }
}
