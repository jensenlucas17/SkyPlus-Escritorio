using System;
using System.Linq;
using System.Windows.Forms;
using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop
{
    public partial class frmReportes
    {
        private readonly CheckBox _chkCompararPeriodoAnterior = new()
        {
            Text = "Comparar período anterior",
            AutoSize = true
        };

        private readonly Label _lblComparacionPeriodos =
            CrearTitulo(string.Empty);

        private readonly DataGridView _dgvComparacionPeriodos =
            CrearGrid();

        private record FilaComparacionPeriodo(
            string Indicador,
            string Anterior,
            string Actual,
            string Variacion,
            string DiferenciaPuntos);

        private void InicializarComparacionPeriodos()
        {
            _panelFiltrosAvanzados.Controls.Add(
                CrearCampo(
                    "Comparación de períodos",
                    _chkCompararPeriodoAnterior,
                    260));

            cmbTipoReporte.SelectedIndexChanged +=
                (_, _) => ActualizarComparacionDisponible();

            ActualizarComparacionDisponible();

            int fila = _tabla.RowCount;

            _tabla.RowCount += 2;

            _tabla.RowStyles.Add(
                new RowStyle(SizeType.AutoSize));

            _tabla.RowStyles.Add(
                new RowStyle(SizeType.AutoSize));

            _tabla.Controls.Add(
                _lblComparacionPeriodos, 0, fila);

            _tabla.Controls.Add(
                _dgvComparacionPeriodos, 0, fila + 1);

            AppEstilos.EstilizarGrid(
                _dgvComparacionPeriodos);

            OcultarComparacionPeriodos();
        }

        private void ActualizarComparacionDisponible()
        {
            bool vuelos = cmbTipoReporte.Text == "Vuelos";

            _chkCompararPeriodoAnterior.Parent!.Visible = vuelos;

            if (!vuelos)
                _chkCompararPeriodoAnterior.Checked = false;
        }

        private void OcultarComparacionPeriodos()
        {
            _lblComparacionPeriodos.Visible = false;
            _dgvComparacionPeriodos.Visible = false;
            _dgvComparacionPeriodos.DataSource = null;
        }

        private static string FormatearVariacion(decimal? valor)
        {
            return valor.HasValue
                ? valor.Value.ToString("+0.##;-0.##;0") + " %"
                : "Sin base de comparación";
        }

        private void MostrarComparacionPeriodos(
            ComparacionPeriodosVuelosResponse? comparacion)
        {
            if (comparacion == null)
            {
                OcultarComparacionPeriodos();
                return;
            }

            _lblComparacionPeriodos.Text =
                $"Comparación: " +
                $"{comparacion.DesdeActual:dd/MM/yyyy}–" +
                $"{comparacion.HastaActual:dd/MM/yyyy}" +
                $" vs. " +
                $"{comparacion.DesdeAnterior:dd/MM/yyyy}–" +
                $"{comparacion.HastaAnterior:dd/MM/yyyy}";

            var filas = comparacion.Indicadores
                .Select(i => new FilaComparacionPeriodo(
                    i.Indicador,

                    i.Anterior.ToString("0.##") +
                        (i.EsPorcentaje ? " %" : ""),

                    i.Actual.ToString("0.##") +
                        (i.EsPorcentaje ? " %" : ""),

                    FormatearVariacion(
                        i.VariacionPorcentual),

                    i.DiferenciaPuntosPorcentuales.HasValue
                        ? i.DiferenciaPuntosPorcentuales.Value
                            .ToString("+0.##;-0.##;0") + " p.p."
                        : "—"))
                .ToList();

            Cargar(
                _dgvComparacionPeriodos,
                filas,
                new[]
                {
                    new Col(
                        "Indicador",
                        nameof(FilaComparacionPeriodo.Indicador)),

                    new Col(
                        "Anterior",
                        nameof(FilaComparacionPeriodo.Anterior)),

                    new Col(
                        "Actual",
                        nameof(FilaComparacionPeriodo.Actual)),

                    new Col(
                        "Variación (%)",
                        nameof(FilaComparacionPeriodo.Variacion)),

                    new Col(
                        "Cambio (p.p.)",
                        nameof(FilaComparacionPeriodo.DiferenciaPuntos))
                });

            _lblComparacionPeriodos.Visible = true;
            _dgvComparacionPeriodos.Visible = true;

            _indicadoresExportacion.Add(
                new Indicador(
                    "Período anterior",
                    $"{comparacion.DesdeAnterior:dd/MM/yyyy}–" +
                    $"{comparacion.HastaAnterior:dd/MM/yyyy}"));

            foreach (var fila in filas)
            {
                _indicadoresExportacion.Add(
                    new Indicador(
                        $"Comparación: {fila.Indicador}",
                        $"Anterior: {fila.Anterior}; " +
                        $"Actual: {fila.Actual}; " +
                        $"Variación: {fila.Variacion}; " +
                        $"Cambio: {fila.DiferenciaPuntos}"));
            }
        }
    }
}