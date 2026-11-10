// SkyPlus.Desktop/frmCancelaciones.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmCancelaciones : Form
    {
        private readonly ReservaClient _reservaClient = new ReservaClient();
        private readonly ReembolsoClient _reembolsoClient = new ReembolsoClient();
        private readonly VueloClient _vueloClient = new VueloClient();
        private List<CancelacionFila> _cancelaciones = new List<CancelacionFila>();

        private ComboBox _cmbVuelo = null!;
        private ComboBox _cmbMotivo = null!;
        private DateTimePicker _dtpDesde = null!;
        private DateTimePicker _dtpHasta = null!;
        private Label _lblResumen = null!;

        private class CancelacionFila
        {
            public string CodigoReserva { get; set; } = string.Empty;
            public string Pasajero { get; set; } = string.Empty;
            public string Vuelo { get; set; } = string.Empty;
            public string Asiento { get; set; } = string.Empty;
            public DateTime Fecha { get; set; }
            public string Motivo { get; set; } = string.Empty;
            public decimal Reembolsado { get; set; }
        }

        public frmCancelaciones()
        {
            InitializeComponent();
        }

        private async void frmCancelaciones_Load(object sender, EventArgs e)
        {
            ConfigurarColumnas();
            AppEstilos.EstilizarGrid(dgvCancelaciones);

            _cmbVuelo = Listados.Combo(170, Array.Empty<string>(), AplicarFiltro);
            _cmbMotivo = Listados.Combo(230, Array.Empty<string>(), AplicarFiltro);
            _dtpDesde = Listados.Fecha(AplicarFiltro);
            _dtpHasta = Listados.Fecha(AplicarFiltro);

            btnBuscar.Visible = false;
            lblCancelaciones.Visible = false;
            Listados.ArmarFiltros(pnlFiltros, txtBuscar, btnRefrescar,
                "Reserva / pasajero",
                "Código de reserva o nombre y apellido",
                ("Vuelo", _cmbVuelo),
                ("Motivo", _cmbMotivo),
                ("Desde", _dtpDesde),
                ("Hasta", _dtpHasta));
            Listados.ConectarBusqueda(txtBuscar, btnRefrescar, AplicarFiltro, CargarCancelaciones, LimpiarFiltros);
            _lblResumen = Listados.CrearResumen(this, dgvCancelaciones);

            await CargarCancelaciones();
        }

        private void LimpiarFiltros()
        {
            _cmbVuelo.SelectedIndex = 0;
            _cmbMotivo.SelectedIndex = 0;
            _dtpDesde.Checked = false;
            _dtpHasta.Checked = false;
        }

        private void ConfigurarColumnas()
        {
            dgvCancelaciones.AutoGenerateColumns = false;
            dgvCancelaciones.Columns.Clear();
            dgvCancelaciones.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Columna(nameof(CancelacionFila.CodigoReserva), "Reserva", 80);
            Columna(nameof(CancelacionFila.Pasajero), "Pasajero", 160);
            Columna(nameof(CancelacionFila.Vuelo), "Vuelo", 100);
            Columna(nameof(CancelacionFila.Asiento), "Asiento", 60);
            Columna(nameof(CancelacionFila.Fecha), "Fecha cancelación", 120, "dd/MM/yyyy HH:mm");
            Columna(nameof(CancelacionFila.Motivo), "Motivo", 200);
            Columna(nameof(CancelacionFila.Reembolsado), "Reembolsado", 100, "C0");
        }

        private void Columna(string propiedad, string titulo, float peso, string? formato = null)
        {
            var columna = new DataGridViewTextBoxColumn { DataPropertyName = propiedad, HeaderText = titulo, FillWeight = peso };
            if (formato != null) columna.DefaultCellStyle.Format = formato;
            dgvCancelaciones.Columns.Add(columna);
        }

        private async Task CargarCancelaciones()
        {
            try
            {
                var tReservas = _reservaClient.ObtenerTodosAsync();
                var tReembolsos = _reembolsoClient.ObtenerTodosAsync();
                var tVuelos = _vueloClient.ObtenerTodosAsync();
                await Task.WhenAll(tReservas, tReembolsos, tVuelos);

                var vuelos = (tVuelos.Result ?? new List<VueloResponse>()).ToDictionary(v => v.IdVuelo);
                var reembolsos = (tReembolsos.Result ?? new List<ReembolsoResponse>())
                    .GroupBy(r => r.IdReserva)
                    .ToDictionary(g => g.Key, g => g.ToList());

                _cancelaciones = (tReservas.Result ?? new List<ReservaResponse>())
                    .Where(r => r.EstadoReserva == "Cancelada")
                    .Select(r =>
                    {
                        vuelos.TryGetValue(r.IdVuelo, out var vuelo);
                        reembolsos.TryGetValue(r.IdReserva, out var lista);
                        var fecha = r.FechaCancelacion ?? lista?.Max(x => x.FechaReembolso) ?? r.FechaReserva;
                        return new CancelacionFila
                        {
                            CodigoReserva = r.CodigoReserva,
                            Pasajero = r.Pasajero,
                            Vuelo = vuelo != null ? $"{vuelo.NumeroVuelo} · {vuelo.Salida:dd/MM}" : r.Vuelo,
                            Asiento = r.Asiento,
                            Fecha = fecha.Kind == DateTimeKind.Utc ? fecha.ToLocalTime() : fecha,
                            Motivo = lista?.FirstOrDefault()?.Motivo ?? "Sin reembolso registrado",
                            Reembolsado = lista?.Sum(x => x.MontoReembolsado) ?? 0
                        };
                    })
                    .OrderByDescending(c => c.Fecha)
                    .ToList();

                Listados.CargarOpciones(_cmbVuelo, _cancelaciones.Select(c => c.Vuelo));
                Listados.CargarOpciones(_cmbMotivo, _cancelaciones.Select(c => c.Motivo).OrderBy(m => m));
                AplicarFiltro();
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("No se pudo conectar con la API de SkyPlus.",
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al cargar las cancelaciones.\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarFiltro()
        {
            if (_lblResumen == null) return;

            var texto = txtBuscar.Text.Trim();
            var filtradas = _cancelaciones
                .Where(c => AppEstilos.Coincide(texto, c.CodigoReserva, c.Pasajero))
                .Where(c => Listados.Pasa(_cmbVuelo, c.Vuelo))
                .Where(c => Listados.Pasa(_cmbMotivo, c.Motivo))
                .Where(c => Listados.PasaDesde(_dtpDesde, c.Fecha) && Listados.PasaHasta(_dtpHasta, c.Fecha))
                .ToList();

            dgvCancelaciones.DataSource = null;
            dgvCancelaciones.DataSource = filtradas;

            _lblResumen.Text =
                $"{filtradas.Count} cancelaciones   |   Reembolsado {filtradas.Sum(c => c.Reembolsado):C0}   |   " +
                $"{filtradas.Select(c => c.Vuelo).Distinct().Count()} vuelos afectados";
        }

        // Manejadores del diseñador sin uso (la búsqueda es en vivo y Refrescar lo maneja Listados)
        private void btnBuscar_Click(object sender, EventArgs e) { }
        private void btnRefrescar_Click(object sender, EventArgs e) { }
    }
}