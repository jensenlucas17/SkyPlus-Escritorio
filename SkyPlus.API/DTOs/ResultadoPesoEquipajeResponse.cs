
namespace SkyPlus.API.DTOs
{
    public class ResultadoPesoEquipajeResponse
    {
        public int IdEquipajeReserva { get; set; }

        public decimal PesoKg { get; set; }

        public decimal LimiteKgContratado { get; set; }

        public decimal ExcesoKg { get; set; }

        public decimal MontoExceso { get; set; }

        public bool RequierePago { get; set; }

        public string Mensaje { get; set; } = string.Empty;
    }
}
