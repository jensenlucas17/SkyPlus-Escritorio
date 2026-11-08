// SkyPlus.Desktop/Estilos/GraficoBarras.cs
// Gráfico de barras simple dibujado a mano (no necesita paquetes extra).
// Sirve vertical (ej. ingresos por mes) u horizontal (ej. reservas por estado).
//
// Uso:
//   var grafico = new GraficoBarras { Dock = DockStyle.Fill, Horizontal = false };
//   grafico.FormatoValor = v => v.ToString("C0");
//   grafico.CargarDatos(new List<(string, decimal)> { ("Jun", 1200), ("Jul", 900) });

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SkyPlus.Desktop.Estilos
{
    [DesignerCategory("Code")]
    public class GraficoBarras : Control
    {
        private List<(string Etiqueta, decimal Valor, Color? Color)> _datos = new();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Horizontal { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorBarra { get; set; } = AppEstilos.ColorPrimario;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<decimal, string> FormatoValor { get; set; } = v => v.ToString("N0");

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MensajeVacio { get; set; } = "No hay datos para mostrar.";

        public GraficoBarras()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
            Font = AppEstilos.FuentePequena;
        }

        public void CargarDatos(IEnumerable<(string Etiqueta, decimal Valor)> datos)
        {
            _datos = datos.Select(d => (d.Etiqueta, d.Valor, (Color?)null)).ToList();
            Invalidate();
        }

        // Variante con un color por barra (ej. confirmadas en azul, canceladas en naranja)
        public void CargarDatos(IEnumerable<(string Etiqueta, decimal Valor, Color Color)> datos)
        {
            _datos = datos.Select(d => (d.Etiqueta, d.Valor, (Color?)d.Color)).ToList();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_datos.Count == 0 || _datos.All(d => d.Valor == 0))
            {
                TextRenderer.DrawText(g, MensajeVacio, Font, ClientRectangle,
                    AppEstilos.ColorTextoSuave,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            decimal maximo = _datos.Max(d => d.Valor);
            if (maximo <= 0) maximo = 1;

            if (Horizontal)
                DibujarHorizontal(g, maximo);
            else
                DibujarVertical(g, maximo);
        }

        private void DibujarVertical(Graphics g, decimal maximo)
        {
            const int altoEtiqueta = 22;
            const int altoValor = 20;
            int altoUtil = Height - altoEtiqueta - altoValor - 4;
            if (altoUtil < 10) return;

            float anchoColumna = (float)Width / _datos.Count;
            float anchoBarra = Math.Min(48f, anchoColumna * 0.55f);
            int baseY = Height - altoEtiqueta;

            using var lapizBase = new Pen(AppEstilos.ColorBorde);
            g.DrawLine(lapizBase, 0, baseY, Width, baseY);

            for (int i = 0; i < _datos.Count; i++)
            {
                var d = _datos[i];
                float alto = (float)(d.Valor / maximo) * altoUtil;
                float x = i * anchoColumna + (anchoColumna - anchoBarra) / 2;
                var barra = new RectangleF(x, baseY - alto, anchoBarra, alto);

                using (var pincel = new SolidBrush(d.Color ?? ColorBarra))
                using (var camino = Redondeado(barra, 5, soloArriba: true))
                    g.FillPath(pincel, camino);

                var zonaValor = new Rectangle((int)(i * anchoColumna), (int)(barra.Top - altoValor), (int)anchoColumna, altoValor);
                TextRenderer.DrawText(g, FormatoValor(d.Valor), Font, zonaValor, AppEstilos.ColorTextoOscuro,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom | TextFormatFlags.EndEllipsis);

                var zonaEtiqueta = new Rectangle((int)(i * anchoColumna), baseY + 4, (int)anchoColumna, altoEtiqueta - 4);
                TextRenderer.DrawText(g, d.Etiqueta, Font, zonaEtiqueta, AppEstilos.ColorTextoSuave,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
            }
        }

        private void DibujarHorizontal(Graphics g, decimal maximo)
        {
            const int anchoEtiqueta = 110;
            const int anchoValor = 90;
            const int altoBarra = 12;
            int anchoUtil = Width - anchoEtiqueta - anchoValor - 8;
            if (anchoUtil < 10) return;

            float altoFila = Math.Min(40f, (float)Height / _datos.Count);

            for (int i = 0; i < _datos.Count; i++)
            {
                var d = _datos[i];
                float y = i * altoFila;
                float centro = y + altoFila / 2;

                var zonaEtiqueta = new Rectangle(0, (int)y, anchoEtiqueta, (int)altoFila);
                TextRenderer.DrawText(g, d.Etiqueta, AppEstilos.FuenteNormal, zonaEtiqueta, AppEstilos.ColorTextoOscuro,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                var fondo = new RectangleF(anchoEtiqueta, centro - altoBarra / 2f, anchoUtil, altoBarra);
                using (var pincelFondo = new SolidBrush(AppEstilos.ColorSecundario))
                using (var caminoFondo = Redondeado(fondo, 6, soloArriba: false))
                    g.FillPath(pincelFondo, caminoFondo);

                float largo = (float)(d.Valor / maximo) * anchoUtil;
                if (largo >= 2)
                {
                    var barra = new RectangleF(anchoEtiqueta, fondo.Top, largo, altoBarra);
                    using var pincel = new SolidBrush(d.Color ?? ColorBarra);
                    using var camino = Redondeado(barra, 6, soloArriba: false);
                    g.FillPath(pincel, camino);
                }

                var zonaValor = new Rectangle(Width - anchoValor, (int)y, anchoValor, (int)altoFila);
                TextRenderer.DrawText(g, FormatoValor(d.Valor), AppEstilos.FuenteBoton, zonaValor, AppEstilos.ColorTextoOscuro,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }
        }

        private static GraphicsPath Redondeado(RectangleF r, float radio, bool soloArriba)
        {
            var camino = new GraphicsPath();
            float d = Math.Min(radio * 2, Math.Min(r.Width, r.Height));
            if (d <= 1)
            {
                camino.AddRectangle(r);
                return camino;
            }

            camino.AddArc(r.Left, r.Top, d, d, 180, 90);
            camino.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            if (soloArriba)
            {
                camino.AddLine(r.Right, r.Bottom, r.Left, r.Bottom);
            }
            else
            {
                camino.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                camino.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            }
            camino.CloseFigure();
            return camino;
        }
    }
}
