// SkyPlus.Desktop/frmVuelos.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmVuelos : Form
    {
        private readonly VueloClientFake _vueloClient = new VueloClientFake();
        private List<VueloResponse> _vuelosCache = new();

        public frmVuelos()
        {
            InitializeComponent();
        }

        private void frmVuelos_Load(object sender, EventArgs e)
        {
            ConfigurarColumnas();

            cmbEstadoFiltro.Items.Clear();
            cmbEstadoFiltro.Items.AddRange(new object[] { "Todos", "Programado", "Confirmado", "Cancelado", "Finalizado" });
            cmbEstadoFiltro.SelectedIndex = 0;

            CargarVuelos();
        }

        private void ConfigurarColumnas()
        {
            dgvVuelos.AutoGenerateColumns = false;
            dgvVuelos.Columns.Clear();
            dgvVuelos.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(VueloResponse.NumeroVuelo), HeaderText = "N° Vuelo", Width = 90 });
            dgvVuelos.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(VueloResponse.LugarOrigen), HeaderText = "Origen", Width = 130 });
            dgvVuelos.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(VueloResponse.LugarDestino), HeaderText = "Destino", Width = 130 });
            dgvVuelos.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(VueloResponse.Aeronave), HeaderText = "Aeronave", Width = 170 });
            dgvVuelos.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(VueloResponse.Salida), HeaderText = "Salida", Width = 120 });
            dgvVuelos.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(VueloResponse.Llegada), HeaderText = "Llegada", Width = 120 });
            dgvVuelos.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(VueloResponse.EstadoVuelo), HeaderText = "Estado", Width = 100 });
            dgvVuelos.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(VueloResponse.Tarifa), HeaderText = "Tarifa", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "C0" } });
        }

        private void CargarVuelos()
        {
            _vuelosCache = _vueloClient.ObtenerTodos();
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            var texto = txtBuscar.Text.Trim().ToLower();
            var estadoFiltro = cmbEstadoFiltro.SelectedItem?.ToString() ?? "Todos";

            var filtrados = _vuelosCache.Where(v =>
                (string.IsNullOrEmpty(texto) || v.NumeroVuelo.ToLower().Contains(texto))
                && (estadoFiltro == "Todos" || v.EstadoVuelo == estadoFiltro)
            ).ToList();

            dgvVuelos.DataSource = filtrados;
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e) => AplicarFiltros();
        private void cmbEstadoFiltro_SelectedIndexChanged(object sender, EventArgs e) => AplicarFiltros();
        private void btnRefrescar_Click(object sender, EventArgs e) => CargarVuelos();

        private VueloResponse? ObtenerSeleccionado()
        {
            if (dgvVuelos.CurrentRow?.DataBoundItem is VueloResponse vuelo)
                return vuelo;

            MessageBox.Show("Seleccioná un vuelo de la lista primero.",
                "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            using var form = new frmVueloABM();
            if (form.ShowDialog() == DialogResult.OK) CargarVuelos();
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            var seleccionado = ObtenerSeleccionado();
            if (seleccionado == null) return;

            using var form = new frmVueloABM(seleccionado);
            if (form.ShowDialog() == DialogResult.OK) CargarVuelos();
        }

        private void btnCancelarVuelo_Click(object sender, EventArgs e)
        {
            var seleccionado = ObtenerSeleccionado();
            if (seleccionado == null) return;

            if (seleccionado.EstadoVuelo == "Cancelado")
            {
                MessageBox.Show("Este vuelo ya está cancelado.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Confirmás cancelar el vuelo {seleccionado.NumeroVuelo}? Esto no se puede deshacer y afecta a las reservas asociadas.",
                "Confirmar cancelación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes) return;

            seleccionado.EstadoVuelo = "Cancelado";
            _vueloClient.Actualizar(seleccionado);
            CargarVuelos();
        }
    }
}