namespace SkyPlus.Domain.Backup;

public sealed class RestoreAuthorizationPayload
{
    public Guid OperationId { get; set; }

    public Guid BackupId { get; set; }

    public int FirstAdministratorId { get; set; }

    public int SecondAdministratorId { get; set; }

    public DateTime ApprovedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public string Purpose { get; set; } =
        "ProductionRestore";

    public string TargetDatabase { get; set; } =
        "skyplus";
}