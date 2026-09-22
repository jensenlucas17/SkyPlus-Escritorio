// SkyPlus.Desktop/frmReservas.cs

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
    public partial class frmReservas : Form
    {
        private readonly ReservaClient _reservaClient = new ReservaClient();
        private List<ReservaResponse> _reservasCache = new List<ReservaResponse>();

        public frmReservas()
        {
            InitializeComponent();
        }

        private async void frmReservas_Load(object sender, EventArgs e)
        {
            await CargarReservas();

            AppEstilos.EstilizarGrid(dgvReservas);
            AppEstilos.EstilizarBotonPrimario(btnNuevo);
            AppEstilos.EstilizarBotonSecundario(btnCancelarReserva);
            AppEstilos.EstilizarBotonSecundario(btnBuscar);
        }

        private async Task CargarReservas()
        {
            try
            {
                var reservas = await _reservaClient.ObtenerTodosAsync();
                _reservasCache = reservas ?? new List<ReservaResponse>();
                AplicarFiltro();
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("No se pudo conectar con la API de SkyPlus.",
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al cargar las reservas.\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarFiltro()
        {
            var texto = txtBuscar.Text.Trim().ToLower();

            var filtradas = _reservasCache.Where(r =>
                string.IsNullOrEmpty(texto) ||
                r.CodigoReserva.ToLower().Contains(texto) ||
                r.Pasajero.ToLower().Contains(texto)
            ).ToList();

            dgvReservas.DataSource = null;
            dgvReservas.DataSource = filtradas;
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private async void btnBuscar_Click(object sender, EventArgs e)
        {
            await CargarReservas();
        }

        private async void btnNuevo_Click(object sender, EventArgs e)
        {
            using (var formulario = new frmReservaABM())
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    await CargarReservas();
                }
            }
        }

        private async void btnCancelarReserva_Click(object sender, EventArgs e)
        {
            if (dgvReservas.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una reserva.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var reserva = dgvReservas.CurrentRow.DataBoundItem as ReservaResponse;
            if (reserva == null) return;

            if (reserva.EstadoReserva == "Cancelada")
            {
                MessageBox.Show("Esta reserva ya está cancelada.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var respuesta = MessageBox.Show(
                $"¿Confirmás cancelar la reserva {reserva.CodigoReserva}?",
                "Confirmar cancelación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (respuesta != DialogResult.Yes) return;

            try
            {
                await _reservaClient.CancelarAsync(reserva.IdReserva);
                MessageBox.Show("Reserva cancelada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await CargarReservas();
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("No se pudo conectar con la API.", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo cancelar la reserva.\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}