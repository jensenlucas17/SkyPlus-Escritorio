// SkyPlus.Desktop/frmReservas.cs
// Reservas organizadas por vuelo (Agente de Reservas):
//   1. Primero se elige qué vuelos ver: Próximos, Finalizados o Cancelados
//      (y opcionalmente se filtra por origen y destino).
//   2. Se elige el vuelo: a la derecha se ven sus números y SUS reservas.
//   3. Próximos y cancelados: "Cancelar y reembolsar" (si el pasajero no hizo check-in
//      y el vuelo no salió). Finalizados: solo consulta, con el resultado de cada
//      reserva (voló / no se presentó / cancelada).
// Cada vuelo guarda su información: las reservas de vuelos terminados quedan en
// "Finalizados" y no se mezclan con las de los vuelos que todavía no salieron.

using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Utilidades;

namespace SkyPlus.Desktop
{
    public partial class frmReservas : Form
    {
        private const int AsientosPorVuelo = 180;

        private enum Vista { Proximos, Finalizados, Cancelados }

        private readonly ReservaClient _reservaClient = new ReservaClient();
        private readonly VueloClient _vueloClient = new VueloClient();
        private readonly PagoClient _pagoClient = new PagoClient();
        private readonly ReembolsoClient _reembolsoClient = new ReembolsoClient();
        private readonly CheckInClient _checkInClient = new CheckInClient();

        private List<VueloResponse> _vuelos = new();
        private List<ReservaResponse> _reservas = new();
        private List<PagoResponse> _pagos = new();
        private HashSet<int> _pagosReembolsados = new();
        private Dictionary<int, CheckInResponse> _checkIns = new();

        private Vista _vista = Vista.Proximos;
        private VueloResponse? _vueloActual;
        private bool _cargandoFiltros;

        private Button _btnProximos = null!;
        private Button _btnFinalizados = null!;
        private Button _btnCancelados = null!;
        private ComboBox _cmbOrigen = null!;
        private ComboBox _cmbDestino = null!;
        private ucListaVuelos _listaVuelos = null!;
        private EncabezadoVuelo _encabezado = null!;
        private TextBox _txtBuscar = null!;
        private ComboBox _cmbEstado = null!;
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
            int? idVuelo = _vueloActual?.IdVuelo;

            try
            {
                _btnRefrescar.Enabled = false;

                var tVuelos = _vueloClient.ObtenerTodosAsync();
                var tReservas = _reservaClient.ObtenerTodosAsync();
                var tPagos = _pagoClient.ObtenerTodosAsync();
                var tReembolsos = _reembolsoClient.ObtenerTodosAsync();
                var tCheckIns = _checkInClient.ObtenerTodosAsync();
                await Task.WhenAll(tVuelos, tReservas, tPagos, tReembolsos, tCheckIns);

                _vuelos = tVuelos.Result ?? new List<VueloResponse>();
                // Las reservas expiradas nunca fueron ventas (no se pagaron): no se muestran
                _reservas = (tReservas.Result ?? new List<ReservaResponse>())
                    .Where(r => r.EstadoReserva != "Expirada" && r.EstadoReserva != "PendientePago")
                    .ToList();
                _pagos = tPagos.Result ?? new List<PagoResponse>();
                _pagosReembolsados = (tReembolsos.Result ?? new List<ReembolsoResponse>())
                    .Select(r => r.IdPago)
                    .ToHashSet();
                _checkIns = (tCheckIns.Result ?? new List<CheckInResponse>())
                    .GroupBy(c => c.IdReserva)
                    .ToDictionary(g => g.Key, g => g.First());

                CargarOrigenes(idVuelo);
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

        private static Vista VistaDe(VueloResponse v) => v.EstadoVuelo switch
        {
            "Finalizado" => Vista.Finalizados,
            "Cancelado" => Vista.Cancelados,
            _ => Vista.Proximos
        };

        private IEnumerable<VueloResponse> VuelosDeLaVista() => _vuelos.Where(v => VistaDe(v) == _vista);

        private List<ReservaResponse> ReservasDe(int idVuelo) =>
            _reservas.Where(r => r.IdVuelo == idVuelo).ToList();

        private List<PagoResponse> PagosReembolsables(int idReserva) =>
            _pagos.Where(p => p.IdReserva == idReserva &&
                              p.EstadoPago == "Registrado" &&
                              !_pagosReembolsados.Contains(p.IdPago))
                  .ToList();

        // Resultado de una reserva de un vuelo finalizado
        private string Resultado(ReservaResponse r)
        {
            if (r.EstadoReserva != "Confirmada") return NombreEstado(r.EstadoReserva);
            if (_checkIns.TryGetValue(r.IdReserva, out var c))
                return c.EstadoCheckin == "Embarcado" ? "Voló" : "Check-in sin embarque";
            return "No se presentó";
        }

        // Reglas para cancelar y reembolsar (las mismas que valida la API).
        // Devuelve null si se puede; si no, el motivo para mostrarle al agente.
        private string? MotivoNoReembolsable(ReservaResponse r)
        {
            if (_vueloActual?.EstadoVuelo == "Finalizado")
                return "Vuelo finalizado: solo consulta.";

            if (r.EstadoReserva != "Confirmada")
                return $"La reserva está {NombreEstado(r.EstadoReserva).ToLower()}.";

            if (_checkIns.ContainsKey(r.IdReserva))
                return "El pasajero ya hizo el check-in: no se puede cancelar ni reembolsar.";

            if (_vueloActual != null && _vueloActual.EstadoVuelo != "Cancelado" && _vueloActual.Salida <= DateTime.Now)
                return "El vuelo ya salió: no se puede cancelar ni reembolsar.";

            return null;
        }

        // =====================================================================
        // Columna izquierda: vista → origen → destino → vuelo
        // =====================================================================
        private void CambiarVista(Vista vista)
        {
            _vista = vista;
            foreach (var (boton, v) in new[] { (_btnProximos, Vista.Proximos), (_btnFinalizados, Vista.Finalizados), (_btnCancelados, Vista.Cancelados) })
            {
                if (v == vista) AppEstilos.EstilizarBotonPrimario(boton);
                else AppEstilos.EstilizarBotonSecundario(boton);
                boton.Height = 34;
            }

            _listaVuelos.MensajeVacio = vista switch
            {
                Vista.Finalizados => "Todavía no hay vuelos finalizados.",
                Vista.Cancelados => "No hay vuelos cancelados.",
                _ => "No hay vuelos próximos."
            };
            _listaVuelos.Avance = vista switch
            {
                Vista.Finalizados => r => ($"{ContarEmbarcados(r.Vuelo.IdVuelo)}/{r.Confirmadas} volaron", ContarEmbarcados(r.Vuelo.IdVuelo), r.Confirmadas),
                Vista.Cancelados => r =>
                {
                    int canceladas = ReservasDe(r.Vuelo.IdVuelo).Count(x => x.EstadoReserva == "Cancelada");
                    int pendientes = r.Confirmadas;
                    return (pendientes == 0 ? "todo reembolsado" : $"{pendientes} por reembolsar", canceladas, canceladas + pendientes);
                },
                _ => r => ($"{r.Confirmadas}/{AsientosPorVuelo} vendidos", r.Confirmadas, AsientosPorVuelo)
            };

            _cmbEstado.Items.Clear();
            _cmbEstado.Items.AddRange(vista == Vista.Finalizados
                ? new object[] { "Todas", "Voló", "No se presentó", "Cancelada" }
                : new object[] { "Todas", "Confirmada", "Cancelada" });
            _cmbEstado.SelectedIndex = 0;

            _vueloActual = null;
            CargarOrigenes(null);
        }

        private int ContarEmbarcados(int idVuelo) =>
            _reservas.Count(r => r.IdVuelo == idVuelo && r.EstadoReserva == "Confirmada" &&
                                 _checkIns.TryGetValue(r.IdReserva, out var c) && c.EstadoCheckin == "Embarcado");

        private void CargarOrigenes(int? idVuelo)
        {
            int? anterior = (_cmbOrigen.SelectedItem as Opcion)?.Id;
            var opciones = new List<Opcion> { new Opcion { Id = 0, Texto = "Todos los orígenes" } };
            opciones.AddRange(VuelosDeLaVista()
                .GroupBy(v => v.IdLugarOrigen)
                .Select(g => new Opcion { Id = g.Key, Texto = g.First().LugarOrigen })
                .OrderBy(o => o.Texto));
            LlenarCombo(_cmbOrigen, opciones, anterior);
            CargarDestinos(idVuelo);
        }

        private void CargarDestinos(int? idVuelo)
        {
            int idOrigen = (_cmbOrigen.SelectedItem as Opcion)?.Id ?? 0;
            int? anterior = (_cmbDestino.SelectedItem as Opcion)?.Id;
            var opciones = new List<Opcion> { new Opcion { Id = 0, Texto = "Todos los destinos" } };
            opciones.AddRange(VuelosDeLaVista()
                .Where(v => idOrigen == 0 || v.IdLugarOrigen == idOrigen)
                .GroupBy(v => v.IdLugarDestino)
                .Select(g => new Opcion { Id = g.Key, Texto = g.First().LugarDestino })
                .OrderBy(o => o.Texto));
            LlenarCombo(_cmbDestino, opciones, anterior);
            CargarListaVuelos(idVuelo);
        }

        private void CargarListaVuelos(int? idVuelo)
        {
            int idOrigen = (_cmbOrigen.SelectedItem as Opcion)?.Id ?? 0;
            int idDestino = (_cmbDestino.SelectedItem as Opcion)?.Id ?? 0;

            var vuelos = VuelosDeLaVista()
                .Where(v => (idOrigen == 0 || v.IdLugarOrigen == idOrigen) &&
                            (idDestino == 0 || v.IdLugarDestino == idDestino));

            // Próximos: el que sale antes primero. Finalizados y cancelados: el más reciente primero.
            vuelos = _vista == Vista.Proximos ? vuelos.OrderBy(v => v.Salida) : vuelos.OrderByDescending(v => v.Salida);

            var resumen = vuelos.Select(v => new ResumenVuelo
            {
                Vuelo = v,
                Pasajeros = ReservasDe(v.IdVuelo)
                    .Where(r => r.EstadoReserva == "Confirmada")
                    .Select(r => new PasajeroVuelo { Reserva = r, CheckIn = _checkIns.GetValueOrDefault(r.IdReserva) })
                    .ToList()
            }).ToList();

            _listaVuelos.Mostrar(resumen, idVuelo);
            if (resumen.Count == 0)
                MostrarVuelo(null);
        }

        private void LlenarCombo(ComboBox combo, List<Opcion> opciones, int? idAnterior)
        {
            _cargandoFiltros = true;
            combo.DataSource = opciones;
            int indice = idAnterior == null ? 0 : opciones.FindIndex(o => o.Id == idAnterior);
            combo.SelectedIndex = indice < 0 ? 0 : indice;
            _cargandoFiltros = false;
        }

        // =====================================================================
        // Columna derecha: el vuelo elegido y sus reservas
        // =====================================================================
        private void MostrarVuelo(ResumenVuelo? resumen)
        {
            _vueloActual = resumen?.Vuelo;
            var reservas = resumen == null ? new List<ReservaResponse>() : ReservasDe(resumen.Vuelo.IdVuelo);
            int confirmadas = reservas.Count(r => r.EstadoReserva == "Confirmada");
            int canceladas = reservas.Count(r => r.EstadoReserva == "Cancelada");
            int conCheckIn = reservas.Count(r => r.EstadoReserva == "Confirmada" && _checkIns.ContainsKey(r.IdReserva));

            switch (_vista)
            {
                case Vista.Finalizados:
                    int volaron = resumen == null ? 0 : ContarEmbarcados(resumen.Vuelo.IdVuelo);
                    _encabezado.Mostrar(resumen,
                        ("Pasajes vendidos", confirmadas, AppEstilos.ColorTextoOscuro),
                        ("Volaron", volaron, AppEstilos.ColorExito),
                        ("No se presentaron", confirmadas - volaron, (confirmadas - volaron) > 0 ? AppEstilos.ColorAcento : AppEstilos.ColorTextoOscuro),
                        ("Canceladas", canceladas, AppEstilos.ColorTextoSuave));
                    break;

                case Vista.Cancelados:
                    _encabezado.Mostrar(resumen,
                        ("Por reembolsar", confirmadas, confirmadas > 0 ? AppEstilos.ColorAcento : AppEstilos.ColorTextoOscuro),
                        ("Reembolsadas", canceladas, AppEstilos.ColorExito),
                        ("Reservas totales", reservas.Count, AppEstilos.ColorTextoOscuro));
                    break;

                default:
                    _encabezado.Mostrar(resumen,
                        ("Confirmadas", confirmadas, AppEstilos.ColorTextoOscuro),
                        ("Con check-in", conCheckIn, AppEstilos.ColorExito),
                        ("Canceladas", canceladas, AppEstilos.ColorTextoSuave),
                        ("Asientos libres", resumen == null ? 0 : AsientosPorVuelo - confirmadas, AppEstilos.ColorPrimario));
                    break;
            }

            _dgv.Columns["Resultado"]!.HeaderText = _vista == Vista.Finalizados ? "Resultado" : "Check-in";
            LlenarGrilla();
        }

        private void LlenarGrilla()
        {
            _dgv.Rows.Clear();

            if (_vueloActual != null)
            {
                string filtro = _cmbEstado.SelectedItem as string ?? "Todas";
                string texto = _txtBuscar.Text.Trim();

                var lista = ReservasDe(_vueloActual.IdVuelo)
                    .Where(r => texto == "" ||
                                r.CodigoReserva.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                                r.Pasajero.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                                r.Asiento.Contains(texto, StringComparison.OrdinalIgnoreCase))
                    .Where(r => filtro == "Todas" ||
                                (_vista == Vista.Finalizados ? Resultado(r) : NombreEstado(r.EstadoReserva)) == filtro)
                    .OrderBy(r => r.EstadoReserva == "Confirmada" ? 0 : 1)
                    .ThenBy(r => r.Pasajero);

                foreach (var r in lista)
                {
                    string columnaResultado = _vista == Vista.Finalizados
                        ? Resultado(r)
                        : !_checkIns.TryGetValue(r.IdReserva, out var c) ? "—"
                        : c.EstadoCheckin == "Embarcado" ? "Embarcó" : "Sí";

                    decimal pagado = PagosReembolsables(r.IdReserva).Sum(p => p.Monto);
                    int fila = _dgv.Rows.Add(
                        r.CodigoReserva,
                        r.Pasajero,
                        r.Asiento,
                        NombreEstado(r.EstadoReserva),
                        columnaResultado,
                        pagado > 0 ? pagado.ToString("C0") : "—");
                    _dgv.Rows[fila].Tag = r;
                }
            }

            ActualizarAcciones();
        }

        private ReservaResponse? Seleccionada() => _dgv?.CurrentRow?.Tag as ReservaResponse;

        private void ActualizarAcciones()
        {
            _btnCancelar.Visible = _vista != Vista.Finalizados;

            var r = Seleccionada();
            string? motivo = r == null ? null : MotivoNoReembolsable(r);
            _btnCancelar.Enabled = r != null && motivo == null;

            if (r == null)
            {
                _lblSeleccion.Text = _vueloActual == null ? "Elegí un vuelo de la lista." : "Elegí una reserva de la lista.";
                _lblSeleccion.ForeColor = AppEstilos.ColorTextoSuave;
                return;
            }

            if (_vista == Vista.Finalizados)
            {
                _lblSeleccion.Text = $"{r.Pasajero} · {r.CodigoReserva} · asiento {r.Asiento} — {Resultado(r)}";
                _lblSeleccion.ForeColor = AppEstilos.ColorTextoOscuro;
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
                    await _reservaClient.CancelarAsync(r.IdReserva);
                else
                    foreach (var pago in pagos)
                        await _reembolsoClient.CrearAsync(pago.IdPago, SesionUsuario.IdUsuario, motivo);

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
            var cmbMotivo = new ComboBox { Location = new Point(24, 150), Width = 430, DropDownStyle = ComboBoxStyle.DropDown, Font = AppEstilos.FuenteNormal };
            cmbMotivo.Items.AddRange(new object[]
            {
                "Cambio de planes del pasajero",
                "Problemas de salud",
                "Motivos laborales",
                "Cancelación del vuelo por la aerolínea",
                "Error en la emisión"
            });
            if (_vueloActual?.EstadoVuelo == "Cancelado")
                cmbMotivo.Text = "Cancelación del vuelo por la aerolínea";

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

            // ---------- Columna izquierda: vuelos ----------
            var tarjetaVuelos = new Panel { Dock = DockStyle.Left, Width = 340, BackColor = Color.White };
            tarjetaVuelos.Paint += DibujarBorde;

            var cabecera = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 218,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(16, 14, 16, 8)
            };

            var filaBotones = new FlowLayoutPanel { AutoSize = true, WrapContents = false, BackColor = Color.White, Margin = new Padding(0, 0, 0, 12) };
            _btnNueva = new Button { Text = "+ Nueva reserva", Width = 196, Margin = new Padding(0, 0, 8, 0) };
            AppEstilos.EstilizarBotonPrimario(_btnNueva);
            _btnNueva.Click += btnNueva_Click;
            _btnRefrescar = new Button { Text = "Refrescar", Width = 100, Margin = Padding.Empty };
            AppEstilos.EstilizarBotonSecundario(_btnRefrescar);
            _btnRefrescar.Click += async (s, e) => await CargarDatosAsync();
            filaBotones.Controls.Add(_btnNueva);
            filaBotones.Controls.Add(_btnRefrescar);

            // Selector de vista: Próximos | Finalizados | Cancelados
            var filaVistas = new FlowLayoutPanel { AutoSize = true, WrapContents = false, BackColor = Color.White, Margin = new Padding(0, 0, 0, 10) };
            _btnProximos = new Button { Text = "Próximos", Width = 100, Margin = new Padding(0, 0, 4, 0) };
            _btnFinalizados = new Button { Text = "Finalizados", Width = 100, Margin = new Padding(0, 0, 4, 0) };
            _btnCancelados = new Button { Text = "Cancelados", Width = 100, Margin = Padding.Empty };
            _btnProximos.Click += (s, e) => CambiarVista(Vista.Proximos);
            _btnFinalizados.Click += (s, e) => CambiarVista(Vista.Finalizados);
            _btnCancelados.Click += (s, e) => CambiarVista(Vista.Cancelados);
            filaVistas.Controls.AddRange(new Control[] { _btnProximos, _btnFinalizados, _btnCancelados });

            _cmbOrigen = CrearComboFiltro();
            _cmbDestino = CrearComboFiltro();
            _cmbOrigen.SelectedIndexChanged += (s, e) => { if (!_cargandoFiltros) CargarDestinos(null); };
            _cmbDestino.SelectedIndexChanged += (s, e) => { if (!_cargandoFiltros) CargarListaVuelos(null); };

            cabecera.Controls.Add(filaBotones);
            cabecera.Controls.Add(filaVistas);
            cabecera.Controls.Add(_cmbOrigen);
            cabecera.Controls.Add(_cmbDestino);

            _listaVuelos = new ucListaVuelos { Dock = DockStyle.Fill };
            _listaVuelos.VueloSeleccionado += (s, vuelo) => MostrarVuelo(vuelo);

            var separador = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = AppEstilos.ColorBorde };
            tarjetaVuelos.Controls.Add(_listaVuelos);
            tarjetaVuelos.Controls.Add(separador);
            tarjetaVuelos.Controls.Add(cabecera);
            _listaVuelos.BringToFront();

            // ---------- Columna derecha ----------
            var derecha = new Panel { Dock = DockStyle.Fill, BackColor = AppEstilos.ColorSecundario };
            _encabezado = new EncabezadoVuelo { Dock = DockStyle.Top };

            var tarjetaLista = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16, 12, 16, 12) };
            tarjetaLista.Paint += DibujarBorde;

            var barra = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, BackColor = Color.White, WrapContents = false };
            var lblLista = new Label { Text = "Reservas del vuelo", Font = AppEstilos.FuenteSubtitulo, ForeColor = AppEstilos.ColorTextoOscuro, AutoSize = true, Margin = new Padding(0, 6, 24, 0) };
            _txtBuscar = new TextBox { Width = 280, Font = AppEstilos.FuenteNormal, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Buscar por pasajero, código o asiento", Margin = new Padding(0, 3, 12, 0) };
            _txtBuscar.TextChanged += (s, e) => LlenarGrilla();
            _cmbEstado = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Font = AppEstilos.FuenteNormal, Margin = new Padding(0, 3, 0, 0) };
            _cmbEstado.SelectedIndexChanged += (s, e) => { if (_dgv != null) LlenarGrilla(); };
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
            _dgv.Columns.Add("Asiento", "Asiento");
            _dgv.Columns.Add("Estado", "Estado");
            _dgv.Columns.Add("Resultado", "Check-in");
            _dgv.Columns.Add("Pagado", "Pagado");
            _dgv.Columns["Pasajero"]!.FillWeight = 190;
            _dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.Value is not string valor) return;
                string columna = _dgv.Columns[e.ColumnIndex].Name;
                if (columna != "Estado" && columna != "Resultado") return;

                Color? color = valor switch
                {
                    "Confirmada" or "Voló" or "Embarcó" or "Sí" => AppEstilos.ColorExito,
                    "Cancelada" or "No se presentó" or "Check-in sin embarque" => AppEstilos.ColorAcento,
                    _ => null
                };
                if (color == null) return;
                e.CellStyle!.ForeColor = color.Value;
                e.CellStyle.Font = AppEstilos.FuenteBoton;
            };
            _dgv.SelectionChanged += (s, e) => ActualizarAcciones();

            tarjetaLista.Controls.Add(_dgv);
            tarjetaLista.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.White });
            tarjetaLista.Controls.Add(barra);
            _dgv.BringToFront();

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

            derecha.Controls.Add(tarjetaLista);
            derecha.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 14, BackColor = AppEstilos.ColorSecundario });
            derecha.Controls.Add(_encabezado);
            derecha.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 14, BackColor = AppEstilos.ColorSecundario });
            derecha.Controls.Add(barraAcciones);

            Controls.Add(derecha);
            Controls.Add(new Panel { Dock = DockStyle.Left, Width = 16, BackColor = AppEstilos.ColorSecundario });
            Controls.Add(tarjetaVuelos);

            CambiarVista(Vista.Proximos);
        }

        private static ComboBox CrearComboFiltro() => new ComboBox
        {
            Width = 306,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            Font = AppEstilos.FuenteNormal,
            Margin = new Padding(0, 0, 0, 8)
        };

        private static string NombreEstado(string estado) => estado == "PendientePago" ? "Pendiente de pago" : estado;

        private static void DibujarBorde(object? sender, PaintEventArgs e)
        {
            if (sender is not Control c) return;
            using var lapiz = new Pen(AppEstilos.ColorBorde);
            e.Graphics.DrawRectangle(lapiz, 0, 0, c.Width - 1, c.Height - 1);
        }
    }
}
