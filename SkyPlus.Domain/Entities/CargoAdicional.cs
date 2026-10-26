
namespace SkyPlus.Domain.Entities
{
    public class CargoAdicional
    {
        public int IdCargoAdicional { get; set; }

        public int IdReserva { get; set; }

        public int? IdEquipajeReserva { get; set; }

        // Equipaje o ExcesoPeso.
        public string TipoCargo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        // Pendiente, Pagado o Anulado.
        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relaciones
        public Reserva? Reserva { get; set; }

        public EquipajeReserva? EquipajeReserva { get; set; }
    }
}
