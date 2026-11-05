namespace SkyPlus.Desktop.Models
{
    public class BackupResponse
    {
        public Guid Id { get; set; }
        public string Archivo { get; set; } = string.Empty;
        public DateTime FechaUtc { get; set; }
        public int IdAdministrador { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }
        public string Sha256 { get; set; } = string.Empty;
        public DateTime? FechaEliminacionUtc { get; set; }
        public int? IdAdministradorEliminacion { get; set; }
    }
}
