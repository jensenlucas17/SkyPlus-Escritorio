using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyPlus.API.DTOs.Reportes
{
    public class IndicadorComparacionResponse
    {
        public string Indicador { get; set; } = string.Empty;
        public decimal Actual { get; set; }
        public decimal Anterior { get; set; }
        public decimal? VariacionPorcentual { get; set; }
        public decimal? DiferenciaPuntosPorcentuales { get; set; }
        public bool EsPorcentaje { get; set; }

        public static IndicadorComparacionResponse Crear(
            string nombre,
            decimal actual,
            decimal anterior,
            bool esPorcentaje = false)
        {
            return new IndicadorComparacionResponse
            {
                Indicador = nombre,
                Actual = actual,
                Anterior = anterior,
                EsPorcentaje = esPorcentaje,

                VariacionPorcentual = anterior == 0
                    ? (actual == 0 ? 0m : null)
                    : Math.Round(
                        (actual - anterior) * 100m / anterior, 2),

                DiferenciaPuntosPorcentuales = esPorcentaje
                    ? Math.Round(actual - anterior, 2)
                    : null
            };
        }
    }

    public class ComparacionPeriodosVuelosResponse
    {
        public DateTime DesdeActual { get; set; }
        public DateTime HastaActual { get; set; }
        public DateTime DesdeAnterior { get; set; }
        public DateTime HastaAnterior { get; set; }

        public List<IndicadorComparacionResponse> Indicadores { get; set; }
            = new();

        public static ComparacionPeriodosVuelosResponse Crear(
            ReporteVuelosResponse actual,
            ReporteVuelosResponse anterior)
        {
            return new ComparacionPeriodosVuelosResponse
            {
                DesdeActual = actual.Desde,
                HastaActual = actual.Hasta,
                DesdeAnterior = anterior.Desde,
                HastaAnterior = anterior.Hasta,

                Indicadores = new()
                {
                    IndicadorComparacionResponse.Crear(
                        "Vuelos",
                        actual.TotalVuelos,
                        anterior.TotalVuelos),

                    IndicadorComparacionResponse.Crear(
                        "Reservas confirmadas",
                        actual.DetalleVuelos.Sum(
                            v => v.ReservasConfirmadas),
                        anterior.DetalleVuelos.Sum(
                            v => v.ReservasConfirmadas)),

                    IndicadorComparacionResponse.Crear(
                        "Asientos ocupados",
                        actual.AsientosOcupados,
                        anterior.AsientosOcupados),

                    IndicadorComparacionResponse.Crear(
                        "Ocupación",
                        actual.PorcentajeOcupacionGeneral,
                        anterior.PorcentajeOcupacionGeneral,
                        true)
                }
            };
        }
    }
}