namespace SkyPlus.Domain.Entities
{
    public class Reserva
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

        // Relaciones
        public Pasajero? Pasajero { get; set; }

        public Vuelo? Vuelo { get; set; }

        public Asiento? Asiento { get; set; }

        public Usuario? UsuarioAgente { get; set; }
    }
}