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
        private List<ReservaResponse> _canceladasCache = new List<ReservaResponse>();

        public frmCancelaciones()
        {
            InitializeComponent();
        }

        private async void frmCancelaciones_Load(object sender, EventArgs e)
        {
            ConfigurarColumnas();
            await CargarCancelaciones();

            AppEstilos.EstilizarGrid(dgvCancelaciones);
            AppEstilos.EstilizarBotonPrimario(btnBuscar);
            AppEstilos.EstilizarBotonSecundario(btnRefrescar);
        }

        private void ConfigurarColumnas()
        {
            dgvCancelaciones.AutoGenerateColumns = false;
            dgvCancelaciones.Columns.Clear();
            dgvCancelaciones.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReservaResponse.CodigoReserva), HeaderText = "Reserva", Width = 90 });
            dgvCancelaciones.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReservaResponse.Pasajero), HeaderText = "Pasajero", Width = 180 });
            dgvCancelaciones.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReservaResponse.Vuelo), HeaderText = "Vuelo", Width = 100 });
            dgvCancelaciones.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReservaResponse.Asiento), HeaderText = "Asiento", Width = 80 });
            dgvCancelaciones.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReservaResponse.TarifaBase), HeaderText = "Tarifa", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "C0" } });
            dgvCancelaciones.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReservaResponse.FechaCancelacion), HeaderText = "Fecha cancelación", Width = 140, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
        }

        private async Task CargarCancelaciones()
        {
            try
            {
                var reservas = await _reservaClient.ObtenerTodosAsync();

                _canceladasCache = (reservas ?? new List<ReservaResponse>())
                    .Where(r => r.EstadoReserva == "Cancelada")
                    .ToList();

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
            var texto = txtBuscar.Text.Trim().ToLower();

            var filtradas = _canceladasCache.Where(r =>
                string.IsNullOrEmpty(texto) ||
                r.CodigoReserva.ToLower().Contains(texto) ||
                r.Pasajero.ToLower().Contains(texto)
            ).ToList();

            dgvCancelaciones.DataSource = null;
            dgvCancelaciones.DataSource = filtradas;
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private async void btnRefrescar_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            await CargarCancelaciones();
        }
    }
}