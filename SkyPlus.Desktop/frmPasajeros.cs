// SkyPlus.Desktop/frmPasajeros.cs

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmPasajeros : Form
    {
        private readonly PasajeroClient _pasajeroClient = new PasajeroClient();
        private List<PasajeroResponse> _pasajeros = new List<PasajeroResponse>();

        public frmPasajeros()
        {
            InitializeComponent();
        }

        private async void frmPasajeros_Load(object sender, EventArgs e)
        {
            await CargarPasajeros();

            AppEstilos.EstilizarGrid(dgvPasajeros);
            AppEstilos.EstilizarBotonPrimario(btnNuevo);
            AppEstilos.EstilizarBotonSecundario(btnEditar);
            AppEstilos.EstilizarBotonSecundario(btnEliminar);
            AppEstilos.EstilizarBotonSecundario(btnBuscar);
        }

        private async Task CargarPasajeros()
        {
            try
            {
                var pasajeros = await _pasajeroClient.ObtenerTodosAsync();
                _pasajeros = pasajeros ?? new List<PasajeroResponse>();

                dgvPasajeros.DataSource = null;
                dgvPasajeros.DataSource = _pasajeros;
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("No se pudo conectar con la API de SkyPlus.",
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al cargar los pasajeros.\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnBuscar_Click(object sender, EventArgs e)
        {
            await CargarPasajeros();
        }

        private async void btnNuevo_Click(object sender, EventArgs e)
        {
            using (var formulario = new frmPasajeroABM(_pasajeroClient, null))
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    await CargarPasajeros();
                }
            }
        }

        private async void btnEditar_Click(object sender, EventArgs e)
        {
            if (dgvPasajeros.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un pasajero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var pasajero = dgvPasajeros.CurrentRow.DataBoundItem as PasajeroResponse;
            if (pasajero == null) return;

            using (var formulario = new frmPasajeroABM(_pasajeroClient, pasajero))
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    await CargarPasajeros();
                }
            }
        }

        private async void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvPasajeros.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un pasajero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var pasajero = dgvPasajeros.CurrentRow.DataBoundItem as PasajeroResponse;
            if (pasajero == null) return;

            var respuesta = MessageBox.Show(
                $"¿Desea eliminar a {pasajero.NombreCompleto}?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes) return;

            try
            {
                await _pasajeroClient.EliminarAsync(pasajero.IdPasajero);
                MessageBox.Show("Pasajero eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await CargarPasajeros();
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("No se pudo conectar con la API.", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar el pasajero.\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}