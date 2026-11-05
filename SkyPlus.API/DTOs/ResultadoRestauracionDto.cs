namespace SkyPlus.API.DTOs
{
    public class ResultadoRestauracionDto
    {
        public Guid IdBackup { get; set; }
        public string BaseDestino { get; set; } = "";
        public int IdAdministradorSolicitante { get; set; }
        public int IdAdministradorAutorizante { get; set; }
        public DateTime FechaUtc { get; set; }
        public string Estado { get; set; } = "";
    }
}
