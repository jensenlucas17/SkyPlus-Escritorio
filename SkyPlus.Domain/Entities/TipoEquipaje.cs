namespace SkyPlus.Domain.Entities
{
    public class TipoEquipaje
    {
        public int IdTipoEquipaje { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal CostoAdicional { get; set; }

        public decimal LimiteKg { get; set; }

        public string Descripcion { get; set; } = string.Empty;
    }
}