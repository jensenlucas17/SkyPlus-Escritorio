
namespace SkyPlus.API.DTOs
{
    public class CheckInResponse
    {
        public int IdCheckin { get; set; }
        public int IdReserva { get; set; }
        public string CodigoReserva { get; set; } = string.Empty;
        public int IdUsuarioAgente { get; set; }
        public string CodigoQr { get; set; } = string.Empty;
        public string BoardingPass { get; set; } = string.Empty;
        public DateTime FechaCheckin { get; set; }
        public string EstadoCheckin { get; set; } = string.Empty;
    }
}
