// SkyPlus.Desktop/frmBoardingPass.cs
// Boarding Pass y embarque por vuelo:
//   1. A la izquierda se elige el vuelo.
//   2. Arriba se ve cuántos hicieron check-in y cuántos ya embarcaron.
//   3. En la lista aparece cada pasajero confirmado con su estado
//      (sin check-in / check-in realizado / embarcado).
//   4. Abajo: abrir el boarding pass (PDF) o registrar el embarque.

using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Utilidades;

namespace SkyPlus.Desktop
{
    public partial class frmBoardingPass : Form
    {
        private readonly CheckInClient _checkInClient = new CheckInClient();

        private List<ResumenVuelo> _vuelos = new();
        private ResumenVuelo? _vueloActual;

        private ucListaVuelos _listaVuelos = null!;
        private EncabezadoVuelo _encabezado = null!;
        private TextBox _txtBuscar = null!;
        private ComboBox _cmbFiltro = null!;
        private DataGridView _dgvPasajeros = null!;
        private Label _lblSeleccion = null!;
        private Button _btnAbrirPdf = null!;
        private Button _btnEmbarcar = null!;
        private Button _btnRefrescar = null!;

        public frmBoardingPass()
        {
            InitializeComponent();
        }

        private async void frmBoardingPass_Load(object sender, EventArgs e)
        {
            ConstruirPantalla();
            await CargarVuelosAsync();
        }

        // =====================================================================
        // Datos
        // =====================================================================
        private async Task CargarVuelosAsync()
        {
            int? idVuelo = _vueloActual?.Vuelo.IdVuelo;
            int? idReserva = Seleccionado()?.Reserva.IdReserva;

            try
            {
                _btnRefrescar.Enabled = false;
                _vuelos = await DatosVuelos.CargarAsync(2);
                _listaVuelos.Mostrar(_vuelos, idVuelo);

                if (_vuelos.Count == 0)
                    MostrarVuelo(null);
                else if (idReserva != null)
                    SeleccionarReserva(idReserva.Value);
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
            }
            finally
            {
                _btnRefrescar.Enabled = true;
            }
        }

        private void MostrarVuelo(ResumenVuelo? vuelo)
        {
            _vueloActual = vuelo;
            int faltanEmbarcar = vuelo == null ? 0 : vuelo.CheckIns - vuelo.Embarcados;

            _encabezado.Mostrar(vuelo,
                ("Pasajeros confirmados", vuelo?.Confirmadas ?? 0, AppEstilos.ColorTextoOscuro),
                ("Con check-in", vuelo?.CheckIns ?? 0, AppEstilos.ColorExito),
                ("Ya embarcaron", vuelo?.Embarcados ?? 0, AppEstilos.ColorPrimario),
                ("Faltan embarcar", faltanEmbarcar, faltanEmbarcar > 0 ? AppEstilos.ColorAcento : AppEstilos.ColorTextoOscuro),
                ("Sin check-in", vuelo?.SinCheckIn ?? 0, AppEstilos.ColorTextoSuave));

            LlenarGrilla();
        }

        private void LlenarGrilla()
        {
            _dgvPasajeros.Rows.Clear();
            if (_vueloActual != null)
            {
                string texto = _txtBuscar.Text.Trim();
                string filtro = _cmbFiltro.SelectedItem as string ?? "Todos";

                var lista = _vueloActual.Pasajeros.Where(p =>
                    (filtro == "Todos" || p.Estado == filtro) &&
                    (texto == "" ||
                     p.Reserva.Pasajero.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                     p.Reserva.CodigoReserva.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                     p.Reserva.Asiento.Contains(texto, StringComparison.OrdinalIgnoreCase)));

                foreach (var p in lista)
                {
                    int fila = _dgvPasajeros.Rows.Add(
                        p.Reserva.CodigoReserva,
                        p.Reserva.Pasajero,
                        p.Reserva.Asiento,
                        p.Estado,
                        p.CheckIn == null ? "—" : p.CheckIn.FechaLocal.ToString("dd/MM HH:mm"));
                    _dgvPasajeros.Rows[fila].Tag = p;
                }
            }
            ActualizarAcciones();
        }

        private void SeleccionarReserva(int idReserva)
        {
            foreach (DataGridViewRow fila in _dgvPasajeros.Rows)
            {
                if (fila.Tag is PasajeroVuelo p && p.Reserva.IdReserva == idReserva)
                {
                    _dgvPasajeros.CurrentCell = fila.Cells[0];
                    return;
                }
            }
        }

        private PasajeroVuelo? Seleccionado() =>
            _dgvPasajeros?.CurrentRow?.Tag as PasajeroVuelo;

        // Habilita los botones según el estado del pasajero elegido
        private void ActualizarAcciones()
        {
            var p = Seleccionado();
            _btnAbrirPdf.Enabled = p?.CheckIn != null;
            _btnEmbarcar.Enabled = p?.Estado == "Check-in realizado";

            if (p == null)
            {
                _lblSeleccion.Text = _vueloActual == null ? "Elegí un vuelo de la lista." : "Elegí un pasajero de la lista.";
                _lblSeleccion.ForeColor = AppEstilos.ColorTextoSuave;
                return;
            }

            string detalle = p.Estado switch
            {
                "Sin check-in" => "Todavía no hizo el check-in: no tiene boarding pass.",
                "Embarcado" => "Ya embarcó. Podés volver a abrir su boarding pass.",
                _ => "Tiene check-in: podés imprimir el boarding pass o registrar el embarque."
            };
            _lblSeleccion.Text = $"{p.Reserva.Pasajero} · Asiento {p.Reserva.Asiento} — {detalle}";
            _lblSeleccion.ForeColor = ColorEstado(p.Estado);
        }

        // =====================================================================
        // Acciones
        // =====================================================================
        private async void btnAbrirPdf_Click(object? sender, EventArgs e)
        {
            var p = Seleccionado();
            if (p?.CheckIn == null) return;

            try
            {
                _btnAbrirPdf.Enabled = false;
                await BoardingPassPdf.AbrirAsync(p.CheckIn.IdCheckin, p.Reserva.CodigoReserva);
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
            }
            catch (IOException ex)
            {
                MessageBox.Show("No se pudo guardar el PDF: " + ex.Message, "Boarding Pass",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                ActualizarAcciones();
            }
        }

        private async void btnEmbarcar_Click(object? sender, EventArgs e)
        {
            var p = Seleccionado();
            if (p?.CheckIn == null) return;

            var confirmacion = MessageBox.Show(
                $"¿Registrar el embarque de {p.Reserva.Pasajero} (asiento {p.Reserva.Asiento})?",
                "Registrar embarque", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                _btnEmbarcar.Enabled = false;
                var resultado = await _checkInClient.RegistrarEmbarqueAsync(p.CheckIn.CodigoQr);

                MessageBox.Show(resultado?.Mensaje ?? "Embarque registrado.", "Embarque",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await CargarVuelosAsync();
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
                ActualizarAcciones();
            }
        }

        // =====================================================================
        // Armado de la pantalla
        // =====================================================================
        private void ConstruirPantalla()
        {
            BackColor = AppEstilos.ColorSecundario;
            Padding = new Padding(24, 20, 24, 20);
            Font = AppEstilos.FuenteNormal;

            // ---------- Columna izquierda: vuelos ----------
            var tarjetaVuelos = new Panel { Dock = DockStyle.Left, Width = 320, BackColor = Color.White };
            tarjetaVuelos.Paint += DibujarBorde;

            var cabeceraVuelos = new Panel { Dock = DockStyle.Top, Height = 66, BackColor = Color.White, Padding = new Padding(16, 12, 12, 8) };
            _btnRefrescar = new Button { Text = "Refrescar", Dock = DockStyle.Right, Width = 96 };
            AppEstilos.EstilizarBotonSecundario(_btnRefrescar);
            _btnRefrescar.Click += async (s, e) => await CargarVuelosAsync();
            var lblVuelos = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Vuelos\nHoy y los próximos 2 días",
                Font = AppEstilos.FuenteSubtitulo,
                ForeColor = AppEstilos.ColorTextoOscuro
            };
            cabeceraVuelos.Controls.Add(lblVuelos);
            cabeceraVuelos.Controls.Add(_btnRefrescar);
            lblVuelos.BringToFront();

            _listaVuelos = new ucListaVuelos { Dock = DockStyle.Fill, MostrarEmbarque = true };
            _listaVuelos.VueloSeleccionado += (s, vuelo) => MostrarVuelo(vuelo);

            tarjetaVuelos.Controls.Add(_listaVuelos);
            tarjetaVuelos.Controls.Add(cabeceraVuelos);
            _listaVuelos.BringToFront();

            // ---------- Columna derecha ----------
            var derecha = new Panel { Dock = DockStyle.Fill, BackColor = AppEstilos.ColorSecundario };
            _encabezado = new EncabezadoVuelo { Dock = DockStyle.Top };

            var tarjetaLista = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16, 12, 16, 12) };
            tarjetaLista.Paint += DibujarBorde;

            var barraFiltros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, BackColor = Color.White, WrapContents = false };
            var lblLista = new Label
            {
                Text = "Pasajeros del vuelo",
                Font = AppEstilos.FuenteSubtitulo,
                ForeColor = AppEstilos.ColorTextoOscuro,
                AutoSize = true,
                Margin = new Padding(0, 6, 24, 0)
            };
            _txtBuscar = new TextBox
            {
                Width = 300,
                Font = AppEstilos.FuenteNormal,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Buscar por pasajero, código o asiento",
                Margin = new Padding(0, 3, 12, 0)
            };
            _txtBuscar.TextChanged += (s, e) => LlenarGrilla();
            _cmbFiltro = new ComboBox
            {
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Font = AppEstilos.FuenteNormal,
                Margin = new Padding(0, 3, 0, 0)
            };
            _cmbFiltro.Items.AddRange(new object[] { "Todos", "Check-in realizado", "Embarcado", "Sin check-in" });
            _cmbFiltro.SelectedIndex = 0;
            _cmbFiltro.SelectedIndexChanged += (s, e) => LlenarGrilla();
            barraFiltros.Controls.AddRange(new Control[] { lblLista, _txtBuscar, _cmbFiltro });

            _dgvPasajeros = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppEstilos.EstilizarGrid(_dgvPasajeros);
            _dgvPasajeros.Columns.Add("Codigo", "Reserva");
            _dgvPasajeros.Columns.Add("Pasajero", "Pasajero");
            _dgvPasajeros.Columns.Add("Asiento", "Asiento");
            _dgvPasajeros.Columns.Add("Estado", "Estado");
            _dgvPasajeros.Columns.Add("Hora", "Check-in");
            _dgvPasajeros.Columns["Pasajero"]!.FillWeight = 220;
            _dgvPasajeros.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || _dgvPasajeros.Columns[e.ColumnIndex].Name != "Estado" || e.Value is not string estado)
                    return;
                e.CellStyle!.ForeColor = ColorEstado(estado);
                e.CellStyle.Font = AppEstilos.FuenteBoton;
            };
            _dgvPasajeros.SelectionChanged += (s, e) => ActualizarAcciones();

            var separadorGrilla = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.White };
            tarjetaLista.Controls.Add(_dgvPasajeros);
            tarjetaLista.Controls.Add(separadorGrilla);
            tarjetaLista.Controls.Add(barraFiltros);
            _dgvPasajeros.BringToFront();

            // Barra de acciones
            var barraAcciones = new Panel { Dock = DockStyle.Bottom, Height = 72, BackColor = Color.White, Padding = new Padding(20, 14, 20, 14) };
            barraAcciones.Paint += DibujarBorde;

            _btnEmbarcar = new Button { Text = "Registrar embarque", Dock = DockStyle.Right, Width = 190, Enabled = false };
            AppEstilos.EstilizarBotonPrimario(_btnEmbarcar);
            _btnEmbarcar.Click += btnEmbarcar_Click;

            _btnAbrirPdf = new Button { Text = "Abrir boarding pass (PDF)", Dock = DockStyle.Right, Width = 220, Enabled = false };
            AppEstilos.EstilizarBotonSecundario(_btnAbrirPdf);
            _btnAbrirPdf.Click += btnAbrirPdf_Click;

            _lblSeleccion = new Label
            {
                Dock = DockStyle.Fill,
                Font = AppEstilos.FuenteBoton,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            barraAcciones.Controls.Add(_lblSeleccion);
            barraAcciones.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 12, BackColor = Color.White });
            barraAcciones.Controls.Add(_btnAbrirPdf);
            barraAcciones.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 12, BackColor = Color.White });
            barraAcciones.Controls.Add(_btnEmbarcar);
            _lblSeleccion.BringToFront();

            // Orden de acoplamiento: encabezado arriba, acciones abajo, lista en el medio
            derecha.Controls.Add(tarjetaLista);
            derecha.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 14, BackColor = AppEstilos.ColorSecundario });
            derecha.Controls.Add(_encabezado);
            derecha.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 14, BackColor = AppEstilos.ColorSecundario });
            derecha.Controls.Add(barraAcciones);

            Controls.Add(derecha);
            Controls.Add(new Panel { Dock = DockStyle.Left, Width = 16, BackColor = AppEstilos.ColorSecundario });
            Controls.Add(tarjetaVuelos);

            MostrarVuelo(null);
        }

        private static void DibujarBorde(object? sender, PaintEventArgs e)
        {
            if (sender is not Control c) return;
            using var lapiz = new Pen(AppEstilos.ColorBorde);
            e.Graphics.DrawRectangle(lapiz, 0, 0, c.Width - 1, c.Height - 1);
        }

        private static Color ColorEstado(string estado) => estado switch
        {
            "Sin check-in" => AppEstilos.ColorTextoSuave,
            "Check-in realizado" => AppEstilos.ColorAcento,
            "Embarcado" => AppEstilos.ColorExito,
            _ => AppEstilos.ColorTextoOscuro
        };
    }
}
