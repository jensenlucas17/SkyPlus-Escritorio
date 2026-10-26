
namespace SkyPlus.API.DTOs
{
    public class EstadoCheckInResponse
    {
        public int IdReserva { get; set; }
        public bool PuedeRealizarCheckIn { get; set; }
        public int EquipajesSinPesar { get; set; }
        public int CargosPendientes { get; set; }
        public decimal MontoPendiente { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
