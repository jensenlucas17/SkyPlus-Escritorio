using SkyPlus.Desktop.Models;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmReportes
    {
        private readonly Label _lblGrafico = new()
        {
            Text = "Evolución",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 12F),
            Margin = new Padding(0, 12, 0, 8)
        };

        private readonly Panel _pnlGrafico = new()
        {
            Height = 270,
            Dock = DockStyle.Top,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 12)
        };

        private readonly List<(string Etiqueta, decimal Valor)>
            _puntosGrafico = new();

        private bool _graficoInicializado;

        private void InicializarGraficos()
        {
            if (_graficoInicializado)
                return;

            _graficoInicializado = true;

            _tabla.SuspendLayout();

            // Insertar gráfico debajo del resumen.
            foreach (Control control in _tabla.Controls)
            {
                int fila = _tabla.GetRow(control);

                if (fila >= 3)
                    _tabla.SetRow(control, fila + 2);
            }

            _tabla.RowCount += 2;

            _tabla.RowStyles.Insert(
                3, new RowStyle(SizeType.AutoSize));

            _tabla.RowStyles.Insert(
                4, new RowStyle(SizeType.AutoSize));

            _tabla.Controls.Add(_lblGrafico, 0, 3);
            _tabla.Controls.Add(_pnlGrafico, 0, 4);

            _pnlGrafico.Paint += DibujarGrafico;

            _lblGrafico.Visible = false;
            _pnlGrafico.Visible = false;

            _tabla.ResumeLayout(true);
        }

        private void MostrarGrafico(
            string titulo,
            IEnumerable<(string Etiqueta, decimal Valor)> datos)
        {
            _puntosGrafico.Clear();
            _puntosGrafico.AddRange(datos);

            _lblGrafico.Text = titulo;

            bool mostrar = _puntosGrafico.Count > 0;

            _lblGrafico.Visible = mostrar;
            _pnlGrafico.Visible = mostrar;

            _pnlGrafico.Invalidate();
        }

        private void OcultarGrafico()
        {
            _puntosGrafico.Clear();
            _lblGrafico.Visible = false;
            _pnlGrafico.Visible = false;
        }

        private void DibujarGrafico(
            object? sender,
            PaintEventArgs e)
        {
            if (_puntosGrafico.Count == 0)
                return;

            var g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            int izquierda = 75;
            int derecha = 25;
            int arriba = 25;
            int abajo = 55;

            int ancho = _pnlGrafico.ClientSize.Width;
            int alto = _pnlGrafico.ClientSize.Height;

            int anchoUtil = ancho - izquierda - derecha;
            int altoUtil = alto - arriba - abajo;

            if (anchoUtil <= 0 || altoUtil <= 0)
                return;

            decimal minimo = Math.Min(
                0m, _puntosGrafico.Min(p => p.Valor));

            decimal maximo = Math.Max(
                0m, _puntosGrafico.Max(p => p.Valor));

            if (maximo == minimo)
                maximo = minimo + 1m;

            using var lapizEjes = new Pen(
                Color.FromArgb(205, 213, 225), 1);

            using var lapizLinea = new Pen(
                Color.FromArgb(37, 99, 235), 3);

            using var lapizCuadricula = new Pen(
                Color.FromArgb(235, 239, 245), 1);

            using var pincelPuntos = new SolidBrush(
                Color.FromArgb(37, 99, 235));

            using var pincelTexto = new SolidBrush(
                Color.FromArgb(80, 95, 115));

            using var fuente = new Font("Segoe UI", 8F);

            for (int i = 0; i <= 4; i++)
            {
                float y = arriba + altoUtil * i / 4f;

                g.DrawLine(
                    lapizCuadricula,
                    izquierda,
                    y,
                    ancho - derecha,
                    y);

                decimal valor =
                    maximo - (maximo - minimo) * i / 4m;

                g.DrawString(
                    valor.ToString("N0"),
                    fuente,
                    pincelTexto,
                    5,
                    y - 8);
            }

            g.DrawLine(
                lapizEjes,
                izquierda,
                arriba + altoUtil,
                ancho - derecha,
                arriba + altoUtil);

            var puntos = new List<PointF>();

            int cantidad = _puntosGrafico.Count;

            for (int i = 0; i < cantidad; i++)
            {
                var dato = _puntosGrafico[i];

                float x = cantidad == 1
                    ? izquierda + anchoUtil / 2f
                    : izquierda +
                      anchoUtil * i / (float)(cantidad - 1);

                float proporcion =
                    (float)((dato.Valor - minimo) /
                            (maximo - minimo));

                float y = arriba +
                    altoUtil * (1f - proporcion);

                puntos.Add(new PointF(x, y));
            }

            if (puntos.Count > 1)
            {
                g.DrawLines(
                    lapizLinea,
                    puntos.ToArray());
            }

            int pasoEtiquetas = Math.Max(
                1, (int)Math.Ceiling(cantidad / 8.0));

            for (int i = 0; i < puntos.Count; i++)
            {
                var punto = puntos[i];

                g.FillEllipse(
                    pincelPuntos,
                    punto.X - 4,
                    punto.Y - 4,
                    8,
                    8);

                if (i % pasoEtiquetas == 0 ||
                    i == puntos.Count - 1)
                {
                    var texto = _puntosGrafico[i].Etiqueta;

                    var medida = g.MeasureString(
                        texto, fuente);

                    g.DrawString(
                        texto,
                        fuente,
                        pincelTexto,
                        punto.X - medida.Width / 2,
                        arriba + altoUtil + 12);
                }
            }
        }

        private void GraficarVentas(ReporteVentasResponse r)
        {
            MostrarGrafico(
                "Evolución del ingreso neto",
                r.VentasPorDia
                    .OrderBy(x => x.Fecha)
                    .Select(x => (
                        x.Fecha.ToString("dd/MM"),
                        x.IngresoNeto)));
        }

        private void GraficarReservas(ReporteReservasResponse r)
        {
            MostrarGrafico(
                "Evolución de reservas",
                r.ReservasPorDia
                    .OrderBy(x => x.Fecha)
                    .Select(x => (
                        x.Fecha.ToString("dd/MM"),
                        (decimal)x.CantidadReservas)));
        }
    }
}
