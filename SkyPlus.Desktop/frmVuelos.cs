﻿// SkyPlus.Desktop/frmVuelos.cs
// Vuelos (Administrador / Gerente), con el mismo formato que Reservas:
//   - Izquierda: "+ Nuevo vuelo", vistas Hoy | Próximos | Anteriores | Cancelados,
//     filtros (búsqueda, estado, origen, destino, día) y la lista de vuelos.
//   - Derecha: el vuelo elegido con sus números, sus datos (aeronave, horarios, tarifa)
//     y la lista de pasajeros.
//   - Abajo: Editar vuelo y Cancelar vuelo.
// Cancelar un vuelo (regla de la API): se cancelan y REEMBOLSAN todas sus reservas,
// se liberan los asientos y la aeronave queda disponible para otro vuelo.

using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Utilidades;

namespace SkyPlus.Desktop
{
    public partial class frmVuelos : Form
    {
        private const int AsientosPorVuelo = 180;
        private const int MinutosEscala = 60;

        private enum Vista { Hoy, Proximos, Anteriores, Cancelados }

        private readonly VueloClient _vueloClient = new VueloClient();
        private readonly ReservaClient _reservaClient = new ReservaClient();
        private readonly CheckInClient _checkInClient = new CheckInClient();

        private List<VueloResponse> _vuelos = new();
        private List<ReservaResponse> _reservas = new();
        private Dictionary<int, CheckInResponse> _checkIns = new();

        private Vista _vista = Vista.Hoy;
        private VueloResponse? _vueloActual;

        private Button _btnHoy = null!;
        private Button _btnProximos = null!;
        private Button _btnAnteriores = null!;
        private Button _btnCancelados = null!;
        private TextBox _txtBuscar = null!;
        private ComboBox _cmbEstado = null!;
        private ComboBox _cmbOrigen = null!;
        private ComboBox _cmbDestino = null!;
        private DateTimePicker _dtpDia = null!;
        private ucListaVuelos _listaVuelos = null!;
        private EncabezadoVuelo _encabezado = null!;
        private Label _lblDatos = null!;
        private DataGridView _dgvPasajeros = null!;
        private Label _lblSeleccion = null!;
        private Button _btnEditar = null!;
        private Button _btnCancelar = null!;
        private Button _btnRefrescar = null!;

        public frmVuelos()
        {
            InitializeComponent();
        }

        private async void frmVuelos_Load(object sender, EventArgs e)
        {
            ConstruirPantalla();
            await CargarDatosAsync();
        }

        // =====================================================================
        // Datos
        // =====================================================================
        private async Task CargarDatosAsync()
        {
            int? idVuelo = _vueloActual?.IdVuelo;
            try
            {
                _btnRefrescar.Enabled = false;

                var tVuelos = _vueloClient.ObtenerTodosAsync();
                var tReservas = _reservaClient.ObtenerTodosAsync();
                var tCheckIns = _checkInClient.ObtenerTodosAsync();
                await Task.WhenAll(tVuelos, tReservas, tCheckIns);

                _vuelos = tVuelos.Result ?? new List<VueloResponse>();
                _reservas = (tReservas.Result ?? new List<ReservaResponse>())
                    .Where(r => r.EstadoReserva == "Confirmada" || r.EstadoReserva == "Cancelada")
                    .ToList();
                _checkIns = (tCheckIns.Result ?? new List<CheckInResponse>())
                    .GroupBy(c => c.IdReserva)
                    .ToDictionary(g => g.Key, g => g.First());

                Listados.CargarOpciones(_cmbOrigen, _vuelos.Select(v => v.LugarOrigen).OrderBy(x => x));
                Listados.CargarOpciones(_cmbDestino, _vuelos.Select(v => v.LugarDestino).OrderBy(x => x));
                CargarLista(idVuelo);
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

        private Vista VistaDe(VueloResponse v)
        {
            if (v.EstadoVuelo == "Cancelado") return Vista.Cancelados;
            if (v.EstadoVuelo == "Finalizado") return Vista.Anteriores;
            return v.Salida.Date == DateTime.Today ? Vista.Hoy : Vista.Proximos;
        }

        private List<ReservaResponse> ReservasDe(int idVuelo) => _reservas.Where(r => r.IdVuelo == idVuelo).ToList();

        private int Confirmadas(int idVuelo) => _reservas.Count(r => r.IdVuelo == idVuelo && r.EstadoReserva == "Confirmada");

        private int ConCheckIn(int idVuelo) =>
            _reservas.Count(r => r.IdVuelo == idVuelo && r.EstadoReserva == "Confirmada" && _checkIns.ContainsKey(r.IdReserva));

        private int Embarcados(int idVuelo) =>
            _reservas.Count(r => r.IdVuelo == idVuelo && r.EstadoReserva == "Confirmada" &&
                                 _checkIns.TryGetValue(r.IdReserva, out var c) && c.EstadoCheckin == "Embarcado");

        // =====================================================================
        // Lista de vuelos (izquierda)
        // =====================================================================
        private void CambiarVista(Vista vista)
        {
            _vista = vista;
            foreach (var (boton, v) in new[] { (_btnHoy, Vista.Hoy), (_btnProximos, Vista.Proximos), (_btnAnteriores, Vista.Anteriores), (_btnCancelados, Vista.Cancelados) })
            {
                if (v == vista) AppEstilos.EstilizarBotonPrimario(boton);
                else AppEstilos.EstilizarBotonSecundario(boton);
                boton.Height = 32;
            }

            // El estado solo tiene sentido en vuelos vigentes
            _cmbEstado.Enabled = vista == Vista.Hoy || vista == Vista.Proximos;
            if (!_cmbEstado.Enabled) _cmbEstado.SelectedIndex = 0;

            _listaVuelos.MensajeVacio = vista switch
            {
                Vista.Hoy => "No hay vuelos programados para hoy.",
                Vista.Proximos => "No hay vuelos próximos.",
                Vista.Anteriores => "Todavía no hay vuelos finalizados.",
                _ => "No hay vuelos cancelados."
            };
            _listaVuelos.Avance = vista switch
            {
                Vista.Anteriores => r => ($"{Embarcados(r.Vuelo.IdVuelo)}/{Confirmadas(r.Vuelo.IdVuelo)} volaron",
                                          Embarcados(r.Vuelo.IdVuelo), Confirmadas(r.Vuelo.IdVuelo)),
                Vista.Cancelados => r => ("reservas reembolsadas", 0, 1),
                _ => r => ($"{Confirmadas(r.Vuelo.IdVuelo)}/{AsientosPorVuelo} vendidos",
                           Confirmadas(r.Vuelo.IdVuelo), AsientosPorVuelo)
            };

            _vueloActual = null;
            CargarLista(null);
        }

        private void CargarLista(int? idVuelo)
        {
            string texto = _txtBuscar.Text.Trim();

            var vuelos = _vuelos
                .Where(v => VistaDe(v) == _vista)
                .Where(v => AppEstilos.Coincide(texto, v.NumeroVuelo, v.Aeronave))
                .Where(v => Listados.Pasa(_cmbEstado, v.EstadoVuelo))
                .Where(v => Listados.Pasa(_cmbOrigen, v.LugarOrigen))
                .Where(v => Listados.Pasa(_cmbDestino, v.LugarDestino))
                .Where(v => !_dtpDia.Checked || v.Salida.Date == _dtpDia.Value.Date);

            vuelos = _vista is Vista.Hoy or Vista.Proximos
                ? vuelos.OrderBy(v => v.Salida)
                : vuelos.OrderByDescending(v => v.Salida);

            var resumen = vuelos.Select(v => new ResumenVuelo
            {
                Vuelo = v,
                Pasajeros = ReservasDe(v.IdVuelo)
                    .Where(r => r.EstadoReserva == "Confirmada")
                    .Select(r => new PasajeroVuelo { Reserva = r, CheckIn = _checkIns.GetValueOrDefault(r.IdReserva) })
                    .ToList()
            }).ToList();

            _listaVuelos.Mostrar(resumen, idVuelo);
            if (resumen.Count == 0) MostrarVuelo(null);
        }

        // =====================================================================
        // Vuelo elegido (derecha)
        // =====================================================================
        private void MostrarVuelo(ResumenVuelo? resumen)
        {
            _vueloActual = resumen?.Vuelo;
            var v = _vueloActual;

            if (v == null)
            {
                _encabezado.Mostrar(null);
                _lblDatos.Text = "";
                _dgvPasajeros.Rows.Clear();
                ActualizarAcciones();
                return;
            }

            int confirmadas = Confirmadas(v.IdVuelo);
            int canceladas = ReservasDe(v.IdVuelo).Count(r => r.EstadoReserva == "Cancelada");
            int ocupacion = (int)Math.Round(confirmadas * 100.0 / AsientosPorVuelo);

            if (_vista == Vista.Cancelados)
                _encabezado.Mostrar(resumen,
                    ("Reservas reembolsadas", canceladas, AppEstilos.ColorExito),
                    ("Pasajeros afectados", canceladas, AppEstilos.ColorAcento),
                    ("Asientos liberados", AsientosPorVuelo, AppEstilos.ColorTextoOscuro));
            else if (_vista == Vista.Anteriores)
                _encabezado.Mostrar(resumen,
                    ("Pasajes vendidos", confirmadas, AppEstilos.ColorTextoOscuro),
                    ("Volaron", Embarcados(v.IdVuelo), AppEstilos.ColorExito),
                    ("No se presentaron", confirmadas - Embarcados(v.IdVuelo), AppEstilos.ColorAcento),
                    ("Ocupación %", ocupacion, AppEstilos.ColorPrimario));
            else
                _encabezado.Mostrar(resumen,
                    ("Pasajes vendidos", confirmadas, AppEstilos.ColorTextoOscuro),
                    ("Ocupación %", ocupacion, AppEstilos.ColorPrimario),
                    ("Con check-in", ConCheckIn(v.IdVuelo), AppEstilos.ColorExito),
                    ("Asientos libres", AsientosPorVuelo - confirmadas, AppEstilos.ColorTextoOscuro));

            TimeSpan duracion = v.Llegada - v.Salida;
            string libre = v.EstadoVuelo == "Cancelado"
                ? "libre (el vuelo está cancelado)"
                : $"libre desde {v.Llegada.AddMinutes(MinutosEscala):dd/MM HH:mm} (llegada + {MinutosEscala} min de escala)";
            _lblDatos.Text =
                $"Aeronave {v.Aeronave}  ·  {libre}\n" +
                $"Sale {v.Salida:dddd dd/MM/yyyy HH:mm}  ·  Llega {v.Llegada:dd/MM HH:mm}  ·  Duración {(int)duracion.TotalHours} h {duracion.Minutes:00} min  ·  Tarifa {v.Tarifa:C0}";

            _dgvPasajeros.Rows.Clear();
            foreach (var r in ReservasDe(v.IdVuelo).OrderBy(r => r.EstadoReserva == "Confirmada" ? 0 : 1).ThenBy(r => r.Pasajero))
            {
                string checkIn = !_checkIns.TryGetValue(r.IdReserva, out var c) ? "—"
                    : c.EstadoCheckin == "Embarcado" ? "Embarcó" : "Sí";
                _dgvPasajeros.Rows.Add(r.CodigoReserva, r.Pasajero, r.Asiento, r.EstadoReserva, checkIn);
            }

            ActualizarAcciones();
        }

        private void ActualizarAcciones()
        {
            var v = _vueloActual;
            bool vigente = v != null && (v.EstadoVuelo == "Programado" || v.EstadoVuelo == "Confirmado") && v.Salida > DateTime.Now;
            _btnEditar.Enabled = vigente;
            _btnCancelar.Enabled = vigente;

            if (v == null)
            {
                _lblSeleccion.Text = "Elegí un vuelo de la lista.";
                _lblSeleccion.ForeColor = AppEstilos.ColorTextoSuave;
            }
            else if (vigente)
            {
                int confirmadas = Confirmadas(v.IdVuelo);
                _lblSeleccion.Text = confirmadas == 0
                    ? $"{v.NumeroVuelo} no tiene reservas: se puede editar o cancelar sin afectar pasajeros."
                    : $"Si cancelás {v.NumeroVuelo}, se reembolsa a sus {confirmadas} pasajeros y se liberan los asientos.";
                _lblSeleccion.ForeColor = AppEstilos.ColorTextoOscuro;
            }
            else
            {
                _lblSeleccion.Text = v.EstadoVuelo switch
                {
                    "Cancelado" => $"{v.NumeroVuelo} está cancelado: solo consulta.",
                    "Finalizado" => $"{v.NumeroVuelo} ya finalizó: solo consulta.",
                    _ => $"{v.NumeroVuelo} ya salió: no se puede editar ni cancelar."
                };
                _lblSeleccion.ForeColor = AppEstilos.ColorTextoSuave;
            }
        }

        // =====================================================================
        // Acciones
        // =====================================================================
        private async void btnNuevo_Click(object? sender, EventArgs e)
        {
            using var form = new frmVueloABM(_vueloClient, null);
            if (form.ShowDialog(this) == DialogResult.OK)
                await CargarDatosAsync();
        }

        private async void btnEditar_Click(object? sender, EventArgs e)
        {
            if (_vueloActual == null) return;
            using var form = new frmVueloABM(_vueloClient, _vueloActual);
            if (form.ShowDialog(this) == DialogResult.OK)
                await CargarDatosAsync();
        }

        private async void btnCancelar_Click(object? sender, EventArgs e)
        {
            var v = _vueloActual;
            if (v == null) return;

            int confirmadas = Confirmadas(v.IdVuelo);
            int conCheckIn = ConCheckIn(v.IdVuelo);

            string detalle = confirmadas == 0
                ? "El vuelo no tiene reservas."
                : $"Se van a cancelar {confirmadas} reservas y se les devolverá TODO lo pagado (pasaje, equipaje y excesos)." +
                  (conCheckIn > 0 ? $"\n{conCheckIn} pasajeros ya hicieron el check-in: también se les reembolsa." : "");

            var confirmacion = MessageBox.Show(
                $"¿Cancelar el vuelo {v.NumeroVuelo} ({v.LugarOrigen} → {v.LugarDestino}, {v.Salida:dd/MM HH:mm})?\n\n" +
                detalle +
                "\n\nLos asientos quedan libres y la aeronave queda disponible para otro vuelo. Esta acción no se puede deshacer.",
                "Cancelar vuelo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                _btnCancelar.Enabled = false;
                UseWaitCursor = true;
                var resultado = await _vueloClient.CancelarAsync(v.IdVuelo);
                UseWaitCursor = false;

                MessageBox.Show(resultado?.Mensaje ?? "Vuelo cancelado.", "Cancelar vuelo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Se muestra en la vista de cancelados
                CambiarVista(Vista.Cancelados);
                await CargarDatosAsync();
                CargarLista(v.IdVuelo);
            }
            catch (HttpRequestException ex)
            {
                UseWaitCursor = false;
                MensajesApi.MostrarError(ex);
                ActualizarAcciones();
            }
        }

        // =====================================================================
        // Armado de la pantalla
        // =====================================================================
        private void ConstruirPantalla()
        {
            Controls.Clear();
            BackColor = AppEstilos.ColorSecundario;
            Padding = new Padding(24, 20, 24, 20);
            Font = AppEstilos.FuenteNormal;

            // ---------- Izquierda ----------
            var tarjetaVuelos = new Panel { Dock = DockStyle.Left, Width = 360, BackColor = Color.White };
            tarjetaVuelos.Paint += DibujarBorde;

            var cabecera = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 285,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(16, 14, 16, 8)
            };

            var filaBotones = new FlowLayoutPanel { AutoSize = true, WrapContents = false, BackColor = Color.White, Margin = new Padding(0, 0, 0, 12) };
            var btnNuevo = new Button { Text = "+ Nuevo vuelo", Width = 206, Margin = new Padding(0, 0, 8, 0) };
            AppEstilos.EstilizarBotonPrimario(btnNuevo);
            btnNuevo.Click += btnNuevo_Click;
            _btnRefrescar = new Button { Text = "Refrescar", Width = 112, Margin = Padding.Empty };
            AppEstilos.EstilizarBotonSecundario(_btnRefrescar);
            filaBotones.Controls.Add(btnNuevo);
            filaBotones.Controls.Add(_btnRefrescar);

            var filaVistas = new FlowLayoutPanel { AutoSize = true, WrapContents = false, BackColor = Color.White, Margin = new Padding(0, 0, 0, 10) };
            _btnHoy = new Button { Text = "Hoy", Width = 58, Margin = new Padding(0, 0, 4, 0) };
            _btnProximos = new Button { Text = "Próximos", Width = 80, Margin = new Padding(0, 0, 4, 0) };
            _btnAnteriores = new Button { Text = "Anteriores", Width = 84, Margin = new Padding(0, 0, 4, 0) };
            _btnCancelados = new Button { Text = "Cancelados", Width = 86, Margin = Padding.Empty };
            _btnHoy.Click += (s, e) => CambiarVista(Vista.Hoy);
            _btnProximos.Click += (s, e) => CambiarVista(Vista.Proximos);
            _btnAnteriores.Click += (s, e) => CambiarVista(Vista.Anteriores);
            _btnCancelados.Click += (s, e) => CambiarVista(Vista.Cancelados);
            filaVistas.Controls.AddRange(new Control[] { _btnHoy, _btnProximos, _btnAnteriores, _btnCancelados });

            _txtBuscar = new TextBox { Width = 326, Font = AppEstilos.FuenteNormal, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "N° de vuelo o aeronave (ej.: SP312, LV-KAY)" };
            _txtBuscar.TextChanged += (s, e) => CargarLista(_vueloActual?.IdVuelo);

            _cmbEstado = Listados.Combo(150, new[] { "Programado", "Confirmado" }, () => CargarLista(_vueloActual?.IdVuelo));
            _dtpDia = Listados.Fecha(() => CargarLista(_vueloActual?.IdVuelo));
            _dtpDia.Width = 150;
            _cmbOrigen = Listados.Combo(150, Array.Empty<string>(), () => CargarLista(_vueloActual?.IdVuelo));
            _cmbDestino = Listados.Combo(150, Array.Empty<string>(), () => CargarLista(_vueloActual?.IdVuelo));

            var filaEstadoDia = new FlowLayoutPanel { AutoSize = true, WrapContents = false, BackColor = Color.White, Margin = new Padding(0, 8, 0, 0) };
            filaEstadoDia.Controls.Add(Listados.Campo("Estado del vuelo", _cmbEstado));
            filaEstadoDia.Controls.Add(Listados.Campo("Día de salida", _dtpDia));
            var filaRuta = new FlowLayoutPanel { AutoSize = true, WrapContents = false, BackColor = Color.White, Margin = new Padding(0, 8, 0, 0) };
            filaRuta.Controls.Add(Listados.Campo("Origen", _cmbOrigen));
            filaRuta.Controls.Add(Listados.Campo("Destino", _cmbDestino));

            _btnRefrescar.Click += async (s, e) =>
            {
                _txtBuscar.Clear();
                _cmbEstado.SelectedIndex = 0;
                _cmbOrigen.SelectedIndex = 0;
                _cmbDestino.SelectedIndex = 0;
                _dtpDia.Checked = false;
                await CargarDatosAsync();
            };

            cabecera.Controls.Add(filaBotones);
            cabecera.Controls.Add(filaVistas);
            cabecera.Controls.Add(Listados.Campo("Buscar", _txtBuscar));
            cabecera.Controls.Add(filaEstadoDia);
            cabecera.Controls.Add(filaRuta);

            _listaVuelos = new ucListaVuelos { Dock = DockStyle.Fill };
            _listaVuelos.VueloSeleccionado += (s, vuelo) => MostrarVuelo(vuelo);

            tarjetaVuelos.Controls.Add(_listaVuelos);
            tarjetaVuelos.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = AppEstilos.ColorBorde });
            tarjetaVuelos.Controls.Add(cabecera);
            _listaVuelos.BringToFront();

            // ---------- Derecha ----------
            var derecha = new Panel { Dock = DockStyle.Fill, BackColor = AppEstilos.ColorSecundario };
            _encabezado = new EncabezadoVuelo { Dock = DockStyle.Top };

            var tarjetaDatos = new Panel { Dock = DockStyle.Top, Height = 66, BackColor = Color.White, Padding = new Padding(20, 10, 20, 10) };
            tarjetaDatos.Paint += DibujarBorde;
            _lblDatos = new Label { Dock = DockStyle.Fill, Font = AppEstilos.FuenteNormal, ForeColor = AppEstilos.ColorTextoOscuro, AutoEllipsis = true };
            tarjetaDatos.Controls.Add(_lblDatos);

            var tarjetaLista = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16, 12, 16, 12) };
            tarjetaLista.Paint += DibujarBorde;
            var lblLista = new Label { Text = "Pasajeros del vuelo", Dock = DockStyle.Top, Height = 32, Font = AppEstilos.FuenteSubtitulo, ForeColor = AppEstilos.ColorTextoOscuro };
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
            _dgvPasajeros.Columns.Add("CheckIn", "Check-in");
            _dgvPasajeros.Columns["Pasajero"]!.FillWeight = 200;
            _dgvPasajeros.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.Value is not string valor) return;
                string col = _dgvPasajeros.Columns[e.ColumnIndex].Name;
                if (col != "Estado" && col != "CheckIn") return;
                Color? color = valor switch
                {
                    "Confirmada" or "Embarcó" or "Sí" => AppEstilos.ColorExito,
                    "Cancelada" => AppEstilos.ColorAcento,
                    _ => null
                };
                if (color == null) return;
                e.CellStyle!.ForeColor = color.Value;
                e.CellStyle.Font = AppEstilos.FuenteBoton;
            };
            tarjetaLista.Controls.Add(_dgvPasajeros);
            tarjetaLista.Controls.Add(lblLista);
            _dgvPasajeros.BringToFront();

            var barraAcciones = new Panel { Dock = DockStyle.Bottom, Height = 72, BackColor = Color.White, Padding = new Padding(20, 14, 20, 14) };
            barraAcciones.Paint += DibujarBorde;
            _btnCancelar = new Button { Text = "Cancelar vuelo", Dock = DockStyle.Right, Width = 170, Enabled = false };
            AppEstilos.EstilizarBotonPrimario(_btnCancelar);
            AppEstilos.CambiarColorBoton(_btnCancelar, AppEstilos.ColorError);
            _btnCancelar.Click += btnCancelar_Click;
            _btnEditar = new Button { Text = "Editar vuelo", Dock = DockStyle.Right, Width = 140, Enabled = false };
            AppEstilos.EstilizarBotonSecundario(_btnEditar);
            _btnEditar.Click += btnEditar_Click;
            _lblSeleccion = new Label { Dock = DockStyle.Fill, Font = AppEstilos.FuenteBoton, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
            barraAcciones.Controls.Add(_lblSeleccion);
            barraAcciones.Controls.Add(_btnEditar);
            barraAcciones.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 10, BackColor = Color.White });
            barraAcciones.Controls.Add(_btnCancelar);
            _lblSeleccion.BringToFront();

            derecha.Controls.Add(tarjetaLista);
            derecha.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 14, BackColor = AppEstilos.ColorSecundario });
            derecha.Controls.Add(tarjetaDatos);
            derecha.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10, BackColor = AppEstilos.ColorSecundario });
            derecha.Controls.Add(_encabezado);
            derecha.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 14, BackColor = AppEstilos.ColorSecundario });
            derecha.Controls.Add(barraAcciones);

            Controls.Add(derecha);
            Controls.Add(new Panel { Dock = DockStyle.Left, Width = 16, BackColor = AppEstilos.ColorSecundario });
            Controls.Add(tarjetaVuelos);

            CambiarVista(Vista.Hoy);
        }

        private static void DibujarBorde(object? sender, PaintEventArgs e)
        {
            if (sender is not Control c) return;
            using var lapiz = new Pen(AppEstilos.ColorBorde);
            e.Graphics.DrawRectangle(lapiz, 0, 0, c.Width - 1, c.Height - 1);
        }
    }
}
