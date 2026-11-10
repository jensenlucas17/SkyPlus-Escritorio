namespace SkyPlus.Desktop.Models
{
    // Respuesta del endpoint POST /api/backups/autorizar-restauracion-produccion.
    public sealed class AutorizacionProduccionResponse
    {
        public string? Mensaje { get; set; }
        public Guid OperationId { get; set; }
        public Guid BackupId { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        // Cadena JSON firmada, compuesta por Payload y Signature.
        // Es un dato sensible y de corta vigencia: no registrar en logs.
        public string? AutorizacionFirmada { get; set; }
    }
}
