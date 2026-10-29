
namespace SkyPlus.API.DTOs
{
    public class ReembolsoResponse
    {
        public int IdReembolso { get; set; }
        public int IdPago { get; set; }
        public int IdReserva { get; set; }
        public string CodigoReserva { get; set; } = "";
        public string Pasajero { get; set; } = "";
        public string Vuelo { get; set; } = "";
        public int IdUsuarioAgente { get; set; }
        public decimal MontoReembolsado { get; set; }
        public string Motivo { get; set; } = "";
        public DateTime FechaReembolso { get; set; }
        public string EstadoReembolso { get; set; } = "";
    }
}
