namespace SkyPlus.API.DTOs;

public sealed class AutorizarRestauracionProduccionRequest
{
    public Guid BackupId { get; set; }

    public bool ConfirmarRestauracion { get; set; }

    public string EmailSegundoAdministrador { get; set; }
        = string.Empty;

    public string PasswordSegundoAdministrador { get; set; }
        = string.Empty;
}