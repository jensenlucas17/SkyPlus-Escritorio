
using System;

namespace SkyPlus.Desktop.Models
{
    public class ReembolsoResponse
    {
        public int IdReembolso { get; set; }
        public int IdPago { get; set; }
        public int IdReserva { get; set; }

        public string CodigoReserva { get; set; } = string.Empty;
        public string Pasajero { get; set; } = string.Empty;
        public string Vuelo { get; set; } = string.Empty;

        public int IdUsuarioAgente { get; set; }
        public decimal MontoReembolsado { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public DateTime FechaReembolso { get; set; }
        public string EstadoReembolso { get; set; } = string.Empty;
    }
}
