
using SkyPlus.API.DTOs.Reportes;

namespace SkyPlus.API.Services
{
    public interface IReporteService
    {
        // Métodos originales: conservan compatibilidad
        Task<ReporteVentasResponse> ObtenerVentasAsync(
            DateTime desde, DateTime hasta);

        Task<ReporteReservasResponse> ObtenerReservasAsync(
            DateTime desde, DateTime hasta);

        Task<ReporteVuelosResponse> ObtenerVuelosAsync(
            DateTime desde, DateTime hasta);

        Task<ReportePasajerosResponse> ObtenerPasajerosAsync(
            DateTime desde, DateTime hasta);

        Task<ReporteEquipajesResponse> ObtenerEquipajesAsync(
            DateTime desde, DateTime hasta);

        Task<ReporteCheckInsResponse> ObtenerCheckInsAsync(
            DateTime desde, DateTime hasta);

        // Nuevos métodos: admiten filtros avanzados
        Task<ReporteVentasResponse> ObtenerVentasAsync(
            ReporteFiltrosRequest filtros);

        Task<ReporteReservasResponse> ObtenerReservasAsync(
            ReporteFiltrosRequest filtros);

        Task<ReporteVuelosResponse> ObtenerVuelosAsync(
            ReporteFiltrosRequest filtros);

        Task<ReportePasajerosResponse> ObtenerPasajerosAsync(
            ReporteFiltrosRequest filtros);

        Task<ReporteEquipajesResponse> ObtenerEquipajesAsync(
            ReporteFiltrosRequest filtros);

        Task<ReporteCheckInsResponse> ObtenerCheckInsAsync(
            ReporteFiltrosRequest filtros);
    }
}
