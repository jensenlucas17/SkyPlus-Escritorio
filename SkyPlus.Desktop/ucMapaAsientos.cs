// SkyPlus.Desktop/ucMapaAsientos.cs
// Mapa de asientos dibujado dentro de un avión (nariz, alas, motores y cola).
// Los asientos son los mismos de siempre: 30 filas x A-F, con pasillo entre C y D.
// Uso (sin cambios): Cargar(asientos), evento AsientoSeleccionado, AsientoActual, MarcarOcupado.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop
{
    [DesignerCategory("Code")]
    public class ucMapaAsientos : UserControl
    {
        private const int Filas = 30;
        private static readonly string[] Letras = { "A", "B", "C", "D", "E", "F" };

        // Colores de los asientos
        private static readonly Color ColorLibre = ColorTranslator.FromHtml("#DCFCE7");
        private static readonly Color TextoLibre = ColorTranslator.FromHtml("#166534");
        private static readonly Color BordeLibre = ColorTranslator.FromHtml("#86EFAC");
        private static readonly Color ColorOcupado = ColorTranslator.FromHtml("#E2E8F0");
        private static readonly Color TextoOcupado = ColorTranslator.FromHtml("#94A3B8");
        private static readonly Color ColorSeleccionado = AppEstilos.ColorPrimario;
        private static readonly Color ColorSinDatos = ColorTranslator.FromHtml("#F1F5F9");

        // Colores del avión
        private static readonly Color ColorFuselaje = ColorTranslator.FromHtml("#FFFFFF");
        private static readonly Color BordeFuselaje = ColorTranslator.FromHtml("#CBD5E1");
        private static readonly Color ColorAla = ColorTranslator.FromHtml("#E2E8F0");
        private static readonly Color ColorMotor = ColorTranslator.FromHtml("#CBD5E1");
        private static readonly Color ColorCabina = ColorTranslator.FromHtml("#334155");

        // Medidas de la cabina (en píxeles)
        private const int AnchoNumero = 30;
        private const int AnchoAsiento = 38;
        private const int AnchoPasillo = 30;
        private const int AltoFila = 30;
        private const int AltoNariz = 190;
        private const int AltoCola = 170;
        private const int MargenLateral = 190; // lugar para las alas

        private static readonly Font FuenteAsiento = new Font("Segoe UI Semibold", 8F);
        private static readonly Font FuenteEtiqueta = new Font("Segoe UI", 8F, FontStyle.Bold);

        private readonly Dictionary<string, Button> _botones = new();
        private readonly Dictionary<string, AsientoResponse> _asientos = new();
        private Button? _botonSeleccionado;

        private readonly Panel _contenedor;
        private readonly LienzoAvion _lienzo;
        private readonly TableLayoutPanel _tabla;

        public event EventHandler<AsientoResponse>? AsientoSeleccionado;

        public AsientoResponse? AsientoActual =>
            _botonSeleccionado?.Tag is string clave && _asientos.TryGetValue(clave, out var asiento)
                ? asiento
                : null;

        public ucMapaAsientos()
        {
            BackColor = AppEstilos.ColorSecundario;

            // ----- Leyenda -----
            var leyenda = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(4, 10, 0, 0),
                BackColor = AppEstilos.ColorSecundario
            };
            leyenda.Controls.Add(new Label
            {
                Text = "Elegí un asiento libre:",
                AutoSize = true,
                Font = AppEstilos.FuenteBoton,
                ForeColor = AppEstilos.ColorTextoOscuro,
                Margin = new Padding(0, 1, 16, 0)
            });
            leyenda.Controls.Add(CrearItemLeyenda("Libre", ColorLibre, BordeLibre));
            leyenda.Controls.Add(CrearItemLeyenda("Ocupado", ColorOcupado, ColorOcupado));
            leyenda.Controls.Add(CrearItemLeyenda("Seleccionado", ColorSeleccionado, ColorSeleccionado));

            // ----- Cabina (asientos) -----
            _tabla = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 9,
                RowCount = Filas + 1,
                BackColor = ColorFuselaje,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            ConstruirAsientos();

            int anchoCabina = AnchoNumero * 2 + AnchoAsiento * 6 + AnchoPasillo;
            int altoCabina = AltoFila * (Filas + 1);

            // ----- Avión -----
            _lienzo = new LienzoAvion(anchoCabina, AltoNariz, altoCabina, AltoCola)
            {
                Size = new Size(anchoCabina + 40 + MargenLateral * 2, AltoNariz + altoCabina + AltoCola + 20)
            };
            _tabla.Location = new Point((_lienzo.Width - anchoCabina) / 2, AltoNariz);
            _lienzo.Controls.Add(_tabla);

            _contenedor = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = AppEstilos.ColorSecundario
            };
            _contenedor.Controls.Add(_lienzo);
            _contenedor.Resize += (s, e) => CentrarAvion();

            Controls.Add(_contenedor);
            Controls.Add(leyenda);
        }

        private void CentrarAvion()
        {
            int x = Math.Max(0, (_contenedor.ClientSize.Width - _lienzo.Width) / 2);
            _lienzo.Location = new Point(x + _contenedor.AutoScrollPosition.X, _lienzo.Top);
        }

        private void ConstruirAsientos()
        {
            _tabla.SuspendLayout();

            // Columnas: número | A B C | pasillo | D E F | número
            _tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, AnchoNumero));
            for (int i = 0; i < 3; i++) _tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, AnchoAsiento));
            _tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, AnchoPasillo));
            for (int i = 0; i < 3; i++) _tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, AnchoAsiento));
            _tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, AnchoNumero));

            for (int i = 0; i <= Filas; i++)
                _tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, AltoFila));

            for (int i = 0; i < Letras.Length; i++)
                _tabla.Controls.Add(CrearEtiqueta(Letras[i]), ColumnaDeLetra(i), 0);

            for (int fila = 1; fila <= Filas; fila++)
            {
                _tabla.Controls.Add(CrearEtiqueta(fila.ToString()), 0, fila);
                _tabla.Controls.Add(CrearEtiqueta(fila.ToString()), 8, fila);

                for (int i = 0; i < Letras.Length; i++)
                {
                    var clave = $"{fila}{Letras[i]}";
                    var boton = CrearBoton(Letras[i], clave);
                    _botones[clave] = boton;
                    _tabla.Controls.Add(boton, ColumnaDeLetra(i), fila);
                }
            }

            _tabla.ResumeLayout(true);
        }

        // A, B, C usan las columnas 1 a 3; la 4 es el pasillo; D, E, F van de la 5 a la 7
        private static int ColumnaDeLetra(int indice) => indice < 3 ? indice + 1 : indice + 2;

        private static Label CrearEtiqueta(string texto) => new Label
        {
            Text = texto,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = FuenteEtiqueta,
            ForeColor = AppEstilos.ColorTextoSuave,
            BackColor = ColorFuselaje,
            Margin = Padding.Empty
        };

        private Button CrearBoton(string letra, string clave)
        {
            var boton = new Button
            {
                Text = letra,
                Tag = clave,
                Dock = DockStyle.Fill,
                Margin = new Padding(3),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorSinDatos,
                ForeColor = TextoOcupado,
                Font = FuenteAsiento,
                UseVisualStyleBackColor = false,
                TabStop = false
            };
            boton.FlatAppearance.BorderSize = 1;
            boton.FlatAppearance.BorderColor = ColorSinDatos;
            boton.Click += Boton_Click;
            return boton;
        }

        private static Control CrearItemLeyenda(string texto, Color relleno, Color borde)
        {
            var panel = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 0, 14, 0), BackColor = AppEstilos.ColorSecundario };
            var muestra = new Panel { Width = 16, Height = 16, BackColor = relleno, Margin = new Padding(0, 2, 6, 0) };
            muestra.Paint += (s, e) =>
            {
                using var lapiz = new Pen(borde);
                e.Graphics.DrawRectangle(lapiz, 0, 0, muestra.Width - 1, muestra.Height - 1);
            };
            panel.Controls.Add(muestra);
            panel.Controls.Add(new Label { Text = texto, AutoSize = true, Font = AppEstilos.FuenteNormal, ForeColor = AppEstilos.ColorTextoOscuro, Margin = new Padding(0, 1, 0, 0) });
            return panel;
        }

        // Carga o recarga los estados de los asientos de un vuelo
        public void Cargar(IEnumerable<AsientoResponse> asientos)
        {
            _asientos.Clear();
            _botonSeleccionado = null;

            foreach (var boton in _botones.Values)
            {
                boton.BackColor = ColorSinDatos;
                boton.ForeColor = TextoOcupado;
                boton.FlatAppearance.BorderColor = ColorSinDatos;
                boton.Cursor = Cursors.Default;
            }

            foreach (var asiento in asientos)
            {
                var clave = $"{asiento.Fila}{asiento.Letra}";
                if (!_botones.TryGetValue(clave, out var boton)) continue;

                _asientos[clave] = asiento;
                PintarSegunEstado(boton, asiento);
            }

            _contenedor.AutoScrollPosition = new Point(0, 0);
        }

        private static void PintarSegunEstado(Button boton, AsientoResponse asiento)
        {
            bool libre = asiento.Estado == "Libre";
            boton.BackColor = libre ? ColorLibre : ColorOcupado;
            boton.ForeColor = libre ? TextoLibre : TextoOcupado;
            boton.FlatAppearance.BorderColor = libre ? BordeLibre : ColorOcupado;
            boton.FlatAppearance.MouseOverBackColor = libre ? BordeLibre : ColorOcupado;
            boton.Cursor = libre ? Cursors.Hand : Cursors.Default;
        }

        private void Boton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button boton || boton.Tag is not string clave) return;
            if (!_asientos.TryGetValue(clave, out var asiento) || asiento.Estado != "Libre") return;

            if (_botonSeleccionado?.Tag is string claveAnterior &&
                _asientos.TryGetValue(claveAnterior, out var anterior))
            {
                PintarSegunEstado(_botonSeleccionado, anterior);
            }

            _botonSeleccionado = boton;
            boton.BackColor = ColorSeleccionado;
            boton.ForeColor = Color.White;
            boton.FlatAppearance.BorderColor = ColorSeleccionado;
            boton.FlatAppearance.MouseOverBackColor = ColorSeleccionado;
            AsientoSeleccionado?.Invoke(this, asiento);
        }

        // Feedback inmediato al confirmar una venta; el estado real lo confirma la API al recargar
        public void MarcarOcupado(int idAsiento)
        {
            var asiento = _asientos.Values.FirstOrDefault(a => a.IdAsiento == idAsiento);
            if (asiento == null) return;

            asiento.Estado = "Ocupado";
            var clave = $"{asiento.Fila}{asiento.Letra}";

            if (_botonSeleccionado?.Tag as string == clave)
                _botonSeleccionado = null;

            PintarSegunEstado(_botones[clave], asiento);
        }

        // =====================================================================
        // Dibujo del avión (fondo de la cabina)
        // =====================================================================
        private class LienzoAvion : Panel
        {
            private readonly int _anchoCabina;
            private readonly int _altoNariz;
            private readonly int _altoCabina;
            private readonly int _altoCola;

            public LienzoAvion(int anchoCabina, int altoNariz, int altoCabina, int altoCola)
            {
                _anchoCabina = anchoCabina;
                _altoNariz = altoNariz;
                _altoCabina = altoCabina;
                _altoCola = altoCola;
                DoubleBuffered = true;
                BackColor = AppEstilos.ColorSecundario;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                float cx = Width / 2f;
                float anchoCuerpo = _anchoCabina + 36;      // un poco más ancho que los asientos
                float izq = cx - anchoCuerpo / 2;
                float der = cx + anchoCuerpo / 2;
                float inicioCabina = _altoNariz;
                float finCabina = _altoNariz + _altoCabina;

                using var pincelFuselaje = new SolidBrush(ColorFuselaje);
                using var pincelAla = new SolidBrush(ColorAla);
                using var pincelMotor = new SolidBrush(ColorMotor);
                using var pincelCabina = new SolidBrush(ColorCabina);
                using var borde = new Pen(BordeFuselaje, 2f);

                // ----- Alas (detrás del fuselaje), a la altura de las filas 11-17 -----
                float raizAla = inicioCabina + AltoFila * 11;
                float largoAla = MargenLateral - 10;
                DibujarAla(g, pincelAla, borde, izq, raizAla, -largoAla);
                DibujarAla(g, pincelAla, borde, der, raizAla, largoAla);

                // Motores debajo de las alas
                var motorIzq = new RectangleF(izq - largoAla * 0.45f - 16, raizAla + 40, 32, 70);
                var motorDer = new RectangleF(der + largoAla * 0.45f - 16, raizAla + 40, 32, 70);
                foreach (var motor in new[] { motorIzq, motorDer })
                {
                    using var camino = Redondeado(motor, 14);
                    g.FillPath(pincelMotor, camino);
                    g.DrawPath(borde, camino);
                }

                // ----- Estabilizadores de cola -----
                float raizCola = finCabina + _altoCola * 0.35f;
                DibujarAla(g, pincelAla, borde, izq + 30, raizCola, -(largoAla * 0.45f), escala: 0.45f);
                DibujarAla(g, pincelAla, borde, der - 30, raizCola, largoAla * 0.45f, escala: 0.45f);

                // ----- Fuselaje: nariz redondeada + cabina + cola que se angosta -----
                using (var fuselaje = new GraphicsPath())
                {
                    float altoArco = _altoNariz * 2 - 30;
                    fuselaje.AddArc(izq, 10, anchoCuerpo, altoArco, 180, 180);
                    fuselaje.AddLine(der, 10 + altoArco / 2, der, finCabina);
                    fuselaje.AddBezier(der, finCabina, der, finCabina + _altoCola * 0.5f,
                        cx + 22, finCabina + _altoCola * 0.8f, cx + 14, finCabina + _altoCola);
                    fuselaje.AddLine(cx + 14, finCabina + _altoCola, cx - 14, finCabina + _altoCola);
                    fuselaje.AddBezier(cx - 14, finCabina + _altoCola, cx - 22, finCabina + _altoCola * 0.8f,
                        izq, finCabina + _altoCola * 0.5f, izq, finCabina);
                    fuselaje.CloseFigure();

                    g.FillPath(pincelFuselaje, fuselaje);
                    g.DrawPath(borde, fuselaje);
                }

                // Ventanas de la cabina de mando
                float yVentanas = 10 + _altoNariz * 0.42f;
                g.FillPolygon(pincelCabina, new[]
                {
                    new PointF(cx - 46, yVentanas + 18), new PointF(cx - 30, yVentanas), new PointF(cx - 4, yVentanas),
                    new PointF(cx - 4, yVentanas + 18)
                });
                g.FillPolygon(pincelCabina, new[]
                {
                    new PointF(cx + 4, yVentanas + 18), new PointF(cx + 4, yVentanas), new PointF(cx + 30, yVentanas),
                    new PointF(cx + 46, yVentanas + 18)
                });

                // Texto "Frente" para orientar al agente
                TextRenderer.DrawText(g, "FRENTE", AppEstilos.FuentePequena,
                    new Rectangle((int)izq, (int)(yVentanas + 34), (int)anchoCuerpo, 20), AppEstilos.ColorTextoSuave,
                    TextFormatFlags.HorizontalCenter);
            }

            // Ala con flecha hacia atrás. 'largo' negativo = hacia la izquierda
            private static void DibujarAla(Graphics g, Brush relleno, Pen borde, float xRaiz, float yRaiz, float largo, float escala = 1f)
            {
                float cuerdaRaiz = 150 * escala;
                float cuerdaPunta = 46 * escala;
                float flecha = 120 * escala;

                var puntos = new[]
                {
                    new PointF(xRaiz, yRaiz),
                    new PointF(xRaiz + largo, yRaiz + flecha),
                    new PointF(xRaiz + largo, yRaiz + flecha + cuerdaPunta),
                    new PointF(xRaiz, yRaiz + cuerdaRaiz)
                };
                g.FillPolygon(relleno, puntos);
                g.DrawPolygon(borde, puntos);
            }

            private static GraphicsPath Redondeado(RectangleF r, float radio)
            {
                var camino = new GraphicsPath();
                float d = radio * 2;
                camino.AddArc(r.Left, r.Top, d, d, 180, 90);
                camino.AddArc(r.Right - d, r.Top, d, d, 270, 90);
                camino.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                camino.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
                camino.CloseFigure();
                return camino;
            }
        }
    }
}
