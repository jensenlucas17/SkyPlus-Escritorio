
namespace SkyPlus.Domain.Entities
{
    public class EquipajeReserva
    {
        public int IdEquipajeReserva { get; set; }

        public int IdReserva { get; set; }

        public int IdTipoEquipaje { get; set; }

        // Se registra cuando se verifica el equipaje.
        public decimal? PesoKg { get; set; }

        // Límite vigente al momento de contratar.
        public decimal LimiteKgContratado { get; set; }

        // Precio vigente al momento de contratar.
        public decimal Costo { get; set; }

        // PendientePago, Activo o Cancelado.
        public string Estado { get; set; } = "PendientePago";

        public DateTime FechaContratacion { get; set; } = DateTime.UtcNow;

        // Relaciones
        public Reserva? Reserva { get; set; }

        public TipoEquipaje? TipoEquipaje { get; set; }
    }
}
