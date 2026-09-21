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
    public partial class frmAeronaves : Form
    {
        private readonly AeronaveClient _aeronaveClient =
            new AeronaveClient();

        private List<AeronaveResponse> _aeronaves =
            new List<AeronaveResponse>();

        public frmAeronaves()
        {
            InitializeComponent();
        }

        private async void frmAeronaves_Load(
            object sender,
            EventArgs e)
        {
            await CargarAeronaves();
        }

        private async Task CargarAeronaves()
        {
            try
            {
                var aeronaves =
                    await _aeronaveClient.ObtenerTodosAsync();

                _aeronaves = aeronaves ??
                    new List<AeronaveResponse>();

                dgvAeronaves.DataSource = null;
                dgvAeronaves.DataSource = _aeronaves;
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(
                    "No se pudo conectar con la API de SkyPlus.",
                    "Error de conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error al cargar las aeronaves.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void btnNuevo_Click(
            object sender,
            EventArgs e)
        {
            using (var formulario =
                new frmAeronaveABM(
                    _aeronaveClient,
                    null))
            {
                if (formulario.ShowDialog() ==
                    DialogResult.OK)
                {
                    await CargarAeronaves();
                }
            }
        }

        private async void btnEditar_Click(
            object sender,
            EventArgs e)
        {
            if (dgvAeronaves.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione una aeronave.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            var aeronave =
                dgvAeronaves.CurrentRow.DataBoundItem
                as AeronaveResponse;

            if (aeronave == null)
                return;

            using (var formulario =
                new frmAeronaveABM(
                    _aeronaveClient,
                    aeronave))
            {
                if (formulario.ShowDialog() ==
                    DialogResult.OK)
                {
                    await CargarAeronaves();
                }
            }
        }

        private async void btnEliminar_Click(
            object sender,
            EventArgs e)
        {
            if (dgvAeronaves.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione una aeronave.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            var aeronave =
                dgvAeronaves.CurrentRow.DataBoundItem
                as AeronaveResponse;

            if (aeronave == null)
                return;

            var respuesta = MessageBox.Show(
                $"¿Desea eliminar la aeronave " +
                $"{aeronave.Matricula}?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            try
            {
                await _aeronaveClient
                    .EliminarAsync(aeronave.IdAeronave);

                MessageBox.Show(
                    "Aeronave eliminada correctamente.",
                    "Éxito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await CargarAeronaves();
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(
                    "No se pudo conectar con la API.",
                    "Error de conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo eliminar la aeronave.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void btnActualizar_Click(
            object sender,
            EventArgs e)
        {
            await CargarAeronaves();
        }
    }
}