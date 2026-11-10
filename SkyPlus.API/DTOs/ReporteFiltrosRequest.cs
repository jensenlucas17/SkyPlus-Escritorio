
namespace SkyPlus.API.DTOs.Reportes
{
    public class ReporteFiltrosRequest
    {
        // Período del reporte
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        // Filtros de vuelos y rutas
        public int? IdVuelo { get; set; }
        public int? IdLugarOrigen { get; set; }
        public int? IdLugarDestino { get; set; }

        // Filtros de estado
        public string? EstadoVuelo { get; set; }
        public string? EstadoReserva { get; set; }

        // Filtros financieros
        public string? MetodoPago { get; set; }
        public string? TipoPago { get; set; }

        // Agrupación temporal: Dia, Semana o Mes
        public string Agrupacion { get; set; } = "Dia";

        // Comparar contra el período anterior
        public bool CompararPeriodoAnterior { get; set; } = false;
    }
}
