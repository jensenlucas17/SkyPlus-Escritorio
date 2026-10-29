
namespace SkyPlus.Domain.Entities
{
    public class PagoDetalle
    {
        public int IdPagoDetalle { get; set; }

        public int IdPago { get; set; }

        // Null cuando el detalle corresponde al pasaje.
        public int? IdCargoAdicional { get; set; }

        // Pasaje o CargoAdicional.
        public string Concepto { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        // Relaciones
        public Pago? Pago { get; set; }

        public CargoAdicional? CargoAdicional { get; set; }
    }
}
