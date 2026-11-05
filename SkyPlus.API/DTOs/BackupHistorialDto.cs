namespace SkyPlus.API.DTOs
{
    public class BackupHistorialDto
    {
        public Guid Id { get; set; }
        public string Archivo { get; set; } = string.Empty;
        public DateTime FechaUtc { get; set; }
        public int IdAdministrador { get; set; }
        public string Tipo { get; set; } = "Manual";
        public string Estado { get; set; } = "Disponible";
        public long TamanoBytes { get; set; }
        public string Sha256 { get; set; } = string.Empty;

        // Datos de eliminación
        public DateTime? FechaEliminacionUtc { get; set; }
        public int? IdAdministradorEliminacion { get; set; }
    }
}
