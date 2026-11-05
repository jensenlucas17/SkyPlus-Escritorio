namespace SkyPlus.Desktop.Models
{
    public class RestaurarBackupResponse
    {
        public string Mensaje { get; set; } = string.Empty;
        public ResultadoRestauracionResponse? Resultado { get; set; }
    }

    public class ResultadoRestauracionResponse
    {
        public Guid IdBackup { get; set; }
        public string BaseDestino { get; set; } = string.Empty;
        public int IdAdministradorSolicitante { get; set; }
        public int IdAdministradorAutorizante { get; set; }
        public DateTime FechaUtc { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
