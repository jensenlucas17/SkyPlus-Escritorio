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
            AppEstilos.EstilizarGrid(dgvLugares);
            AppEstilos.EstilizarBotonPrimario(btnNuevo);
            AppEstilos.EstilizarBotonSecundario(btnEditar);
            AppEstilos.EstilizarBotonSecundario(btnEliminar);
            AppEstilos.EstilizarBotonSecundario(btnReactivar);

            _cmbPais = Listados.Combo(160, Array.Empty<string>(), AplicarFiltros);
            _cmbEstado = Listados.Combo(130, new[] { "Activo", "Inactivo" }, AplicarFiltros);
            Listados.ArmarFiltros(pnlFiltros, txtBuscar, btnRefrescar,
                "Código IATA, aeropuerto o ciudad",
                ("País", _cmbPais),
                ("Estado", _cmbEstado));
            Listados.ConectarBusqueda(txtBuscar, btnRefrescar, AplicarFiltros, CargarLugares, () =>
            {
                _cmbPais.SelectedIndex = 0;
                _cmbEstado.SelectedIndex = 0;
            });
            _lblResumen = Listados.CrearResumen(this, dgvLugares);

            await CargarLugares();
        }

        private ComboBox _cmbPais = null!;
        private ComboBox _cmbEstado = null!;
        private Label _lblResumen = null!;

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

            dgvLugares.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(LugarResponse.Estado),
                HeaderText = "Estado",
                Width = 100
            });
        }

        private async Task CargarLugares()
        {
            try
            {
                var lugares = await _lugarClient.ObtenerTodosAsync();

                _lugaresCache = lugares ?? new List<LugarResponse>();

                Listados.CargarOpciones(_cmbPais, _lugaresCache.Select(l => l.Pais).OrderBy(p => p));
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
            if (_lblResumen == null) return;

            var texto = txtBuscar.Text.Trim();

            var filtrados = _lugaresCache
                .Where(l => AppEstilos.Coincide(texto, l.CodigoIata, l.Nombre, l.Ciudad, l.Pais))
                .Where(l => Listados.Pasa(_cmbPais, l.Pais))
                .Where(l => Listados.Pasa(_cmbEstado, l.Estado))
                .ToList();

            dgvLugares.DataSource = filtrados;

            _lblResumen.Text =
                $"{filtrados.Count} lugares   |   {filtrados.Count(l => l.Estado == "Activo")} activos   |   " +
                $"{filtrados.Select(l => l.Pais).Distinct().Count()} países";
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        // Botón Refrescar (lo maneja Listados.ConectarBusqueda)
        private void btnRefrescar_Click(object sender, EventArgs e)
        {
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

            if (seleccionado.Estado == "Inactivo")
            {
                MessageBox.Show(
                    "Este aeropuerto ya se encuentra inactivo.",
                    "Atención",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Confirmás dar de baja el aeropuerto " +
                $"{seleccionado.CodigoIata} - {seleccionado.Nombre}?\n\n" +
                "El aeropuerto quedará inactivo, pero conservará su historial.",
                "Confirmar baja",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                await _lugarClient.EliminarAsync(seleccionado.IdLugar);

                MessageBox.Show(
                    "Aeropuerto dado de baja correctamente.",
                    "Operación exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await CargarLugares();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "No se pudo completar la baja del aeropuerto.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void btnReactivar_Click(object sender, EventArgs e)
        {
            var seleccionado = ObtenerSeleccionado();

            if (seleccionado == null)
                return;

            if (seleccionado.Estado == "Activo")
            {
                MessageBox.Show(
                    "Este aeropuerto ya se encuentra activo.",
                    "Atención",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Confirmás reactivar el aeropuerto " +
                $"{seleccionado.CodigoIata} - {seleccionado.Nombre}?",
                "Confirmar reactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                await _lugarClient.ReactivarAsync(seleccionado.IdLugar);

                MessageBox.Show(
                    "Aeropuerto reactivado correctamente.",
                    "Operación exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await CargarLugares();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "No se pudo reactivar el aeropuerto.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}