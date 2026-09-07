namespace SkyPlus.Domain.Entities
{
    public class CheckIn
    {
        public int IdCheckin { get; set; }

        public int IdReserva { get; set; }

        public int IdUsuarioAgente { get; set; }

        public string CodigoQr { get; set; } = string.Empty;

        public string BoardingPass { get; set; } = string.Empty;

        public DateTime FechaCheckin { get; set; }

        // Relaciones
        public Reserva? Reserva { get; set; }

        public Usuario? UsuarioAgente { get; set; }
    }
}