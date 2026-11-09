// SkyPlus.Desktop/frmCheckin.cs
// Check-in por vuelo:
//   1. A la izquierda se elige el vuelo (hoy y los próximos 2 días).
//   2. Arriba se ve cuántos pasajeros confirmados tiene y cuántos check-in faltan.
//   3. En la lista se elige el pasajero; abajo se ve si puede hacer el check-in
//      (y si no, por qué). En el mostrador se puede agregar una valija,
//      cobrarla, pesarla, cobrar el exceso de peso si lo hay, y hacer el check-in.

using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Utilidades;

namespace SkyPlus.Desktop
{
    public partial class frmCheckin : Form
    {
        private readonly CheckInClient _checkInClient = new CheckInClient();
        private readonly EquipajeClient _equipajeClient = new EquipajeClient();
        private readonly PagoClient _pagoClient = new PagoClient();

        private List<ResumenVuelo> _vuelos = new();
        private ResumenVuelo? _vueloActual;
        private PasajeroVuelo? _pasajeroActual;
        private int _consultaActual; // para descartar respuestas viejas si se cambia rápido de pasajero

        // Controles (se crean en ConstruirPantalla)
        private ucListaVuelos _listaVuelos = null!;
        private EncabezadoVuelo _encabezado = null!;
        private TextBox _txtBuscar = null!;
        private ComboBox _cmbFiltro = null!;
        private DataGridView _dgvPasajeros = null!;
        private Label _lblPasajero = null!;
        private Label _lblEstado = null!;
        private Label _lblEquipaje = null!;
        private FlowLayoutPanel _pnlPeso = null!;
        private ComboBox _cmbEquipaje = null!;
        private NumericUpDown _numPeso = null!;
        private Button _btnPesar = null!;
        private FlowLayoutPanel _pnlCobro = null!;
        private Label _lblCobro = null!;
        private ComboBox _cmbMetodo = null!;
        private Button _btnCobrar = null!;
        private FlowLayoutPanel _pnlAgregar = null!;
        private ComboBox _cmbTipoAgregar = null!;
        private Button _btnAgregarEquipaje = null!;
        private List<TipoEquipajeResponse> _tiposEquipaje = new();
        private Button _btnRealizarCheckin = null!;
        private Button _btnRefrescar = null!;

        public frmCheckin()
        {
            InitializeComponent();
        }

        private async void frmCheckin_Load(object sender, EventArgs e)
        {
            ConstruirPantalla();
            await CargarTiposEquipajeAsync();
            await CargarVuelosAsync();
        }

        // =====================================================================
        // Datos
        // =====================================================================
        private async Task CargarVuelosAsync()
        {
            int? idVuelo = _vueloActual?.Vuelo.IdVuelo;
            int? idReserva = _pasajeroActual?.Reserva.IdReserva;

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

        // Tipos de equipaje que se pueden agregar en el mostrador (el de mano sin costo va incluido)
        private async Task CargarTiposEquipajeAsync()
        {
            try
            {
                _tiposEquipaje = (await _equipajeClient.ObtenerTiposAsync() ?? new List<TipoEquipajeResponse>())
                    .Where(t => t.CostoAdicional > 0)
                    .OrderBy(t => t.CostoAdicional)
                    .ToList();

                _cmbTipoAgregar.DataSource = _tiposEquipaje
                    .Select(t => new { t.IdTipoEquipaje, Texto = $"{t.Nombre} · hasta {t.LimiteKg:0.##} kg · {t.CostoAdicional:C0}" })
                    .ToList();
                _cmbTipoAgregar.DisplayMember = "Texto";
                _cmbTipoAgregar.ValueMember = "IdTipoEquipaje";
            }
            catch (HttpRequestException)
            {
                _tiposEquipaje = new(); // sin tipos no se muestra "Agregar equipaje"
            }
        }

        private void MostrarVuelo(ResumenVuelo? vuelo)
        {
            _vueloActual = vuelo;

            _encabezado.Mostrar(vuelo,
                ("Pasajeros confirmados", vuelo?.Confirmadas ?? 0, AppEstilos.ColorTextoOscuro),
                ("Check-in realizados", vuelo?.CheckIns ?? 0, AppEstilos.ColorExito),
                ("Faltan check-in", vuelo?.SinCheckIn ?? 0, (vuelo?.SinCheckIn ?? 0) > 0 ? AppEstilos.ColorAcento : AppEstilos.ColorTextoOscuro),
                ("Ya embarcaron", vuelo?.Embarcados ?? 0, AppEstilos.ColorPrimario));

            LlenarGrilla();
        }

        private void LlenarGrilla()
        {
            _dgvPasajeros.Rows.Clear();
            if (_vueloActual == null)
            {
                MostrarPasajero(null);
                return;
            }

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

            if (_dgvPasajeros.Rows.Count == 0)
                MostrarPasajero(null);
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

        // Muestra abajo el estado del pasajero elegido
        private async void MostrarPasajero(PasajeroVuelo? p)
        {
            _pasajeroActual = p;
            int consulta = ++_consultaActual;

            _btnRealizarCheckin.Enabled = false;
            _pnlPeso.Visible = false;
            _pnlCobro.Visible = false;
            _pnlAgregar.Visible = false;
            _lblEquipaje.Text = "";

            if (p == null)
            {
                _lblPasajero.Text = "Elegí un pasajero de la lista";
                _lblEstado.Text = _vueloActual == null
                    ? "Primero elegí un vuelo."
                    : "Seleccioná una fila para ver si puede hacer el check-in.";
                _lblEstado.ForeColor = AppEstilos.ColorTextoSuave;
                return;
            }

            _lblPasajero.Text = $"{p.Reserva.Pasajero}  ·  Reserva {p.Reserva.CodigoReserva}  ·  Asiento {p.Reserva.Asiento}";

            if (p.CheckIn != null)
            {
                _lblEstado.Text = p.Estado == "Embarcado"
                    ? "El pasajero ya embarcó."
                    : $"Check-in realizado el {p.CheckIn.FechaLocal:dd/MM HH:mm}. El boarding pass está disponible en Boarding Pass.";
                _lblEstado.ForeColor = AppEstilos.ColorExito;
                return;
            }

            _lblEstado.Text = "Consultando el estado de la reserva...";
            _lblEstado.ForeColor = AppEstilos.ColorTextoSuave;

            try
            {
                var tEstado = _checkInClient.ConsultarEstadoAsync(p.Reserva.IdReserva);
                var tEquipajes = _equipajeClient.ObtenerPorReservaAsync(p.Reserva.IdReserva);
                await Task.WhenAll(tEstado, tEquipajes);

                if (consulta != _consultaActual) return; // el usuario ya eligió otro pasajero

                var estado = tEstado.Result;
                var equipajes = (tEquipajes.Result ?? new List<EquipajeReservaResponse>())
                    .Where(eq => eq.Estado != "Cancelado")
                    .ToList();

                bool puede = estado?.PuedeRealizarCheckIn == true;
                string mensaje = estado?.Mensaje ?? "No se pudo consultar el estado.";

                // Equipaje agregado en el mostrador o exceso de peso: se cobra acá mismo
                bool hayExceso = equipajes.Any(eq =>
                    eq.Estado == "Activo" && eq.PesoKg.HasValue && eq.PesoKg > eq.LimiteKgContratado);
                bool hayEquipajeSinPagar = equipajes.Any(eq => eq.Estado == "PendientePago");

                if (!puede && estado != null && estado.CargosPendientes > 0)
                {
                    if (hayExceso || hayEquipajeSinPagar)
                    {
                        string concepto = hayExceso && hayEquipajeSinPagar ? "Equipaje y exceso de peso"
                            : hayExceso ? "Exceso de peso" : "Equipaje agregado";
                        mensaje += " Cobralo para poder continuar.";
                        _lblCobro.Text = $"{concepto} a cobrar: {estado.MontoPendiente:C0}";
                        _btnCobrar.Text = $"Cobrar {estado.MontoPendiente:C0}";
                        _pnlCobro.Visible = true;
                    }
                    else
                    {
                        mensaje += $" Monto pendiente: {estado.MontoPendiente:C0}. Ese pago lo registra el Agente de Reservas.";
                    }
                }

                _lblEstado.Text = mensaje;
                _lblEstado.ForeColor = puede ? AppEstilos.ColorExito : AppEstilos.ColorAcento;
                _btnRealizarCheckin.Enabled = puede;

                _lblEquipaje.Text = equipajes.Count == 0
                    ? "Equipaje: sin equipaje despachado."
                    : "Equipaje: " + string.Join("  |  ", equipajes.Select(eq => eq.Descripcion));

                // El pasajero puede sumar una valija en el mostrador antes del check-in
                _pnlAgregar.Visible = _tiposEquipaje.Count > 0;

                var sinPesar = equipajes.Where(eq => eq.Estado == "Activo" && eq.PesoKg == null).ToList();
                if (sinPesar.Count > 0)
                {
                    _cmbEquipaje.DataSource = sinPesar;
                    _cmbEquipaje.DisplayMember = nameof(EquipajeReservaResponse.Descripcion);
                    _numPeso.Value = Math.Min(_numPeso.Maximum, Math.Max(_numPeso.Minimum, sinPesar[0].LimiteKgContratado));
                    _pnlPeso.Visible = true;
                }
            }
            catch (HttpRequestException ex)
            {
                if (consulta != _consultaActual) return;
                _lblEstado.Text = "No se pudo consultar el estado: " + ex.Message;
                _lblEstado.ForeColor = AppEstilos.ColorError;
            }
        }

        // =====================================================================
        // Acciones
        // =====================================================================
        private async void btnPesar_Click(object? sender, EventArgs e)
        {
            if (_pasajeroActual == null || _cmbEquipaje.SelectedItem is not EquipajeReservaResponse equipaje)
                return;

            decimal peso = Math.Round(_numPeso.Value, 2);
            if (peso <= 0)
            {
                MessageBox.Show("Ingresá un peso mayor que cero.", "Equipaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _btnPesar.Enabled = false;
                var resultado = await _equipajeClient.RegistrarPesoAsync(equipaje.IdEquipajeReserva, peso);

                string mensaje = resultado?.Mensaje ?? "Peso registrado.";
                if (resultado != null && resultado.RequierePago)
                    mensaje += $"\n\nExceso: {resultado.ExcesoKg:0.##} kg → {resultado.MontoExceso:C0}." +
                               "\nCobralo con el botón \"Cobrar\" para continuar con el check-in.";

                MessageBox.Show(mensaje, "Equipaje", MessageBoxButtons.OK,
                    resultado?.RequierePago == true ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                MostrarPasajero(_pasajeroActual); // vuelve a consultar el estado
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
            }
            finally
            {
                _btnPesar.Enabled = true;
            }
        }

        private async void btnAgregarEquipaje_Click(object? sender, EventArgs e)
        {
            var p = _pasajeroActual;
            if (p == null || _cmbTipoAgregar.SelectedValue is not int idTipo) return;

            var confirmacion = MessageBox.Show(
                $"¿Agregar \"{_cmbTipoAgregar.Text}\" a la reserva de {p.Reserva.Pasajero}?",
                "Agregar equipaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmacion != DialogResult.Yes) return;

            try
            {
                _btnAgregarEquipaje.Enabled = false;
                await _equipajeClient.ContratarAsync(p.Reserva.IdReserva, idTipo);

                MessageBox.Show("Equipaje agregado. Ahora cobralo y después registrá su peso.",
                    "Agregar equipaje", MessageBoxButtons.OK, MessageBoxIcon.Information);

                MostrarPasajero(p);
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
            }
            finally
            {
                _btnAgregarEquipaje.Enabled = true;
            }
        }

        private async void btnCobrar_Click(object? sender, EventArgs e)
        {
            var p = _pasajeroActual;
            if (p == null) return;

            string metodo = _cmbMetodo.SelectedItem as string ?? "Efectivo";
            var confirmacion = MessageBox.Show(
                $"¿Registrar el cobro ({_lblCobro.Text}) de {p.Reserva.Pasajero} en {metodo}?",
                "Cobrar en el mostrador", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                _btnCobrar.Enabled = false;
                var pago = await _pagoClient.CrearAdicionalAsync(p.Reserva.IdReserva, metodo);

                MessageBox.Show(
                    $"Cobro registrado: {pago?.Monto ?? 0:C0} ({metodo}).",
                    "Cobrar en el mostrador", MessageBoxButtons.OK, MessageBoxIcon.Information);

                MostrarPasajero(p); // vuelve a consultar: ahora debería poder hacer el check-in
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
            }
            finally
            {
                _btnCobrar.Enabled = true;
            }
        }

        private async void btnRealizarCheckin_Click(object? sender, EventArgs e)
        {
            var p = _pasajeroActual;
            if (p == null) return;

            var confirmacion = MessageBox.Show(
                $"¿Confirmás el check-in de {p.Reserva.Pasajero} (reserva {p.Reserva.CodigoReserva}, asiento {p.Reserva.Asiento})?",
                "Realizar check-in", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                _btnRealizarCheckin.Enabled = false;
                var checkIn = await _checkInClient.CrearAsync(p.Reserva.IdReserva);

                var abrir = MessageBox.Show(
                    "Check-in realizado correctamente.\n\n¿Querés abrir el boarding pass ahora para imprimirlo?",
                    "Check-in", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (abrir == DialogResult.Yes && checkIn != null)
                    await BoardingPassPdf.AbrirAsync(checkIn.IdCheckin, p.Reserva.CodigoReserva);

                await CargarVuelosAsync();
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
                MostrarPasajero(p);
            }
            catch (IOException ex)
            {
                MessageBox.Show("El check-in se hizo, pero no se pudo abrir el PDF: " + ex.Message,
                    "Boarding Pass", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                await CargarVuelosAsync();
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

            _listaVuelos = new ucListaVuelos { Dock = DockStyle.Fill };
            _listaVuelos.VueloSeleccionado += (s, vuelo) => MostrarVuelo(vuelo);

            tarjetaVuelos.Controls.Add(_listaVuelos);
            tarjetaVuelos.Controls.Add(cabeceraVuelos);
            _listaVuelos.BringToFront();

            // ---------- Columna derecha ----------
            var derecha = new Panel { Dock = DockStyle.Fill, BackColor = AppEstilos.ColorSecundario };

            _encabezado = new EncabezadoVuelo { Dock = DockStyle.Top };

            // Lista de pasajeros
            var tarjetaLista = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16, 12, 16, 12) };
            tarjetaLista.Paint += DibujarBorde;

            var barraFiltros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, BackColor = Color.White, WrapContents = false };
            var lblLista = new Label
            {
                Text = "Pasajeros confirmados",
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
            _cmbFiltro.Items.AddRange(new object[] { "Todos", "Sin check-in", "Check-in realizado", "Embarcado" });
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
            _dgvPasajeros.SelectionChanged += (s, e) =>
                MostrarPasajero(_dgvPasajeros.CurrentRow?.Tag as PasajeroVuelo);

            var separadorGrilla = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.White };
            tarjetaLista.Controls.Add(_dgvPasajeros);
            tarjetaLista.Controls.Add(separadorGrilla);
            tarjetaLista.Controls.Add(barraFiltros);
            _dgvPasajeros.BringToFront();

            // Detalle del pasajero elegido
            var tarjetaDetalle = new Panel { Dock = DockStyle.Bottom, Height = 236, BackColor = Color.White, Padding = new Padding(20, 14, 20, 14) };
            tarjetaDetalle.Paint += DibujarBorde;

            _btnRealizarCheckin = new Button { Text = "Realizar check-in", Dock = DockStyle.Right, Width = 190, Enabled = false };
            AppEstilos.EstilizarBotonPrimario(_btnRealizarCheckin);
            _btnRealizarCheckin.Click += btnRealizarCheckin_Click;
            var contenedorBoton = new Panel { Dock = DockStyle.Right, Width = 190, BackColor = Color.White };
            _btnRealizarCheckin.Dock = DockStyle.Top;
            _btnRealizarCheckin.Height = 44;
            contenedorBoton.Controls.Add(_btnRealizarCheckin);

            var info = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.White
            };
            _lblPasajero = new Label { AutoSize = true, Font = AppEstilos.FuenteSubtitulo, ForeColor = AppEstilos.ColorTextoOscuro, Margin = new Padding(0, 0, 0, 6) };
            _lblEstado = new Label { AutoSize = true, Font = AppEstilos.FuenteBoton, MaximumSize = new Size(500, 0), Margin = new Padding(0, 0, 0, 6) };
            _lblEquipaje = new Label { AutoSize = true, Font = AppEstilos.FuenteNormal, ForeColor = AppEstilos.ColorTextoSuave, MaximumSize = new Size(500, 0), Margin = new Padding(0, 0, 0, 6) };

            _pnlPeso = new FlowLayoutPanel { AutoSize = true, WrapContents = true, BackColor = Color.White, Visible = false, Margin = Padding.Empty };
            var lblPesar = new Label { Text = "Pesar equipaje:", AutoSize = true, Font = AppEstilos.FuenteBoton, ForeColor = AppEstilos.ColorTextoOscuro, Margin = new Padding(0, 8, 8, 0) };
            _cmbEquipaje = new ComboBox { Width = 230, DropDownWidth = 360, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Font = AppEstilos.FuenteNormal, Margin = new Padding(0, 4, 8, 0) };
            _numPeso = new NumericUpDown { Width = 80, DecimalPlaces = 2, Minimum = 0.1m, Maximum = 99m, Increment = 0.5m, Font = AppEstilos.FuenteNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 4, 0) };
            var lblKg = new Label { Text = "kg", AutoSize = true, Margin = new Padding(0, 8, 12, 0), ForeColor = AppEstilos.ColorTextoSuave };
            _btnPesar = new Button { Text = "Registrar peso", Width = 130, Margin = new Padding(0, 2, 0, 0) };
            AppEstilos.EstilizarBotonSecundario(_btnPesar);
            _btnPesar.Click += btnPesar_Click;
            _pnlPeso.Controls.AddRange(new Control[] { lblPesar, _cmbEquipaje, _numPeso, lblKg, _btnPesar });

            _pnlCobro = new FlowLayoutPanel { AutoSize = true, WrapContents = true, BackColor = Color.White, Visible = false, Margin = Padding.Empty };
            _lblCobro = new Label { AutoSize = true, Font = AppEstilos.FuenteBoton, ForeColor = AppEstilos.ColorAcento, Margin = new Padding(0, 8, 12, 0) };
            _cmbMetodo = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Font = AppEstilos.FuenteNormal, Margin = new Padding(0, 4, 8, 0) };
            _cmbMetodo.Items.AddRange(new object[] { "Efectivo", "Transferencia", "Tarjeta" });
            _cmbMetodo.SelectedIndex = 0;
            _btnCobrar = new Button { Text = "Cobrar", Width = 160, Margin = new Padding(0, 2, 0, 0) };
            AppEstilos.EstilizarBotonPrimario(_btnCobrar);
            AppEstilos.CambiarColorBoton(_btnCobrar, AppEstilos.ColorAcento);
            _btnCobrar.Click += btnCobrar_Click;
            _pnlCobro.Controls.AddRange(new Control[] { _lblCobro, _cmbMetodo, _btnCobrar });

            _pnlAgregar = new FlowLayoutPanel { AutoSize = true, WrapContents = true, BackColor = Color.White, Visible = false, Margin = new Padding(0, 4, 0, 0) };
            var lblAgregar = new Label { Text = "Agregar equipaje:", AutoSize = true, Font = AppEstilos.FuenteBoton, ForeColor = AppEstilos.ColorTextoOscuro, Margin = new Padding(0, 8, 8, 0) };
            _cmbTipoAgregar = new ComboBox { Width = 230, DropDownWidth = 360, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Font = AppEstilos.FuenteNormal, Margin = new Padding(0, 4, 8, 0) };
            _btnAgregarEquipaje = new Button { Text = "Agregar", Width = 110, Margin = new Padding(0, 2, 0, 0) };
            AppEstilos.EstilizarBotonSecundario(_btnAgregarEquipaje);
            _btnAgregarEquipaje.Click += btnAgregarEquipaje_Click;
            _pnlAgregar.Controls.AddRange(new Control[] { lblAgregar, _cmbTipoAgregar, _btnAgregarEquipaje });

            // Orden del mostrador: primero cobrar lo pendiente, después pesar, y opcionalmente agregar otra valija
            info.Controls.AddRange(new Control[] { _lblPasajero, _lblEstado, _lblEquipaje, _pnlCobro, _pnlPeso, _pnlAgregar });

            // Las filas de pesar/cobrar no pueden ser más anchas que el panel: si no entran, bajan de línea
            info.AutoScroll = true;
            info.Resize += (s, e) =>
            {
                int ancho = Math.Max(200, info.ClientSize.Width - 8);
                foreach (Control c in new Control[] { _lblEstado, _lblEquipaje, _pnlPeso, _pnlCobro, _pnlAgregar })
                    c.MaximumSize = new Size(ancho, 0);
            };

            tarjetaDetalle.Controls.Add(info);
            tarjetaDetalle.Controls.Add(contenedorBoton);
            info.BringToFront();

            // Orden de acoplamiento en la columna derecha:
            // encabezado arriba, detalle abajo y la lista ocupa el resto
            derecha.Controls.Add(tarjetaLista);
            derecha.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 14, BackColor = AppEstilos.ColorSecundario });
            derecha.Controls.Add(_encabezado);
            derecha.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 14, BackColor = AppEstilos.ColorSecundario });
            derecha.Controls.Add(tarjetaDetalle);

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
            "Sin check-in" => AppEstilos.ColorAcento,
            "Check-in realizado" => AppEstilos.ColorExito,
            "Embarcado" => AppEstilos.ColorPrimario,
            _ => AppEstilos.ColorTextoOscuro
        };
    }
}
