
namespace SkyPlus.API.DTOs
{
    public class TipoEquipajeResponse
    {
        public int IdTipoEquipaje { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal CostoAdicional { get; set; }
        public decimal LimiteKg { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
