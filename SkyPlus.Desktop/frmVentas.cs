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
        private List<VentaFila> _ventas = new List<VentaFila>();

        private class VentaFila
        {
            public int IdPago { get; set; }
            public int IdReserva { get; set; }
            public int IdUsuarioAgente { get; set; }
            public string CodigoReserva { get; set; } = string.Empty;
            public string Pasajero { get; set; } = string.Empty;
            public string Vuelo { get; set; } = string.Empty;
            public string Asiento { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public string MetodoPago { get; set; } = string.Empty;
            public DateTime FechaPago { get; set; }
            public string Fecha => FechaPago.ToString("dd/MM/yyyy HH:mm");
            public string Estado { get; set; } = string.Empty;
        }

        public frmVentas()
        {
            InitializeComponent();
        }

        private async void frmVentas_Load(object sender, EventArgs e)
        {
            ConfigurarTabla();
            AppEstilos.EstilizarGrid(dgvVentas);
            AppEstilos.EstilizarBotonPrimario(btnBuscar);
            AppEstilos.EstilizarBotonSecundario(btnRefrescar);
            AppEstilos.EstilizarBotonSecundario(btnVerDetalle);
            var btnRegistrar = new Button
            {
                Name = "btnRegistrarPago",
                Text = "Registrar pago",
                Size = new System.Drawing.Size(130, 30),
                Location = new System.Drawing.Point(520, 10)
            };
            btnRegistrar.Click += btnRegistrarPago_Click;
            pnlBotones.Controls.Add(btnRegistrar);
            AppEstilos.EstilizarBotonPrimario(btnRegistrar);
            await CargarVentasAsync();
        }

        private async Task CargarVentasAsync()
        {
            try
            {
                var pagos = await _pagoClient.ObtenerTodosAsync();
                var reservas = await _reservaClient.ObtenerTodosAsync()
                    ?? new List<ReservaResponse>();
                var porId = reservas.ToDictionary(r => r.IdReserva);

                _ventas = pagos.Select(p =>
                {
                    porId.TryGetValue(p.IdReserva, out var reserva);
                    return new VentaFila
                    {
                        IdPago = p.IdPago,
                        IdReserva = p.IdReserva,
                        IdUsuarioAgente = p.IdUsuarioAgente,
                        CodigoReserva = p.CodigoReserva,
                        Pasajero = reserva?.Pasajero ?? string.Empty,
                        Vuelo = reserva?.Vuelo ?? string.Empty,
                        Asiento = reserva?.Asiento ?? string.Empty,
                        Monto = p.Monto,
                        MetodoPago = p.MetodoPago,
                        FechaPago = p.FechaPago,
                        Estado = p.EstadoPago
                    };
                }).OrderByDescending(v => v.FechaPago).ToList();
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
            AgregarColumna("colVuelo", "Vuelo", "Vuelo", 70);
            AgregarColumna("colAsiento", "Asiento", "Asiento", 70);
            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMonto",
                HeaderText = "Monto",
                DataPropertyName = "Monto",
                FillWeight = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
            });
            AgregarColumna("colMetodo", "Método de pago", "MetodoPago", 110);
            AgregarColumna("colFecha", "Fecha", "Fecha", 100);
            AgregarColumna("colEstado", "Estado", "Estado", 80);
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
            var texto = txtBuscar.Text.Trim();
            var resultado = _ventas.Where(v =>
                string.IsNullOrWhiteSpace(texto) ||
                v.CodigoReserva.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                v.Pasajero.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                v.Vuelo.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                v.MetodoPago.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                v.Estado.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .ToList();

            dgvVentas.DataSource = null;
            dgvVentas.DataSource = resultado;
            lblCantidad.Text = $"Ventas: {resultado.Count}";
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e) => AplicarFiltros();
        private void btnBuscar_Click(object sender, EventArgs e) => AplicarFiltros();

        private async void btnRefrescar_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            await CargarVentasAsync();
        }

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
                $"Monto: {venta.Monto:C2}\n" +
                $"Método: {venta.MetodoPago}\n" +
                $"Fecha: {venta.Fecha}\n" +
                $"Estado: {venta.Estado}\n" +
                $"Agente (ID): {venta.IdUsuarioAgente}",
                "Detalle de venta", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private async void btnRegistrarPago_Click(object? sender, EventArgs e)
        {
            using var formulario = new frmRegistrarPago();
            if (formulario.ShowDialog(this) == DialogResult.OK)
                await CargarVentasAsync();
        }
    }
}
