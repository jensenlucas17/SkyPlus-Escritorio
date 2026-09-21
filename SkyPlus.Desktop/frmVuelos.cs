// SkyPlus.Desktop/frmVuelos.cs

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
    public partial class frmVuelos : Form
    {
        private readonly VueloClient _vueloClient =
            new VueloClient();

        private List<VueloResponse> _vuelosCache =
            new List<VueloResponse>();

        public frmVuelos()
        {
            InitializeComponent();
        }

        private async void frmVuelos_Load(
            object sender,
            EventArgs e)
        {
            ConfigurarColumnas();

            cmbEstadoFiltro.Items.Clear();

            cmbEstadoFiltro.Items.AddRange(
                new object[]
                {
                    "Todos",
                    "Programado",
                    "En embarque",
                    "En vuelo",
                    "Demorado",
                    "Cancelado",
                    "Finalizado"
                });

            cmbEstadoFiltro.SelectedIndex = 0;

            await CargarVuelos();
        }

        private void ConfigurarColumnas()
        {
            dgvVuelos.AutoGenerateColumns = false;
            dgvVuelos.Columns.Clear();

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.NumeroVuelo),
                    HeaderText = "N° Vuelo",
                    Width = 90
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.LugarOrigen),
                    HeaderText = "Origen",
                    Width = 130
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.LugarDestino),
                    HeaderText = "Destino",
                    Width = 130
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.Aeronave),
                    HeaderText = "Aeronave",
                    Width = 170
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.Salida),
                    HeaderText = "Salida",
                    Width = 120,
                    DefaultCellStyle =
                        new DataGridViewCellStyle
                        {
                            Format = "dd/MM/yyyy HH:mm"
                        }
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.Llegada),
                    HeaderText = "Llegada",
                    Width = 120,
                    DefaultCellStyle =
                        new DataGridViewCellStyle
                        {
                            Format = "dd/MM/yyyy HH:mm"
                        }
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.EstadoVuelo),
                    HeaderText = "Estado",
                    Width = 110
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.Tarifa),
                    HeaderText = "Tarifa",
                    Width = 100,
                    DefaultCellStyle =
                        new DataGridViewCellStyle
                        {
                            Format = "C0"
                        }
                });
        }

        private async Task CargarVuelos()
        {
            try
            {
                var vuelos =
                    await _vueloClient.ObtenerTodosAsync();

                _vuelosCache =
                    vuelos ??
                    new List<VueloResponse>();

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
                    "Ocurrió un error al cargar los vuelos:\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void AplicarFiltros()
        {
            var texto =
                txtBuscar.Text.Trim();

            var estadoFiltro =
                cmbEstadoFiltro.SelectedItem?.ToString()
                ?? "Todos";

            var filtrados =
                _vuelosCache
                    .Where(v =>
                        (
                            string.IsNullOrWhiteSpace(texto) ||
                            (v.NumeroVuelo ?? string.Empty)
                                .Contains(
                                    texto,
                                    StringComparison.OrdinalIgnoreCase) ||
                            (v.LugarOrigen ?? string.Empty)
                                .Contains(
                                    texto,
                                    StringComparison.OrdinalIgnoreCase) ||
                            (v.LugarDestino ?? string.Empty)
                                .Contains(
                                    texto,
                                    StringComparison.OrdinalIgnoreCase)
                        )
                        &&
                        (
                            estadoFiltro == "Todos" ||
                            v.EstadoVuelo == estadoFiltro
                        )
                    )
                    .ToList();

            dgvVuelos.DataSource = null;
            dgvVuelos.DataSource = filtrados;
        }

        private void txtBuscar_TextChanged(
            object sender,
            EventArgs e)
        {
            AplicarFiltros();
        }

        private void cmbEstadoFiltro_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            AplicarFiltros();
        }

        private async void btnRefrescar_Click(
            object sender,
            EventArgs e)
        {
            await CargarVuelos();
        }

        private VueloResponse? ObtenerSeleccionado()
        {
            if (dgvVuelos.CurrentRow?.DataBoundItem
                is VueloResponse vuelo)
            {
                return vuelo;
            }

            MessageBox.Show(
                "Seleccioná un vuelo de la lista primero.",
                "Atención",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return null;
        }

        private async void btnNuevo_Click(
            object sender,
            EventArgs e)
        {
            using var form =
                new frmVueloABM(
                    _vueloClient,
                    null);

            if (form.ShowDialog() ==
                DialogResult.OK)
            {
                await CargarVuelos();
            }
        }

        private async void btnEditar_Click(
            object sender,
            EventArgs e)
        {
            var seleccionado =
                ObtenerSeleccionado();

            if (seleccionado == null)
                return;

            using var form =
                new frmVueloABM(
                    _vueloClient,
                    seleccionado);

            if (form.ShowDialog() ==
                DialogResult.OK)
            {
                await CargarVuelos();
            }
        }

        private async void btnEliminar_Click(
            object sender,
            EventArgs e)
        {
            var seleccionado =
                ObtenerSeleccionado();

            if (seleccionado == null)
                return;

            // TODO: reemplazar DELETE fisico por baja logica (EstadoVuelo = "Cancelado")
            // para no romper reservas asociadas al vuelo. Pendiente definir con Lucas
            // el metodo de actualizacion real (ver charla sobre integridad referencial).
            var confirmacion =
                MessageBox.Show(
                    $"¿Confirmás eliminar el vuelo " +
                    $"{seleccionado.NumeroVuelo}?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (confirmacion !=
                DialogResult.Yes)
            {
                return;
            }

            try
            {
                await _vueloClient.EliminarAsync(
                    seleccionado.IdVuelo);

                MessageBox.Show(
                    "Vuelo eliminado correctamente.",
                    "Éxito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await CargarVuelos();
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(
                    "No se pudo conectar con la API.\n\n" +
                    "Verificá que SkyPlus.API esté ejecutándose.",
                    "Error de conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo eliminar el vuelo:\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
 }
