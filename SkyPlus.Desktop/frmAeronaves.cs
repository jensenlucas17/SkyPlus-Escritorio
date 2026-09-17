// SkyPlus.Desktop/frmAeronaves.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;

namespace SkyPlus.Desktop
{
    public partial class frmAeronaves : Form
    {
        private readonly AeronaveClientFake _aeronaveClient = new AeronaveClientFake();
        private List<AeronaveResponse> _aeronavesCache = new();

        public frmAeronaves()
        {
            InitializeComponent();
        }

        private void frmAeronaves_Load(object sender, EventArgs e)
        {
            ConfigurarColumnas();
            CargarAeronaves();
        }

        private void ConfigurarColumnas()
        {
            dgvAeronaves.AutoGenerateColumns = false;
            dgvAeronaves.Columns.Clear();
            dgvAeronaves.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AeronaveResponse.Matricula), HeaderText = "Matrícula", Width = 150 });
            dgvAeronaves.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AeronaveResponse.Modelo), HeaderText = "Modelo", Width = 250 });
        }

        private void CargarAeronaves()
        {
            _aeronavesCache = _aeronaveClient.ObtenerTodos();
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            var texto = txtBuscar.Text.Trim().ToLower();

            var filtrados = _aeronavesCache.Where(a =>
                string.IsNullOrEmpty(texto) ||
                a.Matricula.ToLower().Contains(texto) ||
                a.Modelo.ToLower().Contains(texto)
            ).ToList();

            dgvAeronaves.DataSource = filtrados;
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e) => AplicarFiltros();

        private void btnRefrescar_Click(object sender, EventArgs e) => CargarAeronaves();

        private AeronaveResponse? ObtenerSeleccionado()
        {
            if (dgvAeronaves.CurrentRow?.DataBoundItem is AeronaveResponse aeronave)
                return aeronave;

            MessageBox.Show("Seleccioná una aeronave de la lista primero.",
                "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            using var form = new frmAeronaveABM();
            if (form.ShowDialog() == DialogResult.OK) CargarAeronaves();
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            var seleccionado = ObtenerSeleccionado();
            if (seleccionado == null) return;

            using var form = new frmAeronaveABM(seleccionado);
            if (form.ShowDialog() == DialogResult.OK) CargarAeronaves();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            var seleccionado = ObtenerSeleccionado();
            if (seleccionado == null) return;

            var confirmacion = MessageBox.Show(
                $"¿Confirmás eliminar la aeronave {seleccionado.Matricula}?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes) return;

            _aeronaveClient.Eliminar(seleccionado.IdAeronave);
            CargarAeronaves();
        }
    }
}