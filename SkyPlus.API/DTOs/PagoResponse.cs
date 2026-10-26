using System;

namespace SkyPlus.API.DTOs
{
    public class PagoResponse
    {
        public int IdPago { get; set; }
        public int IdReserva { get; set; }
        public string CodigoReserva { get; set; } = string.Empty;
        public int IdUsuarioAgente { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public DateTime FechaPago { get; set; }
        public string EstadoPago { get; set; } = string.Empty;
    }
}
