using System.Globalization;
using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class ReporteClient
    {
        private readonly ApiClient _apiClient;

        public ReporteClient()
        {
            _apiClient = new ApiClient();
        }

        // Desde = comienzo del primer día; Hasta = final del último día
        private static string Rango(DateTime desde, DateTime hasta)
        {
            const string formato = "yyyy-MM-dd'T'HH:mm:ss";
            var d = desde.Date.ToString(formato, CultureInfo.InvariantCulture);
            var h = hasta.Date.AddDays(1).AddSeconds(-1).ToString(formato, CultureInfo.InvariantCulture);
            return $"?desde={d}&hasta={h}";
        }

        public Task<ReporteVentasResponse?> ObtenerVentasAsync(DateTime desde, DateTime hasta) =>
            _apiClient.GetAsync<ReporteVentasResponse>("api/Reportes/ventas" + Rango(desde, hasta));

        public Task<ReporteReservasResponse?> ObtenerReservasAsync(DateTime desde, DateTime hasta) =>
            _apiClient.GetAsync<ReporteReservasResponse>("api/Reportes/reservas" + Rango(desde, hasta));

        public Task<ReporteVuelosResponse?> ObtenerVuelosAsync(DateTime desde, DateTime hasta) =>
            _apiClient.GetAsync<ReporteVuelosResponse>("api/Reportes/vuelos" + Rango(desde, hasta));

        public Task<ReportePasajerosResponse?> ObtenerPasajerosAsync(DateTime desde, DateTime hasta) =>
            _apiClient.GetAsync<ReportePasajerosResponse>("api/Reportes/pasajeros" + Rango(desde, hasta));

        public Task<ReporteEquipajesResponse?> ObtenerEquipajesAsync(DateTime desde, DateTime hasta) =>
            _apiClient.GetAsync<ReporteEquipajesResponse>("api/Reportes/equipajes" + Rango(desde, hasta));

        public Task<ReporteCheckInsResponse?> ObtenerCheckInsAsync(DateTime desde, DateTime hasta) =>
            _apiClient.GetAsync<ReporteCheckInsResponse>("api/Reportes/checkins" + Rango(desde, hasta));
    }
}