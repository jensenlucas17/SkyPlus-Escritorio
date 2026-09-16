// SkyPlus.Desktop/frmLugares.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;

namespace SkyPlus.Desktop
{
    public partial class frmLugares : Form
    {
        // TODO: reemplazar por LugarClient real cuando exista
        private readonly LugarClientFake _lugarClient = new LugarClientFake();
        private List<LugarResponse> _lugaresCache = new();

        public frmLugares()
        {
            InitializeComponent();
        }

        private void frmLugares_Load(object sender, EventArgs e)
        {
            ConfigurarColumnas();
            CargarLugares();
        }

        private void ConfigurarColumnas()
        {
            dgvLugares.AutoGenerateColumns = false;
            dgvLugares.Columns.Clear();
            dgvLugares.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(LugarResponse.CodigoIata), HeaderText = "IATA", Width = 60 });
            dgvLugares.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(LugarResponse.Nombre), HeaderText = "Aeropuerto", Width = 280 });
            dgvLugares.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(LugarResponse.Ciudad), HeaderText = "Ciudad", Width = 140 });
            dgvLugares.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(LugarResponse.Pais), HeaderText = "País", Width = 120 });
        }

        private void CargarLugares()
        {
            _lugaresCache = _lugarClient.ObtenerTodos();
            AplicarFiltros();
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

        private void txtBuscar_TextChanged(object sender, EventArgs e) => AplicarFiltros();

        private void btnRefrescar_Click(object sender, EventArgs e) => CargarLugares();

        private LugarResponse? ObtenerSeleccionado()
        {
            if (dgvLugares.CurrentRow?.DataBoundItem is LugarResponse lugar)
                return lugar;

            MessageBox.Show("Seleccioná un aeropuerto de la lista primero.",
                "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            using var form = new frmLugarABM();
            if (form.ShowDialog() == DialogResult.OK) CargarLugares();
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            var seleccionado = ObtenerSeleccionado();
            if (seleccionado == null) return;

            using var form = new frmLugarABM(seleccionado);
            if (form.ShowDialog() == DialogResult.OK) CargarLugares();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            var seleccionado = ObtenerSeleccionado();
            if (seleccionado == null) return;

            var confirmacion = MessageBox.Show(
                $"¿Confirmás eliminar {seleccionado.CodigoIata} - {seleccionado.Nombre}?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes) return;

            _lugarClient.Eliminar(seleccionado.IdLugar);
            CargarLugares();
        }
    }
}