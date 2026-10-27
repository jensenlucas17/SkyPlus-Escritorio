namespace SkyPlus.API.DTOs.Reportes
{
    public class VueloDetalleReporteResponse
    {
        public int IdVuelo { get; set; }
        public string NumeroVuelo { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;

        public DateTime Salida { get; set; }
        public DateTime Llegada { get; set; }
        public string EstadoVuelo { get; set; } = string.Empty;

        public int TotalAsientos { get; set; }
        public int AsientosOcupados { get; set; }
        public int AsientosDisponibles { get; set; }
        public int ReservasConfirmadas { get; set; }
        public decimal PorcentajeOcupacion { get; set; }
    }

    public class VueloPorEstadoResponse
    {
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class ReporteVuelosResponse
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public int TotalVuelos { get; set; }
        public int Programados { get; set; }
        public int Confirmados { get; set; }
        public int Cancelados { get; set; }
        public int Finalizados { get; set; }

        public int TotalAsientos { get; set; }
        public int AsientosOcupados { get; set; }
        public int AsientosDisponibles { get; set; }

        public decimal PorcentajeOcupacionGeneral { get; set; }

        public List<VueloPorEstadoResponse> VuelosPorEstado { get; set; }
            = new();

        public List<VueloDetalleReporteResponse> DetalleVuelos { get; set; }
            = new();
    }
}