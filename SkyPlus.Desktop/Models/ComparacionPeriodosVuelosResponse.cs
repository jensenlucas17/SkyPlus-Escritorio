using System;
using System.Collections.Generic;

namespace SkyPlus.Desktop.Models
{
    public class IndicadorComparacionResponse
    {
        public string Indicador { get; set; } = string.Empty;
        public decimal Actual { get; set; }
        public decimal Anterior { get; set; }
        public decimal? VariacionPorcentual { get; set; }
        public decimal? DiferenciaPuntosPorcentuales { get; set; }
        public bool EsPorcentaje { get; set; }
    }

    public class ComparacionPeriodosVuelosResponse
    {
        public DateTime DesdeActual { get; set; }
        public DateTime HastaActual { get; set; }
        public DateTime DesdeAnterior { get; set; }
        public DateTime HastaAnterior { get; set; }

        public List<IndicadorComparacionResponse> Indicadores { get; set; }
            = new();
    }
}