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


        private static string ConstruirQuery(ReporteFiltrosRequest f)
        {
            var parametros = new List<string>
            {
                $"desde={f.Desde.Date:yyyy-MM-ddTHH:mm:ss}",
                $"hasta={f.Hasta.Date.AddDays(1).AddSeconds(-1):yyyy-MM-ddTHH:mm:ss}",
                $"agrupacion={Uri.EscapeDataString(f.Agrupacion)}"
            };

            void Agregar(string nombre, object? valor)
            {
                if (valor == null)
                    return;

                string texto = valor.ToString() ?? "";

                if (!string.IsNullOrWhiteSpace(texto))
                    parametros.Add(
                        $"{nombre}={Uri.EscapeDataString(texto)}");
            }

            Agregar("idVuelo", f.IdVuelo);
            Agregar("idLugarOrigen", f.IdLugarOrigen);
            Agregar("idLugarDestino", f.IdLugarDestino);
            Agregar("estadoVuelo", f.EstadoVuelo);
            Agregar("estadoReserva", f.EstadoReserva);
            Agregar("metodoPago", f.MetodoPago);
            Agregar("tipoPago", f.TipoPago);

            if (f.CompararPeriodoAnterior)
                parametros.Add("compararPeriodoAnterior=true");

            return string.Join("&", parametros);
        }

        public Task<ReporteVentasResponse?> ObtenerVentasAsync(
            ReporteFiltrosRequest filtros)
        {
            return _apiClient.GetAsync<ReporteVentasResponse>(
                $"api/Reportes/ventas?{ConstruirQuery(filtros)}");
        }

        public Task<ReporteReservasResponse?> ObtenerReservasAsync(
            ReporteFiltrosRequest filtros)
        {
            return _apiClient.GetAsync<ReporteReservasResponse>(
                $"api/Reportes/reservas?{ConstruirQuery(filtros)}");
        }

        public Task<ReporteVuelosResponse?> ObtenerVuelosAsync(
            ReporteFiltrosRequest filtros)
        {
            return _apiClient.GetAsync<ReporteVuelosResponse>(
                $"api/Reportes/vuelos?{ConstruirQuery(filtros)}");
        }

        public Task<ReportePasajerosResponse?> ObtenerPasajerosAsync(
            ReporteFiltrosRequest filtros)
        {
            return _apiClient.GetAsync<ReportePasajerosResponse>(
                $"api/Reportes/pasajeros?{ConstruirQuery(filtros)}");
        }

        public Task<ReporteEquipajesResponse?> ObtenerEquipajesAsync(
            ReporteFiltrosRequest filtros)
        {
            return _apiClient.GetAsync<ReporteEquipajesResponse>(
                $"api/Reportes/equipajes?{ConstruirQuery(filtros)}");
        }

        public Task<ReporteCheckInsResponse?> ObtenerCheckInsAsync(
            ReporteFiltrosRequest filtros)
        {
            return _apiClient.GetAsync<ReporteCheckInsResponse>(
                $"api/Reportes/checkins?{ConstruirQuery(filtros)}");
        }
    }
}