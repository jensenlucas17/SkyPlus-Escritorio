namespace SkyPlus.Desktop.Models
{
    // Mismos campos que los DTO de Equipajes de la API
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

        public string Descripcion =>
            $"{NombreTipoEquipaje} (hasta {LimiteKgContratado:0.##} kg)" +
            (PesoKg.HasValue ? $" · {PesoKg:0.##} kg" : " · sin pesar");
    }

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

    public class TipoEquipajeResponse
    {
        public int IdTipoEquipaje { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal CostoAdicional { get; set; }
        public decimal LimiteKg { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
