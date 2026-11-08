// SkyPlus.Desktop/frmReservas.cs
// Reservas por vuelo (Agente de Reservas):
//   1. Arriba se filtra por origen, destino y vuelo.
//   2. En la lista aparecen las reservas de esos vuelos, con lo que pagó cada pasajero.
//   3. "Nueva reserva" abre la venta (asiento → equipaje → pago).
//   4. "Cancelar y reembolsar" devuelve todo lo pagado y cancela la reserva en un solo paso:
//      primero se reembolsan los pagos adicionales y al final el pago inicial,
//      que es el que cancela la reserva y libera el asiento (regla de la API).
//      Solo se puede si el pasajero NO hizo check-in y el vuelo todavía no salió.

using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Utilidades;

namespace SkyPlus.Desktop
{
    public partial class frmReservas : Form
    {
        private readonly ReservaClient _reservaClient = new ReservaClient();
        private readonly VueloClient _vueloClient = new VueloClient();
        private readonly PagoClient _pagoClient = new PagoClient();
        private readonly ReembolsoClient _reembolsoClient = new ReembolsoClient();
        private readonly CheckInClient _checkInClient = new CheckInClient();

        private List<VueloResponse> _vuelos = new();
        private List<ReservaResponse> _reservas = new();
        private List<PagoResponse> _pagos = new();
        private HashSet<int> _pagosReembolsados = new();
        private HashSet<int> _reservasConCheckIn = new();
        private Dictionary<int, VueloResponse> _vuelosPorId = new();
        private bool _cargandoFiltros;

        private ComboBox _cmbOrigen = null!;
        private ComboBox _cmbDestino = null!;
        private ComboBox _cmbVuelo = null!;
        private ComboBox _cmbEstado = null!;
        private TextBox _txtBuscar = null!;
        private Label _lblResumen = null!;
        private DataGridView _dgv = null!;
        private Label _lblSeleccion = null!;
        private Button _btnCancelar = null!;
        private Button _btnNueva = null!;
        private Button _btnRefrescar = null!;

        private class Opcion
        {
            public int Id { get; init; }
            public string Texto { get; init; } = string.Empty;
            public override string ToString() => Texto;
        }

        public frmReservas()
        {
            InitializeComponent();
        }

        private async void frmReservas_Load(object sender, EventArgs e)
        {
            ConstruirPantalla();
            await CargarDatosAsync();
        }

        // =====================================================================
        // Datos
        // =====================================================================
        private async Task CargarDatosAsync()
        {
            try
            {
                _btnRefrescar.Enabled = false;

                var tVuelos = _vueloClient.ObtenerTodosAsync();
                var tReservas = _reservaClient.ObtenerTodosAsync();
                var tPagos = _pagoClient.ObtenerTodosAsync();
                var tReembolsos = _reembolsoClient.ObtenerTodosAsync();
                var tCheckIns = _checkInClient.ObtenerTodosAsync();
                await Task.WhenAll(tVuelos, tReservas, tPagos, tReembolsos, tCheckIns);

                // Vuelos desde hoy (también los cancelados, para poder reembolsar a sus pasajeros)
                _vuelos = (tVuelos.Result ?? new List<VueloResponse>())
                    .Where(v => v.Salida.Date >= DateTime.Today && v.EstadoVuelo != "Finalizado")
                    .OrderBy(v => v.Salida)
                    .ToList();
                _vuelosPorId = _vuelos.ToDictionary(v => v.IdVuelo);
                _reservas = tReservas.Result ?? new List<ReservaResponse>();
                _reservasConCheckIn = (tCheckIns.Result ?? new List<CheckInResponse>())
                    .Select(c => c.IdReserva)
                    .ToHashSet();
                _pagos = tPagos.Result ?? new List<PagoResponse>();
                _pagosReembolsados = (tReembolsos.Result ?? new List<ReembolsoResponse>())
                    .Select(r => r.IdPago)
                    .ToHashSet();

                CargarOrigenes();
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

        // Pagos de la reserva que todavía se pueden reembolsar
        private List<PagoResponse> PagosReembolsables(int idReserva) =>
            _pagos.Where(p => p.IdReserva == idReserva &&
                              p.EstadoPago == "Registrado" &&
                              !_pagosReembolsados.Contains(p.IdPago))
                  .ToList();

        // Reglas para cancelar y reembolsar (las mismas que valida la API).
        // Devuelve null si se puede; si no, el motivo para mostrarle al agente.
        private string? MotivoNoReembolsable(ReservaResponse r)
        {
            if (r.EstadoReserva != "Confirmada")
                return $"La reserva está {NombreEstado(r.EstadoReserva).ToLower()}.";

            if (_reservasConCheckIn.Contains(r.IdReserva))
                return "El pasajero ya hizo el check-in: no se puede cancelar ni reembolsar.";

            if (_vuelosPorId.TryGetValue(r.IdVuelo, out var vuelo) &&
                vuelo.EstadoVuelo != "Cancelado" && vuelo.Salida <= DateTime.Now)
                return "El vuelo ya salió: no se puede cancelar ni reembolsar.";

            return null;
        }

        // =====================================================================
        // Filtros: origen → destino → vuelo
        // =====================================================================
        private void CargarOrigenes()
        {
            int? origenAnterior = (_cmbOrigen.SelectedItem as Opcion)?.Id;
            var opciones = new List<Opcion> { new Opcion { Id = 0, Texto = "Todos los orígenes" } };
            opciones.AddRange(_vuelos
                .GroupBy(v => v.IdLugarOrigen)
                .Select(g => new Opcion { Id = g.Key, Texto = g.First().LugarOrigen })
                .OrderBy(o => o.Texto));

            LlenarCombo(_cmbOrigen, opciones, origenAnterior);
            CargarDestinos();
        }

        private void CargarDestinos()
        {
            int idOrigen = (_cmbOrigen.SelectedItem as Opcion)?.Id ?? 0;
            int? destinoAnterior = (_cmbDestino.SelectedItem as Opcion)?.Id;

            var opciones = new List<Opcion> { new Opcion { Id = 0, Texto = "Todos los destinos" } };
            opciones.AddRange(_vuelos
                .Where(v => idOrigen == 0 || v.IdLugarOrigen == idOrigen)
                .GroupBy(v => v.IdLugarDestino)
                .Select(g => new Opcion { Id = g.Key, Texto = g.First().LugarDestino })
                .OrderBy(o => o.Texto));

            LlenarCombo(_cmbDestino, opciones, destinoAnterior);
            CargarVuelos();
        }

        private void CargarVuelos()
        {
            int idOrigen = (_cmbOrigen.SelectedItem as Opcion)?.Id ?? 0;
            int idDestino = (_cmbDestino.SelectedItem as Opcion)?.Id ?? 0;
            int? vueloAnterior = (_cmbVuelo.SelectedItem as Opcion)?.Id;

            var opciones = new List<Opcion> { new Opcion { Id = 0, Texto = "Todos los vuelos" } };
            opciones.AddRange(VuelosFiltrados(idOrigen, idDestino, 0)
                .Select(v => new Opcion
                {
                    Id = v.IdVuelo,
                    Texto = $"{v.NumeroVuelo} · {v.Salida:dd/MM HH:mm}" + (v.EstadoVuelo == "Cancelado" ? " · CANCELADO" : "")
                }));

            LlenarCombo(_cmbVuelo, opciones, vueloAnterior);
            LlenarGrilla();
        }

        private IEnumerable<VueloResponse> VuelosFiltrados(int idOrigen, int idDestino, int idVuelo) =>
            _vuelos.Where(v =>
                (idOrigen == 0 || v.IdLugarOrigen == idOrigen) &&
                (idDestino == 0 || v.IdLugarDestino == idDestino) &&
                (idVuelo == 0 || v.IdVuelo == idVuelo));

        private void LlenarCombo(ComboBox combo, List<Opcion> opciones, int? idAnterior)
        {
            _cargandoFiltros = true;
            combo.DataSource = opciones;
            int indice = idAnterior == null ? 0 : opciones.FindIndex(o => o.Id == idAnterior);
            combo.SelectedIndex = indice < 0 ? 0 : indice;
            _cargandoFiltros = false;
        }

        // =====================================================================
        // Lista de reservas
        // =====================================================================
        private void LlenarGrilla()
        {
            int idOrigen = (_cmbOrigen.SelectedItem as Opcion)?.Id ?? 0;
            int idDestino = (_cmbDestino.SelectedItem as Opcion)?.Id ?? 0;
            int idVuelo = (_cmbVuelo.SelectedItem as Opcion)?.Id ?? 0;
            string estado = _cmbEstado.SelectedItem as string ?? "Todas";
            string texto = _txtBuscar.Text.Trim();

            var vuelos = VuelosFiltrados(idOrigen, idDestino, idVuelo).ToDictionary(v => v.IdVuelo);

            var lista = _reservas
                .Where(r => vuelos.ContainsKey(r.IdVuelo))
                .Where(r => estado == "Todas" || NombreEstado(r.EstadoReserva) == estado)
                .Where(r => texto == "" ||
                            r.CodigoReserva.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                            r.Pasajero.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                            r.Asiento.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => vuelos[r.IdVuelo].Salida)
                .ThenBy(r => r.Pasajero)
                .ToList();

            _dgv.Rows.Clear();
            foreach (var r in lista)
            {
                var v = vuelos[r.IdVuelo];
                decimal pagado = PagosReembolsables(r.IdReserva).Sum(p => p.Monto);
                int fila = _dgv.Rows.Add(
                    r.CodigoReserva,
                    r.Pasajero,
                    $"{v.NumeroVuelo} · {v.Salida:dd/MM HH:mm}",
                    r.Asiento,
                    NombreEstado(r.EstadoReserva),
                    _reservasConCheckIn.Contains(r.IdReserva) ? "Sí" : "—",
                    pagado > 0 ? pagado.ToString("C0") : "—");
                _dgv.Rows[fila].Tag = r;
            }

            int confirmadas = lista.Count(r => r.EstadoReserva == "Confirmada");
            int canceladas = lista.Count(r => r.EstadoReserva == "Cancelada");
            _lblResumen.Text = idVuelo != 0 && vuelos.TryGetValue(idVuelo, out var vuelo)
                ? $"{vuelo.NumeroVuelo} · {vuelo.LugarOrigen} → {vuelo.LugarDestino} · Sale {vuelo.Salida:dddd dd/MM HH:mm} · {vuelo.EstadoVuelo}   |   " +
                  $"{confirmadas} confirmadas · {canceladas} canceladas · {180 - confirmadas} asientos libres"
                : $"{vuelos.Count} vuelos   |   {lista.Count} reservas · {confirmadas} confirmadas · {canceladas} canceladas";

            ActualizarAcciones();
        }

        private ReservaResponse? Seleccionada() => _dgv?.CurrentRow?.Tag as ReservaResponse;

        private void ActualizarAcciones()
        {
            var r = Seleccionada();
            string? motivo = r == null ? null : MotivoNoReembolsable(r);
            _btnCancelar.Enabled = r != null && motivo == null;

            if (r == null)
            {
                _lblSeleccion.Text = "Elegí una reserva de la lista.";
                _lblSeleccion.ForeColor = AppEstilos.ColorTextoSuave;
                return;
            }

            decimal aDevolver = PagosReembolsables(r.IdReserva).Sum(p => p.Monto);
            if (motivo == null)
            {
                _lblSeleccion.Text = $"{r.Pasajero} · {r.CodigoReserva} · asiento {r.Asiento} — pagó {aDevolver:C0}";
                _lblSeleccion.ForeColor = AppEstilos.ColorTextoOscuro;
            }
            else
            {
                _lblSeleccion.Text = $"{r.Pasajero} · {r.CodigoReserva} — {motivo}";
                _lblSeleccion.ForeColor = r.EstadoReserva == "Confirmada" ? AppEstilos.ColorAcento : AppEstilos.ColorTextoSuave;
            }
        }

        // =====================================================================
        // Acciones
        // =====================================================================
        private async void btnNueva_Click(object? sender, EventArgs e)
        {
            using var formulario = new frmReservaABM();
            if (formulario.ShowDialog(this) == DialogResult.OK)
                await CargarDatosAsync();
        }

        private async void btnCancelar_Click(object? sender, EventArgs e)
        {
            var r = Seleccionada();
            if (r == null || MotivoNoReembolsable(r) != null) return;

            var pagos = PagosReembolsables(r.IdReserva)
                .OrderBy(p => p.TipoPago == "Inicial" ? 1 : 0) // primero los adicionales
                .ThenBy(p => p.IdPago)
                .ToList();
            decimal total = pagos.Sum(p => p.Monto);

            string? motivo = PedirMotivo(r, total);
            if (motivo == null) return;

            try
            {
                _btnCancelar.Enabled = false;
                UseWaitCursor = true;

                if (pagos.Count == 0)
                {
                    // Sin pagos (no debería pasar con la venta nueva): cancelación directa
                    await _reservaClient.CancelarAsync(r.IdReserva);
                }
                else
                {
                    foreach (var pago in pagos)
                        await _reembolsoClient.CrearAsync(pago.IdPago, SesionUsuario.IdUsuario, motivo);
                }

                UseWaitCursor = false;
                MessageBox.Show(
                    $"Reserva {r.CodigoReserva} cancelada.\nSe reembolsaron {total:C0} a {r.Pasajero}.\nEl asiento {r.Asiento} quedó libre.",
                    "Cancelar y reembolsar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                UseWaitCursor = false;
                MensajesApi.MostrarError(ex);
            }
            finally
            {
                UseWaitCursor = false;
                await CargarDatosAsync();
            }
        }

        // Ventanita para confirmar y escribir el motivo. Devuelve null si se cancela.
        private string? PedirMotivo(ReservaResponse r, decimal total)
        {
            using var dialogo = new Form
            {
                Text = "Cancelar y reembolsar",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(480, 300),
                BackColor = Color.White,
                Font = AppEstilos.FuenteNormal
            };

            var lblTitulo = new Label { Text = $"Cancelar la reserva {r.CodigoReserva}", Font = AppEstilos.FuenteSubtitulo, ForeColor = AppEstilos.ColorTextoOscuro, AutoSize = true, Location = new Point(24, 20) };
            var lblDetalle = new Label
            {
                Text = $"{r.Pasajero} · asiento {r.Asiento}\nSe devuelve todo lo pagado: {total:C0}\nEl asiento queda libre para otra venta.",
                ForeColor = AppEstilos.ColorTextoSuave,
                AutoSize = true,
                Location = new Point(24, 52)
            };
            var lblMotivo = new Label { Text = "Motivo", Font = AppEstilos.FuenteBoton, AutoSize = true, Location = new Point(24, 126) };
            var cmbMotivo = new ComboBox
            {
                Location = new Point(24, 150),
                Width = 430,
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = AppEstilos.FuenteNormal
            };
            cmbMotivo.Items.AddRange(new object[]
            {
                "Cambio de planes del pasajero",
                "Problemas de salud",
                "Motivos laborales",
                "Cancelación del vuelo por la aerolínea",
                "Error en la emisión"
            });

            var lblError = new Label { ForeColor = AppEstilos.ColorError, AutoSize = true, Location = new Point(24, 182), Visible = false, Text = "Escribí o elegí un motivo." };

            var btnVolver = new Button { Text = "Volver", Size = new Size(110, 40), Location = new Point(220, 234), DialogResult = DialogResult.Cancel };
            var btnConfirmar = new Button { Text = $"Reembolsar {total:C0}", Size = new Size(124, 40), Location = new Point(338, 234) };
            btnConfirmar.Width = Math.Max(124, TextRenderer.MeasureText(btnConfirmar.Text, AppEstilos.FuenteBoton).Width + 30);
            btnConfirmar.Left = 456 - btnConfirmar.Width;
            btnVolver.Left = btnConfirmar.Left - btnVolver.Width - 10;
            AppEstilos.EstilizarBotonSecundario(btnVolver);
            AppEstilos.EstilizarBotonPrimario(btnConfirmar);
            AppEstilos.CambiarColorBoton(btnConfirmar, AppEstilos.ColorError);
            btnConfirmar.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(cmbMotivo.Text))
                {
                    lblError.Visible = true;
                    return;
                }
                dialogo.DialogResult = DialogResult.OK;
            };

            dialogo.Controls.AddRange(new Control[] { lblTitulo, lblDetalle, lblMotivo, cmbMotivo, lblError, btnVolver, btnConfirmar });
            dialogo.CancelButton = btnVolver;

            return dialogo.ShowDialog(this) == DialogResult.OK ? cmbMotivo.Text.Trim() : null;
        }

        // =====================================================================
        // Armado de la pantalla
        // =====================================================================
        private void ConstruirPantalla()
        {
            BackColor = AppEstilos.ColorSecundario;
            Padding = new Padding(24, 20, 24, 20);
            Font = AppEstilos.FuenteNormal;

            // ----- Filtros -----
            var tarjetaFiltros = new Panel { Dock = DockStyle.Top, Height = 132, BackColor = Color.White, Padding = new Padding(20, 14, 20, 12) };
            tarjetaFiltros.Paint += DibujarBorde;

            var filaFiltros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, WrapContents = false };
            _cmbOrigen = CrearComboFiltro(240);
            _cmbDestino = CrearComboFiltro(240);
            _cmbVuelo = CrearComboFiltro(250);
            _cmbOrigen.SelectedIndexChanged += (s, e) => { if (!_cargandoFiltros) CargarDestinos(); };
            _cmbDestino.SelectedIndexChanged += (s, e) => { if (!_cargandoFiltros) CargarVuelos(); };
            _cmbVuelo.SelectedIndexChanged += (s, e) => { if (!_cargandoFiltros) LlenarGrilla(); };
            filaFiltros.Controls.Add(CrearCampo("Origen", _cmbOrigen));
            filaFiltros.Controls.Add(CrearCampo("Destino", _cmbDestino));
            filaFiltros.Controls.Add(CrearCampo("Vuelo", _cmbVuelo));

            _btnNueva = new Button { Text = "+ Nueva reserva", Width = 170, Dock = DockStyle.Right };
            AppEstilos.EstilizarBotonPrimario(_btnNueva);
            _btnNueva.Height = 40;
            _btnNueva.Click += btnNueva_Click;
            _btnRefrescar = new Button { Text = "Refrescar", Width = 100, Dock = DockStyle.Right };
            AppEstilos.EstilizarBotonSecundario(_btnRefrescar);
            _btnRefrescar.Height = 40;
            _btnRefrescar.Click += async (s, e) => await CargarDatosAsync();

            var botonesArriba = new Panel { Dock = DockStyle.Right, Width = 290, BackColor = Color.White, Padding = new Padding(0, 18, 0, 2) };
            botonesArriba.Controls.Add(_btnRefrescar);
            botonesArriba.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 10, BackColor = Color.White });
            botonesArriba.Controls.Add(_btnNueva);

            var arriba = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            arriba.Controls.Add(filaFiltros);
            arriba.Controls.Add(botonesArriba);
            filaFiltros.Dock = DockStyle.Fill;
            filaFiltros.BringToFront();

            _lblResumen = new Label { Dock = DockStyle.Bottom, Height = 34, Font = AppEstilos.FuenteBoton, ForeColor = AppEstilos.ColorTextoOscuro, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
            tarjetaFiltros.Controls.Add(arriba);
            tarjetaFiltros.Controls.Add(_lblResumen);

            // ----- Lista -----
            var tarjetaLista = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16, 12, 16, 12) };
            tarjetaLista.Paint += DibujarBorde;

            var barra = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, BackColor = Color.White, WrapContents = false };
            var lblLista = new Label { Text = "Reservas", Font = AppEstilos.FuenteSubtitulo, ForeColor = AppEstilos.ColorTextoOscuro, AutoSize = true, Margin = new Padding(0, 6, 24, 0) };
            _txtBuscar = new TextBox { Width = 300, Font = AppEstilos.FuenteNormal, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Buscar por pasajero, código o asiento", Margin = new Padding(0, 3, 12, 0) };
            _txtBuscar.TextChanged += (s, e) => LlenarGrilla();
            _cmbEstado = new ComboBox { Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Font = AppEstilos.FuenteNormal, Margin = new Padding(0, 3, 0, 0) };
            _cmbEstado.Items.AddRange(new object[] { "Todas", "Confirmada", "Cancelada", "Expirada", "Pendiente de pago" });
            _cmbEstado.SelectedIndex = 0;
            _cmbEstado.SelectedIndexChanged += (s, e) => LlenarGrilla();
            barra.Controls.AddRange(new Control[] { lblLista, _txtBuscar, _cmbEstado });

            _dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppEstilos.EstilizarGrid(_dgv);
            _dgv.Columns.Add("Codigo", "Reserva");
            _dgv.Columns.Add("Pasajero", "Pasajero");
            _dgv.Columns.Add("Vuelo", "Vuelo");
            _dgv.Columns.Add("Asiento", "Asiento");
            _dgv.Columns.Add("Estado", "Estado");
            _dgv.Columns.Add("CheckIn", "Check-in");
            _dgv.Columns.Add("Pagado", "Pagado");
            _dgv.Columns["Pasajero"]!.FillWeight = 180;
            _dgv.Columns["Vuelo"]!.FillWeight = 140;
            _dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || _dgv.Columns[e.ColumnIndex].Name != "Estado" || e.Value is not string estado)
                    return;
                e.CellStyle!.ForeColor = estado switch
                {
                    "Confirmada" => AppEstilos.ColorExito,
                    "Cancelada" => AppEstilos.ColorAcento,
                    _ => AppEstilos.ColorTextoSuave
                };
                e.CellStyle.Font = AppEstilos.FuenteBoton;
            };
            _dgv.SelectionChanged += (s, e) => ActualizarAcciones();

            tarjetaLista.Controls.Add(_dgv);
            tarjetaLista.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.White });
            tarjetaLista.Controls.Add(barra);
            _dgv.BringToFront();

            // ----- Acciones -----
            var barraAcciones = new Panel { Dock = DockStyle.Bottom, Height = 72, BackColor = Color.White, Padding = new Padding(20, 14, 20, 14) };
            barraAcciones.Paint += DibujarBorde;
            _btnCancelar = new Button { Text = "Cancelar y reembolsar", Dock = DockStyle.Right, Width = 220, Enabled = false };
            AppEstilos.EstilizarBotonPrimario(_btnCancelar);
            AppEstilos.CambiarColorBoton(_btnCancelar, AppEstilos.ColorError);
            _btnCancelar.Click += btnCancelar_Click;
            _lblSeleccion = new Label { Dock = DockStyle.Fill, Font = AppEstilos.FuenteBoton, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
            barraAcciones.Controls.Add(_lblSeleccion);
            barraAcciones.Controls.Add(_btnCancelar);
            _lblSeleccion.BringToFront();

            Controls.Add(tarjetaLista);
            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 14, BackColor = AppEstilos.ColorSecundario });
            Controls.Add(tarjetaFiltros);
            Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 14, BackColor = AppEstilos.ColorSecundario });
            Controls.Add(barraAcciones);
        }

        private static ComboBox CrearComboFiltro(int ancho) => new ComboBox
        {
            Width = ancho,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            Font = AppEstilos.FuenteNormal,
            Margin = Padding.Empty
        };

        private static Control CrearCampo(string titulo, Control control)
        {
            var campo = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = Color.White, Margin = new Padding(0, 0, 16, 0) };
            campo.Controls.Add(new Label { Text = titulo, AutoSize = true, Font = AppEstilos.FuenteBoton, ForeColor = AppEstilos.ColorTextoSuave, Margin = new Padding(0, 0, 0, 4) });
            campo.Controls.Add(control);
            return campo;
        }

        private static string NombreEstado(string estado) => estado == "PendientePago" ? "Pendiente de pago" : estado;

        private static void DibujarBorde(object? sender, PaintEventArgs e)
        {
            if (sender is not Control c) return;
            using var lapiz = new Pen(AppEstilos.ColorBorde);
            e.Graphics.DrawRectangle(lapiz, 0, 0, c.Width - 1, c.Height - 1);
        }
    }
}
