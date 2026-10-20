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

        public string EstadoCheckin { get; set; } = "Realizado";

        // Relaciones
        public Reserva? Reserva { get; set; }

        public Usuario? UsuarioAgente { get; set; }
    }
}