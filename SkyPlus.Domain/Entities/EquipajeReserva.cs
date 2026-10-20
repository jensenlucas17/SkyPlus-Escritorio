namespace SkyPlus.Domain.Entities
{
    public class EquipajeReserva
    {
        public int IdEquipajeReserva { get; set; }

        public int IdReserva { get; set; }

        public int IdTipoEquipaje { get; set; }

        public decimal PesoKg { get; set; }

        public decimal Costo { get; set; }

        public string Estado { get; set; } = "Activo";

        // Relaciones
        public Reserva? Reserva { get; set; }

        public TipoEquipaje? TipoEquipaje { get; set; }
    }
}