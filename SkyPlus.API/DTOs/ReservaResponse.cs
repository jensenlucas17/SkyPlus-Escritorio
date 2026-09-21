namespace SkyPlus.API.DTOs
{
    public class ReservaResponse
    {
        public int IdReserva { get; set; }

        public int IdPasajero { get; set; }

        public int IdVuelo { get; set; }

        public int IdAsiento { get; set; }

        public int IdUsuarioAgente { get; set; }

        public string CodigoReserva { get; set; } = string.Empty;

        public decimal TarifaBase { get; set; }

        public string EstadoReserva { get; set; } = string.Empty;

        public DateTime FechaReserva { get; set; }

        public DateTime? FechaCancelacion { get; set; }

        // Datos para mostrar en Desktop
        public string Pasajero { get; set; } = string.Empty;

        public string Vuelo { get; set; } = string.Empty;

        public string Asiento { get; set; } = string.Empty;
    }
}