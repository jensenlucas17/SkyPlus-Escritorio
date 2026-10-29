namespace SkyPlus.API.DTOs.Reportes
{
    public class EquipajePorTipoResponse
    {
        public string TipoEquipaje { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PesoTotalKg { get; set; }
        public decimal ImporteContratado { get; set; }
    }

    public class ReporteEquipajesResponse
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public int TotalEquipajes { get; set; }
        public int EquipajesActivos { get; set; }
        public int EquipajesPendientesPago { get; set; }
        public int EquipajesCancelados { get; set; }

        public int EquipajesPesados { get; set; }
        public int EquipajesConExceso { get; set; }

        public decimal PesoTotalKg { get; set; }
        public decimal ExcesoTotalKg { get; set; }

        public decimal ImporteContratado { get; set; }

        public int CargosGenerados { get; set; }
        public int CargosPagados { get; set; }
        public int CargosPendientes { get; set; }

        public decimal TotalCargosGenerados { get; set; }
        public decimal TotalCargosPagados { get; set; }
        public decimal TotalCargosPendientes { get; set; }

        public List<EquipajePorTipoResponse> EquipajesPorTipo { get; set; }
            = new();
    }
}