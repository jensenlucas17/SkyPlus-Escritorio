namespace SkyPlus.API.DTOs;

public sealed class AutorizarContinuacionProduccionRequest
{
    public Guid OperationId { get; set; }
    public Guid BackupId { get; set; }
    public bool ConfirmarRestauracion { get; set; }
    public string EmailSegundoAdministrador { get; set; } = "";
    public string PasswordSegundoAdministrador { get; set; } = "";
}