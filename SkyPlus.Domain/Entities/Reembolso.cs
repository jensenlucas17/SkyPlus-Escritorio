namespace SkyPlus.Domain.Entities
{
    public class Reembolso
    {
        public int IdReembolso { get; set; }

        public int IdPago { get; set; }

        public int IdUsuarioAgente { get; set; }

        public decimal MontoReembolsado { get; set; }

        public string Motivo { get; set; } = string.Empty;

        public DateTime FechaReembolso { get; set; }

        // Relaciones
        public Pago? Pago { get; set; }

        public Usuario? UsuarioAgente { get; set; }
    }
}