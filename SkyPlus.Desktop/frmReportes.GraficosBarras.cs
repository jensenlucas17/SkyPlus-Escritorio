
using SkyPlus.Desktop.Models;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmReportes
    {
        private readonly Panel _pnlGraficoBarras = new()
        {
            Height = 300,
            Dock = DockStyle.Top,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 12)
        };

        private readonly Label _lblGraficoBarras = new()
        {
            Text = "Ranking",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 12F),
            Margin = new Padding(0, 12, 0, 8)
        };

        private readonly List<(string Etiqueta, decimal Valor)>
            _datosBarras = new();

        private bool _barrasInicializadas;

        private void InicializarGraficosBarras()
        {
            if (_barrasInicializadas)
                return;

            _barrasInicializadas = true;

            _tabla.SuspendLayout();

            int filaInsercion = 5;

            foreach (Control control in _tabla.Controls)
            {
                int fila = _tabla.GetRow(control);

                if (fila >= filaInsercion)
                    _tabla.SetRow(control, fila + 2);
            }

            _tabla.RowCount += 2;

            _tabla.RowStyles.Insert(
                filaInsercion,
                new RowStyle(SizeType.AutoSize));

            _tabla.RowStyles.Insert(
                filaInsercion + 1,
                new RowStyle(SizeType.AutoSize));

            _tabla.Controls.Add(
                _lblGraficoBarras, 0, filaInsercion);

            _tabla.Controls.Add(
                _pnlGraficoBarras, 0, filaInsercion + 1);

            _pnlGraficoBarras.Paint += DibujarBarras;

            _lblGraficoBarras.Visible = false;
            _pnlGraficoBarras.Visible = false;

            _tabla.ResumeLayout(true);
        }

        private void MostrarBarras(
            string titulo,
            IEnumerable<(string Etiqueta, decimal Valor)> datos)
        {
            _datosBarras.Clear();
            _datosBarras.AddRange(datos.Take(10));

            _lblGraficoBarras.Text = titulo;

            bool visible = _datosBarras.Count > 0;

            _lblGraficoBarras.Visible = visible;
            _pnlGraficoBarras.Visible = visible;

            _pnlGraficoBarras.Invalidate();
        }

        private void OcultarBarras()
        {
            _datosBarras.Clear();
            _lblGraficoBarras.Visible = false;
            _pnlGraficoBarras.Visible = false;
        }

        private void DibujarBarras(
            object? sender,
            PaintEventArgs e)
        {
            if (_datosBarras.Count == 0)
                return;

            var g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            int ancho = _pnlGraficoBarras.ClientSize.Width;

            if (ancho < 250)
                return;

            int izquierda = 170;
            int derecha = 75;
            int arriba = 18;
            int altoFila = 26;

            decimal maximo = Math.Max(
                1m, _datosBarras.Max(x => x.Valor));

            int anchoDisponible =
                ancho - izquierda - derecha;

            using var pincelBarras = new SolidBrush(
                Color.FromArgb(37, 99, 235));

            using var pincelTexto = new SolidBrush(
                Color.FromArgb(60, 75, 95));

            using var fuente = new Font("Segoe UI", 9F);

            for (int i = 0; i < _datosBarras.Count; i++)
            {
                var dato = _datosBarras[i];

                int y = arriba + i * altoFila;

                float largo = (float)(
                    dato.Valor / maximo * anchoDisponible);

                g.DrawString(
                    dato.Etiqueta,
                    fuente,
                    pincelTexto,
                    8,
                    y + 2);

                g.FillRectangle(
                    pincelBarras,
                    izquierda,
                    y,
                    Math.Max(0, largo),
                    18);

                g.DrawString(
                    dato.Valor.ToString("0.##"),
                    fuente,
                    pincelTexto,
                    izquierda + largo + 6,
                    y + 2);
            }
        }

        private void GraficarVuelos(ReporteVuelosResponse r)
        {
            MostrarBarras(
                "Ranking de vuelos por ocupación (%)",
                r.DetalleVuelos
                    .OrderByDescending(
                        x => x.PorcentajeOcupacion)
                    .Take(10)
                    .Select(x => (
                        x.NumeroVuelo,
                        (decimal)x.PorcentajeOcupacion)));
        }

        private void GraficarPasajeros(ReportePasajerosResponse r)
        {
            MostrarBarras(
                "Pasajeros frecuentes por reservas",
                r.PasajerosFrecuentes
                    .OrderByDescending(x => x.TotalReservas)
                    .Take(10)
                    .Select(x => (
                        x.NombreCompleto,
                        (decimal)x.TotalReservas)));
        }

        private void GraficarEquipajes(ReporteEquipajesResponse r)
        {
            MostrarBarras(
                "Equipajes por tipo",
                r.EquipajesPorTipo
                    .OrderByDescending(x => x.Cantidad)
                    .Select(x => (
                        x.TipoEquipaje,
                        (decimal)x.Cantidad)));
        }

        private void GraficarCheckIns(ReporteCheckInsResponse r)
        {
            MostrarBarras(
                "Embarcados por vuelo",
                r.CheckInsPorVuelo
                    .OrderByDescending(x => x.Embarcados)
                    .Take(10)
                    .Select(x => (
                        x.NumeroVuelo,
                        (decimal)x.Embarcados)));
        }
    }
}
