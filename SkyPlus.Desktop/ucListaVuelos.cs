// SkyPlus.Desktop/ucListaVuelos.cs
// Lista de vuelos para elegir primero el vuelo en Check-in y en Boarding Pass.
// Cada vuelo muestra su ruta, horario y una barra con el avance del check-in.
// También incluye DatosVuelos, que trae de la API vuelos + reservas + check-ins
// y arma el resumen de cada vuelo.

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;

namespace SkyPlus.Desktop
{
    // Estado de un pasajero confirmado dentro de un vuelo
    public class PasajeroVuelo
    {
        public ReservaResponse Reserva { get; set; } = new();
        public CheckInResponse? CheckIn { get; set; }

        public string Estado =>
            CheckIn == null ? "Sin check-in" :
            CheckIn.EstadoCheckin == "Embarcado" ? "Embarcado" : "Check-in realizado";
    }

    public class ResumenVuelo
    {
        public VueloResponse Vuelo { get; set; } = new();
        public List<PasajeroVuelo> Pasajeros { get; set; } = new();

        public int Confirmadas => Pasajeros.Count;
        public int CheckIns => Pasajeros.Count(p => p.CheckIn != null);
        public int Embarcados => Pasajeros.Count(p => p.Estado == "Embarcado");
        public int SinCheckIn => Confirmadas - CheckIns;

        public string Ruta => $"{Iata(Vuelo.LugarOrigen)} → {Iata(Vuelo.LugarDestino)}";
        public string RutaCompleta => $"{Ciudad(Vuelo.LugarOrigen)} → {Ciudad(Vuelo.LugarDestino)}";

        public string SalidaTexto
        {
            get
            {
                var s = Vuelo.Salida;
                if (s.Date == DateTime.Today) return "Hoy " + s.ToString("HH:mm");
                if (s.Date == DateTime.Today.AddDays(1)) return "Mañana " + s.ToString("HH:mm");
                return s.ToString("dd/MM HH:mm");
            }
        }

        // "AEP - Buenos Aires" → "AEP" / "Buenos Aires"
        private static string Iata(string lugar) => lugar.Split(" - ")[0].Trim();
        private static string Ciudad(string lugar)
        {
            var partes = lugar.Split(" - ");
            return partes.Length > 1 ? partes[1].Trim() : lugar;
        }
    }

    public static class DatosVuelos
    {
        // Vuelos habilitados para check-in desde hoy hasta dentro de 'dias' días
        public static async Task<List<ResumenVuelo>> CargarAsync(int dias = 2)
        {
            var tVuelos = new VueloClient().ObtenerTodosAsync();
            var tReservas = new ReservaClient().ObtenerTodosAsync();
            var tCheckIns = new CheckInClient().ObtenerTodosAsync();
            await Task.WhenAll(tVuelos, tReservas, tCheckIns);

            var vuelos = tVuelos.Result ?? new List<VueloResponse>();
            var reservas = tReservas.Result ?? new List<ReservaResponse>();
            var checkIns = (tCheckIns.Result ?? new List<CheckInResponse>())
                .GroupBy(c => c.IdReserva)
                .ToDictionary(g => g.Key, g => g.First());

            DateTime hoy = DateTime.Today;

            return vuelos
                .Where(v => (v.EstadoVuelo == "Programado" || v.EstadoVuelo == "Confirmado") &&
                            v.Salida.Date >= hoy && v.Salida.Date <= hoy.AddDays(dias))
                .OrderBy(v => v.Salida)
                .Select(v => new ResumenVuelo
                {
                    Vuelo = v,
                    Pasajeros = reservas
                        .Where(r => r.IdVuelo == v.IdVuelo && r.EstadoReserva == "Confirmada")
                        .Select(r => new PasajeroVuelo
                        {
                            Reserva = r,
                            CheckIn = checkIns.TryGetValue(r.IdReserva, out var c) ? c : null
                        })
                        .OrderBy(p => p.Reserva.Pasajero)
                        .ToList()
                })
                .ToList();
        }
    }

    [DesignerCategory("Code")]
    public class ucListaVuelos : UserControl
    {
        private readonly ListBox _lista;
        private readonly Label _lblVacio;

        // Se dispara cuando el usuario elige un vuelo
        public event EventHandler<ResumenVuelo>? VueloSeleccionado;

        // "check-in" o "embarque": qué avance muestra la barra de cada vuelo
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool MostrarEmbarque { get; set; }

        // Opcional: qué muestra la barra de cada vuelo (si no se indica, el avance del check-in/embarque)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<ResumenVuelo, (string Texto, int Hechos, int Total)>? Avance { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MensajeVacio
        {
            get => _lblVacio.Text;
            set => _lblVacio.Text = value;
        }

        public ucListaVuelos()
        {
            BackColor = Color.White;

            _lista = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 78,
                IntegralHeight = false,
                BackColor = Color.White
            };
            _lista.DrawItem += DibujarVuelo;
            _lista.Resize += (s, e) => _lista.Invalidate(); // redibuja el texto alineado a la derecha
            _lista.SelectedIndexChanged += (s, e) =>
            {
                if (_lista.SelectedItem is ResumenVuelo r)
                    VueloSeleccionado?.Invoke(this, r);
            };

            _lblVacio = new Label
            {
                Dock = DockStyle.Fill,
                Text = "No hay vuelos programados para hoy ni para los próximos días.",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = AppEstilos.FuenteNormal,
                ForeColor = AppEstilos.ColorTextoSuave,
                Visible = false
            };

            Controls.Add(_lista);
            Controls.Add(_lblVacio);
        }

        // Carga la lista y vuelve a seleccionar el vuelo que estaba elegido
        public void Mostrar(List<ResumenVuelo> vuelos, int? idVueloSeleccionado = null)
        {
            _lista.BeginUpdate();
            _lista.Items.Clear();
            foreach (var v in vuelos)
                _lista.Items.Add(v);
            _lista.EndUpdate();

            _lblVacio.Visible = vuelos.Count == 0;
            _lista.Visible = vuelos.Count > 0;

            if (vuelos.Count == 0) return;

            int indice = idVueloSeleccionado == null
                ? 0
                : vuelos.FindIndex(v => v.Vuelo.IdVuelo == idVueloSeleccionado);
            _lista.SelectedIndex = indice < 0 ? 0 : indice;
        }

        private void DibujarVuelo(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || _lista.Items[e.Index] is not ResumenVuelo r) return;

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            bool seleccionado = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var b = e.Bounds;

            using (var fondo = new SolidBrush(seleccionado ? AppEstilos.ColorPrimarioSuave : Color.White))
                g.FillRectangle(fondo, b);

            if (seleccionado)
            {
                using var marca = new SolidBrush(AppEstilos.ColorPrimario);
                g.FillRectangle(marca, b.Left, b.Top, 4, b.Height);
            }

            int x = b.Left + 16;
            int ancho = b.Width - 32;

            // Línea 1: número de vuelo y ruta
            TextRenderer.DrawText(g, r.Vuelo.NumeroVuelo, AppEstilos.FuenteSubtitulo,
                new Point(x, b.Top + 10), AppEstilos.ColorTextoOscuro);
            TextRenderer.DrawText(g, r.Ruta, AppEstilos.FuenteNormal,
                new Rectangle(x, b.Top + 10, ancho, 22), AppEstilos.ColorTextoOscuro, TextFormatFlags.Right);

            // Línea 2: salida y estado
            TextRenderer.DrawText(g, $"{r.SalidaTexto} · {r.Vuelo.EstadoVuelo}", AppEstilos.FuentePequena,
                new Point(x, b.Top + 34), AppEstilos.ColorTextoSuave);

            // Línea 3: barra de avance
            int hechos, total;
            string texto;
            if (Avance != null)
            {
                (texto, hechos, total) = Avance(r);
            }
            else
            {
                hechos = MostrarEmbarque ? r.Embarcados : r.CheckIns;
                total = MostrarEmbarque ? r.CheckIns : r.Confirmadas;
                texto = MostrarEmbarque ? $"{hechos}/{total} embarcados" : $"{hechos}/{total} check-in";
            }

            var zonaTexto = new Rectangle(x, b.Top + 50, ancho, 18);
            TextRenderer.DrawText(g, texto, AppEstilos.FuentePequena, zonaTexto, AppEstilos.ColorTextoSuave, TextFormatFlags.Right);

            int anchoBarra = ancho - 110;
            var barra = new Rectangle(x, b.Top + 56, anchoBarra, 6);
            using (var fondoBarra = new SolidBrush(AppEstilos.ColorBorde))
                g.FillRectangle(fondoBarra, barra);
            if (total > 0 && hechos > 0)
            {
                bool completo = hechos >= total;
                using var avance = new SolidBrush(completo ? AppEstilos.ColorExito : AppEstilos.ColorPrimario);
                g.FillRectangle(avance, barra.Left, barra.Top, (int)(barra.Width * (double)hechos / total), barra.Height);
            }

            using (var separador = new Pen(AppEstilos.ColorBorde))
                g.DrawLine(separador, b.Left, b.Bottom - 1, b.Right, b.Bottom - 1);
        }
    }

    // Encabezado blanco con el vuelo elegido y sus números principales
    [DesignerCategory("Code")]
    public class EncabezadoVuelo : Panel
    {
        private readonly Label _lblTitulo;
        private readonly Label _lblDetalle;
        private readonly TableLayoutPanel _datos;

        public EncabezadoVuelo()
        {
            BackColor = Color.White;
            Padding = new Padding(20, 14, 20, 10);
            Height = 150;
            Paint += (s, e) =>
            {
                using var lapiz = new Pen(AppEstilos.ColorBorde);
                e.Graphics.DrawRectangle(lapiz, 0, 0, Width - 1, Height - 1);
            };

            _lblTitulo = new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                Font = AppEstilos.FuenteTitulo,
                ForeColor = AppEstilos.ColorTextoOscuro,
                AutoEllipsis = true,
                Text = "Elegí un vuelo de la lista"
            };
            _lblDetalle = new Label
            {
                Dock = DockStyle.Top,
                Height = 26,
                Font = AppEstilos.FuenteNormal,
                ForeColor = AppEstilos.ColorTextoSuave,
                AutoEllipsis = true
            };
            _datos = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                BackColor = Color.White
            };

            Controls.Add(_datos);
            Controls.Add(_lblDetalle);
            Controls.Add(_lblTitulo);
            _datos.BringToFront();
        }

        public void Mostrar(ResumenVuelo? r, params (string Titulo, int Valor, Color Color)[] datos)
        {
            foreach (Control c in _datos.Controls.Cast<Control>().ToList())
                c.Dispose();
            _datos.Controls.Clear();
            _datos.ColumnStyles.Clear();

            if (r == null)
            {
                _lblTitulo.Text = "Elegí un vuelo de la lista";
                _lblDetalle.Text = "";
                return;
            }

            _lblTitulo.Text = $"{r.Vuelo.NumeroVuelo} · {r.RutaCompleta}";
            _lblDetalle.Text = $"Salida {r.SalidaTexto} · Llegada {r.Vuelo.Llegada:HH:mm} · {r.Vuelo.EstadoVuelo} · {r.Vuelo.Aeronave}";

            _datos.ColumnCount = datos.Length;
            for (int i = 0; i < datos.Length; i++)
            {
                _datos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / datos.Length));

                var celda = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = Color.White };
                var lblValor = new Label
                {
                    Dock = DockStyle.Top,
                    Height = 40,
                    Font = AppEstilos.FuenteNumero,
                    ForeColor = datos[i].Color,
                    Text = datos[i].Valor.ToString()
                };
                var lblTitulo = new Label
                {
                    Dock = DockStyle.Top,
                    Height = 20,
                    Font = AppEstilos.FuentePequena,
                    ForeColor = AppEstilos.ColorTextoSuave,
                    Text = datos[i].Titulo
                };
                celda.Controls.Add(lblValor);
                celda.Controls.Add(lblTitulo); // el último agregado queda arriba
                _datos.Controls.Add(celda, i, 0);
            }
        }
    }

    public static class BoardingPassPdf
    {
        // Descarga el PDF que genera la API y lo abre con el visor del sistema
        public static async Task AbrirAsync(int idCheckin, string codigoReserva)
        {
            var pdf = await new CheckInClient().DescargarBoardingPassAsync(idCheckin);

            var ruta = Path.Combine(Path.GetTempPath(),
                $"SkyPlus_BoardingPass_{codigoReserva}_{DateTime.Now:HHmmss}.pdf");

            await File.WriteAllBytesAsync(ruta, pdf);

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(ruta) { UseShellExecute = true });
        }
    }
}
