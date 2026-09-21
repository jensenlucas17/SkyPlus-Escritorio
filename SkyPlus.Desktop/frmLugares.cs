// SkyPlus.Desktop/frmLugares.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmLugares : Form
    {
        private readonly LugarClient _lugarClient = new LugarClient();
        private List<LugarResponse> _lugaresCache = new();

        public frmLugares()
        {
            InitializeComponent();
        }

        private async void frmLugares_Load(object sender, EventArgs e)
        {
            ConfigurarColumnas();
            await CargarLugares();
            AppEstilos.EstilizarGrid(dgvLugares);
            AppEstilos.EstilizarBotonPrimario(btnNuevo);
            AppEstilos.EstilizarBotonSecundario(btnEditar);
            AppEstilos.EstilizarBotonSecundario(btnEliminar);
            AppEstilos.EstilizarBotonSecundario(btnRefrescar);
        }

        private void ConfigurarColumnas()
        {
            dgvLugares.AutoGenerateColumns = false;
            dgvLugares.Columns.Clear();

            dgvLugares.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(LugarResponse.CodigoIata),
                HeaderText = "IATA",
                Width = 60
            });

            dgvLugares.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(LugarResponse.Nombre),
                HeaderText = "Aeropuerto",
                Width = 280
            });

            dgvLugares.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(LugarResponse.Ciudad),
                HeaderText = "Ciudad",
                Width = 140
            });

            dgvLugares.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(LugarResponse.Pais),
                HeaderText = "País",
                Width = 120
            });
        }

        private async Task CargarLugares()
        {
            try
            {
                var lugares = await _lugarClient.ObtenerTodosAsync();

                _lugaresCache = lugares ?? new List<LugarResponse>();

                AplicarFiltros();
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(
                    "No se pudo conectar con la API de SkyPlus.\n\n" +
                    "Verificá que SkyPlus.API esté ejecutándose.",
                    "Error de conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error al cargar los aeropuertos:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void AplicarFiltros()
        {
            var texto = txtBuscar.Text.Trim().ToLower();

            var filtrados = _lugaresCache.Where(l =>
                string.IsNullOrEmpty(texto) ||
                l.CodigoIata.ToLower().Contains(texto) ||
                l.Nombre.ToLower().Contains(texto) ||
                l.Ciudad.ToLower().Contains(texto) ||
                l.Pais.ToLower().Contains(texto)
            ).ToList();

            dgvLugares.DataSource = filtrados;
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private async void btnRefrescar_Click(object sender, EventArgs e)
        {
            await CargarLugares();
        }

        private LugarResponse? ObtenerSeleccionado()
        {
            if (dgvLugares.CurrentRow?.DataBoundItem is LugarResponse lugar)
                return lugar;

            MessageBox.Show(
                "Seleccioná un aeropuerto de la lista primero.",
                "Atención",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return null;
        }

        private async void btnNuevo_Click(object sender, EventArgs e)
        {
            using var form = new frmLugarABM();

            if (form.ShowDialog() == DialogResult.OK)
            {
                await CargarLugares();
            }
        }

        private async void btnEditar_Click(object sender, EventArgs e)
        {
            var seleccionado = ObtenerSeleccionado();

            if (seleccionado == null)
                return;

            using var form = new frmLugarABM(seleccionado);

            if (form.ShowDialog() == DialogResult.OK)
            {
                await CargarLugares();
            }
        }

        private async void btnEliminar_Click(object sender, EventArgs e)
        {
            var seleccionado = ObtenerSeleccionado();

            if (seleccionado == null)
                return;

            var confirmacion = MessageBox.Show(
                $"¿Confirmás eliminar {seleccionado.CodigoIata} - {seleccionado.Nombre}?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                await _lugarClient.EliminarAsync(seleccionado.IdLugar);

                await CargarLugares();
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
                    "Ocurrió un error al eliminar el aeropuerto:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}