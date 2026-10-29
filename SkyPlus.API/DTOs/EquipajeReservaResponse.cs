
namespace SkyPlus.API.DTOs
{
    public class EquipajeReservaResponse
    {
        public int IdEquipajeReserva { get; set; }
        public int IdReserva { get; set; }
        public int IdTipoEquipaje { get; set; }
        public string NombreTipoEquipaje { get; set; } = string.Empty;
        public decimal? PesoKg { get; set; }
        public decimal LimiteKgContratado { get; set; }
        public decimal Costo { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaContratacion { get; set; }
    }
}
