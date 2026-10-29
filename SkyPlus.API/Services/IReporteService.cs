using SkyPlus.API.DTOs.Reportes;

namespace SkyPlus.API.Services
{
    public interface IReporteService
    {
        Task<ReporteVentasResponse> ObtenerVentasAsync(
            DateTime desde,
            DateTime hasta);

        Task<ReporteReservasResponse> ObtenerReservasAsync(
            DateTime desde,
            DateTime hasta);

        Task<ReporteVuelosResponse> ObtenerVuelosAsync(
            DateTime desde,
            DateTime hasta);

        Task<ReportePasajerosResponse> ObtenerPasajerosAsync(
            DateTime desde,
            DateTime hasta);

        Task<ReporteEquipajesResponse> ObtenerEquipajesAsync(
            DateTime desde,
            DateTime hasta);

        Task<ReporteCheckInsResponse> ObtenerCheckInsAsync(
            DateTime desde,
            DateTime hasta);
    }
}
