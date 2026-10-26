
using System.Collections.Generic;

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

        public string EstadoPago { get; set; } = "Registrado";

        // Inicial o Adicional.
        public string TipoPago { get; set; } = "Inicial";

        // Relaciones
        public Reserva? Reserva { get; set; }

        public Usuario? UsuarioAgente { get; set; }

        public ICollection<PagoDetalle> Detalles { get; set; }
            = new List<PagoDetalle>();
    }
}
