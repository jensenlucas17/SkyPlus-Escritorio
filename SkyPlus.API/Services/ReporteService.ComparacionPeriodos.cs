using System;
using System.Threading.Tasks;
using SkyPlus.API.DTOs.Reportes;
using SkyPlus.API.Exceptions;

namespace SkyPlus.API.Services
{
    public partial class ReporteService
    {
        public async Task<ReporteVuelosResponse> ObtenerVuelosAsync(
            ReporteFiltrosRequest f)
        {
            if (f == null)
                throw new ValidacionNegocioException(
                    "Los filtros son obligatorios.");

            var filtrosActuales = CopiarFiltrosVuelos(
                f,
                f.Desde.Date,
                f.Hasta.Date);

            ValidarFiltrosOperativos(filtrosActuales);

            if (!string.IsNullOrWhiteSpace(f.EstadoReserva))
                throw new ValidacionNegocioException(
                    "El filtro de estado de reserva no corresponde " +
                    "al reporte de vuelos.");

            if (!f.CompararPeriodoAnterior)
                return await ObtenerVuelosPeriodoAsync(
                    filtrosActuales);

            DateTime desde = filtrosActuales.Desde;
            DateTime hasta = filtrosActuales.Hasta;

            int dias = (hasta - desde).Days + 1;

            if ((desde - DateTime.MinValue.Date).Days < dias)
                throw new ValidacionNegocioException(
                    "No se puede calcular el período anterior " +
                    "para las fechas indicadas.");

            var filtrosAnteriores = CopiarFiltrosVuelos(
                f,
                desde.AddDays(-dias),
                desde.AddDays(-1));

            // Ejecutar secuencialmente porque comparten DbContext.
            var actual = await ObtenerVuelosPeriodoAsync(
                filtrosActuales);

            var anterior = await ObtenerVuelosPeriodoAsync(
                filtrosAnteriores);

            actual.Comparacion =
                ComparacionPeriodosVuelosResponse.Crear(
                    actual,
                    anterior);

            return actual;
        }

        private static ReporteFiltrosRequest CopiarFiltrosVuelos(
            ReporteFiltrosRequest f,
            DateTime desde,
            DateTime hasta)
        {
            return new ReporteFiltrosRequest
            {
                Desde = desde,
                Hasta = hasta,

                IdVuelo = f.IdVuelo,
                IdLugarOrigen = f.IdLugarOrigen,
                IdLugarDestino = f.IdLugarDestino,

                EstadoVuelo = f.EstadoVuelo,
                EstadoReserva = f.EstadoReserva,

                MetodoPago = f.MetodoPago,
                TipoPago = f.TipoPago,
                Agrupacion = f.Agrupacion,

                // Cada consulta genera solamente su propio período.
                CompararPeriodoAnterior = false
            };
        }
    }
}