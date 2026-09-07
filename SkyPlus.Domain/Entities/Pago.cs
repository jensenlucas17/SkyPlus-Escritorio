namespace SkyPlus.Domain.Entities
{
    public class Pago
    {
        public int IdPago { get; set; }

        public int IdReserva { get; set; }

        public int IdUsuarioAgente { get; set; }

        public decimal Monto { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        public DateTime FechaPago { get; set; }

        // Relaciones
        public Reserva? Reserva { get; set; }

        public Usuario? UsuarioAgente { get; set; }
    }
}