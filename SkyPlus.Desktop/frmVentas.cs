using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmVentas : Form
    {
        private readonly PagoClient _pagoClient = new PagoClient();
        private readonly ReservaClient _reservaClient = new ReservaClient();
        private readonly ReembolsoClient _reembolsoClient = new ReembolsoClient();
        private readonly VueloClient _vueloClient = new VueloClient();
        private List<VentaFila> _ventas = new List<VentaFila>();

        // Filtros
        private ComboBox _cmbVuelo = null!;
        private ComboBox _cmbTipo = null!;
        private ComboBox _cmbMetodo = null!;
        private DateTimePicker _dtpDesde = null!;
        private DateTimePicker _dtpHasta = null!;
        private Label _lblResumen = null!;

        private class VentaFila
        {
            public int IdPago { get; set; }
            public int IdReserva { get; set; }
            public int IdUsuarioAgente { get; set; }
            public string CodigoReserva { get; set; } = string.Empty;
            public string Pasajero { get; set; } = string.Empty;
            public string Vuelo { get; set; } = string.Empty;
            public string Asiento { get; set; } = string.Empty;
            public string Concepto { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public string MetodoPago { get; set; } = string.Empty;
            public DateTime FechaPago { get; set; }
            public string Fecha => FechaPago.ToString("dd/MM/yyyy HH:mm");
            public string Estado { get; set; } = string.Empty;
            public decimal Reembolsado { get; set; }
        }

        public frmVentas()
        {
            InitializeComponent();
        }

        private async void frmVentas_Load(object sender, EventArgs e)
        {
            ConfigurarTabla();
            AppEstilos.EstilizarGrid(dgvVentas);
            AppEstilos.EstilizarBotonSecundario(btnVerDetalle);

            _cmbVuelo = Listados.Combo(170, Array.Empty<string>(), AplicarFiltros);
            _cmbTipo = Listados.Combo(150, new[] { "Pasaje", "Equipaje / exceso" }, AplicarFiltros);
            _cmbMetodo = Listados.Combo(140, new[] { "Efectivo", "Transferencia", "Tarjeta" }, AplicarFiltros);
            _dtpDesde = Listados.Fecha(AplicarFiltros);
            _dtpHasta = Listados.Fecha(AplicarFiltros);

            Listados.ArmarFiltros(pnlFiltros, txtBuscar, btnRefrescar,
                "Reserva / pasajero",
                "Código de reserva o pasajero",
                ("Vuelo", _cmbVuelo),
                ("Concepto", _cmbTipo),
                ("Método de pago", _cmbMetodo),
                ("Desde", _dtpDesde),
                ("Hasta", _dtpHasta));
            Listados.ConectarBusqueda(txtBuscar, btnRefrescar, AplicarFiltros, CargarVentasAsync, LimpiarFiltros);

            lblCantidad.Visible = false;
            _lblResumen = Listados.CrearResumen(this, dgvVentas);

            await CargarVentasAsync();
        }

        private void LimpiarFiltros()
        {
            foreach (var combo in new[] { _cmbVuelo, _cmbTipo, _cmbMetodo })
                combo.SelectedIndex = 0;
            _dtpDesde.Checked = false;
            _dtpHasta.Checked = false;
        }

        private async Task CargarVentasAsync()
        {
            try
            {
                var tPagos = _pagoClient.ObtenerTodosAsync();
                var tReservas = _reservaClient.ObtenerTodosAsync();
                var tReembolsos = _reembolsoClient.ObtenerTodosAsync();
                var tVuelos = _vueloClient.ObtenerTodosAsync();
                await Task.WhenAll(tPagos, tReservas, tReembolsos, tVuelos);

                var porId = (tReservas.Result ?? new List<ReservaResponse>()).ToDictionary(r => r.IdReserva);
                var vuelos = (tVuelos.Result ?? new List<VueloResponse>()).ToDictionary(v => v.IdVuelo);
                var reembolsos = (tReembolsos.Result ?? new List<ReembolsoResponse>())
                    .GroupBy(r => r.IdPago)
                    .ToDictionary(g => g.Key, g => g.Sum(r => r.MontoReembolsado));

                _ventas = (tPagos.Result ?? new List<PagoResponse>()).Select(p =>
                {
                    porId.TryGetValue(p.IdReserva, out var reserva);
                    VueloResponse? vuelo = null;
                    if (reserva != null) vuelos.TryGetValue(reserva.IdVuelo, out vuelo);
                    return new VentaFila
                    {
                        IdPago = p.IdPago,
                        IdReserva = p.IdReserva,
                        IdUsuarioAgente = p.IdUsuarioAgente,
                        CodigoReserva = p.CodigoReserva,
                        Pasajero = reserva?.Pasajero ?? string.Empty,
                        Vuelo = vuelo != null ? $"{vuelo.NumeroVuelo} · {vuelo.Salida:dd/MM}" : reserva?.Vuelo ?? string.Empty,
                        Asiento = reserva?.Asiento ?? string.Empty,
                        Concepto = p.TipoPago == "Adicional" ? "Equipaje / exceso" : "Pasaje",
                        Monto = p.Monto,
                        MetodoPago = p.MetodoPago,
                        FechaPago = p.FechaPago.Kind == DateTimeKind.Utc ? p.FechaPago.ToLocalTime() : p.FechaPago,
                        Estado = reembolsos.ContainsKey(p.IdPago) ? "Reembolsado" : p.EstadoPago,
                        Reembolsado = reembolsos.GetValueOrDefault(p.IdPago)
                    };
                }).OrderByDescending(v => v.FechaPago).ToList();

                Listados.CargarOpciones(_cmbVuelo, _ventas
                    .Where(v => v.Vuelo != "")
                    .OrderByDescending(v => v.FechaPago)
                    .Select(v => v.Vuelo));

                AplicarFiltros();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    ex.StatusCode.HasValue ? ex.Message :
                        "No se pudo conectar con la API de SkyPlus. Verificá que esté ejecutándose.",
                    "Error al cargar ventas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar las ventas.\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarTabla()
        {
            dgvVentas.AutoGenerateColumns = false;
            dgvVentas.Columns.Clear();
            dgvVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            AgregarColumna("colReserva", "Reserva", "CodigoReserva", 80);
            AgregarColumna("colPasajero", "Pasajero", "Pasajero", 150);
            AgregarColumna("colVuelo", "Vuelo", "Vuelo", 100);
            AgregarColumna("colAsiento", "Asiento", "Asiento", 60);
            AgregarColumna("colConcepto", "Concepto", "Concepto", 110);
            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMonto",
                HeaderText = "Monto",
                DataPropertyName = "Monto",
                FillWeight = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C0" }
            });
            AgregarColumna("colMetodo", "Método de pago", "MetodoPago", 100);
            AgregarColumna("colFecha", "Fecha", "Fecha", 110);
            AgregarColumna("colEstado", "Estado", "Estado", 90);
        }

        private void AgregarColumna(string nombre, string titulo, string propiedad, float ancho)
        {
            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nombre,
                HeaderText = titulo,
                DataPropertyName = propiedad,
                FillWeight = ancho
            });
        }

        private void AplicarFiltros()
        {
            if (_lblResumen == null) return; // todavía armando la pantalla

            var texto = txtBuscar.Text.Trim();
            var resultado = _ventas
                .Where(v => AppEstilos.Coincide(texto, v.CodigoReserva, v.Pasajero))
                .Where(v => Listados.Pasa(_cmbVuelo, v.Vuelo))
                .Where(v => Listados.Pasa(_cmbTipo, v.Concepto))
                .Where(v => Listados.Pasa(_cmbMetodo, v.MetodoPago))
                .Where(v => Listados.PasaDesde(_dtpDesde, v.FechaPago) && Listados.PasaHasta(_dtpHasta, v.FechaPago))
                .ToList();

            dgvVentas.DataSource = null;
            dgvVentas.DataSource = resultado;

            decimal cobrado = resultado.Sum(v => v.Monto);
            decimal reembolsado = resultado.Sum(v => v.Reembolsado);
            string porMetodo = string.Join("  ·  ", resultado
                .GroupBy(v => v.MetodoPago)
                .OrderByDescending(g => g.Sum(v => v.Monto))
                .Select(g => $"{g.Key} {g.Sum(v => v.Monto):C0}"));

            _lblResumen.Text =
                $"{resultado.Count} cobros   |   Cobrado {cobrado:C0}   |   Reembolsado {reembolsado:C0}   |   " +
                $"Neto {cobrado - reembolsado:C0}" + (porMetodo == "" ? "" : $"   |   {porMetodo}");
        }

        // Los manejadores del diseñador quedan sin uso: la búsqueda es en vivo y Refrescar lo maneja Listados
        private void txtBuscar_TextChanged(object sender, EventArgs e) { }
        private void btnBuscar_Click(object sender, EventArgs e) { }
        private void btnRefrescar_Click(object sender, EventArgs e) { }

        private void btnVerDetalle_Click(object sender, EventArgs e)
        {
            if (dgvVentas.CurrentRow?.DataBoundItem is not VentaFila venta)
            {
                MessageBox.Show("Seleccione una venta.", "Ventas",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show(
                $"Pago N.º: {venta.IdPago}\n" +
                $"Reserva: {venta.CodigoReserva} (ID {venta.IdReserva})\n" +
                $"Pasajero: {venta.Pasajero}\n" +
                $"Vuelo: {venta.Vuelo}\n" +
                $"Asiento: {venta.Asiento}\n" +
                $"Concepto: {venta.Concepto}\n" +
                $"Monto: {venta.Monto:C0}\n" +
                (venta.Reembolsado > 0 ? $"Reembolsado: {venta.Reembolsado:C0}\n" : "") +
                $"Método: {venta.MetodoPago}\n" +
                $"Fecha: {venta.Fecha}\n" +
                $"Estado: {venta.Estado}\n" +
                $"Agente (ID): {venta.IdUsuarioAgente}",
                "Detalle de venta", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
