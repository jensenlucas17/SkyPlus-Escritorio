using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;

namespace SkyPlus.Desktop
{
    public partial class frmUsuarios : Form
    {
        private readonly UsuarioClient _usuarioClient = new UsuarioClient();
        private List<UsuarioResponse> _usuariosCache = new();

        public frmUsuarios()
        {
            InitializeComponent();
        }

        private void frmUsuarios_Load(object sender, EventArgs e)
        {
            ConfigurarColumnas();

            cmbEstadoFiltro.Items.AddRange(new object[] { "Todos", "Activo", "Inactivo" });
            cmbEstadoFiltro.SelectedIndex = 0;

            _ = CargarUsuariosAsync();
        }

        private void ConfigurarColumnas()
        {
            dgvUsuarios.Columns.Clear();
            dgvUsuarios.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(UsuarioResponse.Nombre), HeaderText = "Nombre", Width = 120 });
            dgvUsuarios.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(UsuarioResponse.Apellido), HeaderText = "Apellido", Width = 120 });
            dgvUsuarios.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(UsuarioResponse.EmailCorporativo), HeaderText = "Email corporativo", Width = 200 });
            dgvUsuarios.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(UsuarioResponse.Rol), HeaderText = "Rol", Width = 130 });
            dgvUsuarios.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(UsuarioResponse.EstadoCuenta), HeaderText = "Estado", Width = 80 });
            dgvUsuarios.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(UsuarioResponse.UltimaSesion), HeaderText = "Última sesión", Width = 140 });
        }

        private async System.Threading.Tasks.Task CargarUsuariosAsync()
        {
            try
            {
                var usuarios = await _usuarioClient.ObtenerTodosAsync();
                _usuariosCache = usuarios ?? new List<UsuarioResponse>();
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar los usuarios: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarFiltros()
        {
            var texto = txtBuscar.Text.Trim().ToLower();
            var estadoFiltro = cmbEstadoFiltro.SelectedItem?.ToString() ?? "Todos";

            var filtrados = _usuariosCache.Where(u =>
                (string.IsNullOrEmpty(texto) ||
                    u.Nombre.ToLower().Contains(texto) ||
                    u.Apellido.ToLower().Contains(texto) ||
                    u.EmailCorporativo.ToLower().Contains(texto))
                &&
                (estadoFiltro == "Todos" || u.EstadoCuenta == estadoFiltro)
            ).ToList();

            dgvUsuarios.DataSource = filtrados;
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e) => AplicarFiltros();

        private void cmbEstadoFiltro_SelectedIndexChanged(object sender, EventArgs e) => AplicarFiltros();

        private void btnRefrescar_Click(object sender, EventArgs e) => _ = CargarUsuariosAsync();

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            using var form = new frmUsuarioABM();
            if (form.ShowDialog() == DialogResult.OK)
            {
                _ = CargarUsuariosAsync();
            }
        }

        private UsuarioResponse? ObtenerSeleccionado()
        {
            if (dgvUsuarios.CurrentRow?.DataBoundItem is UsuarioResponse usuario)
                return usuario;

            MessageBox.Show("Seleccioná un usuario de la lista primero.",
                "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            var seleccionado = ObtenerSeleccionado();
            if (seleccionado == null) return;

            using var form = new frmUsuarioABM(seleccionado);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _ = CargarUsuariosAsync();
            }
        }

        private async void btnDesactivarReactivar_Click(object sender, EventArgs e)
        {
            var seleccionado = ObtenerSeleccionado();
            if (seleccionado == null) return;

            bool estaActivo = seleccionado.EstadoCuenta == "Activo";
            string nuevoEstado = estaActivo ? "Inactivo" : "Activo";
            string accion = estaActivo ? "desactivar" : "reactivar";

            var confirmacion = MessageBox.Show(
                $"¿Confirmás {accion} a {seleccionado.Nombre} {seleccionado.Apellido}?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                var request = new ActualizarUsuarioRequest
                {
                    IdRol = seleccionado.IdRol,
                    Nombre = seleccionado.Nombre,
                    Apellido = seleccionado.Apellido,
                    EmailCorporativo = seleccionado.EmailCorporativo,
                    EstadoCuenta = nuevoEstado
                };

                await _usuarioClient.ActualizarAsync(seleccionado.IdUsuario, request);
                await CargarUsuariosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo actualizar el estado: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnEliminar_Click(object sender, EventArgs e)
        {
            var seleccionado = ObtenerSeleccionado();
            if (seleccionado == null) return;

            var confirmacion = MessageBox.Show(
                $"Esto elimina PERMANENTEMENTE a {seleccionado.Nombre} {seleccionado.Apellido}, sin posibilidad de recuperarlo. ¿Continuar?",
                "Eliminar definitivamente", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                await _usuarioClient.EliminarAsync(seleccionado.IdUsuario);
                await CargarUsuariosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtBuscar_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void cmbEstadoFiltro_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void btnRefrescar_Click_1(object sender, EventArgs e)
        {

        }

        private void btnNuevo_Click_1(object sender, EventArgs e)
        {

        }

        private void btnEditar_Click_1(object sender, EventArgs e)
        {

        }

        private void btnDesactivarReactivar_Click_1(object sender, EventArgs e)
        {

        }

        private void btnEliminar_Click_1(object sender, EventArgs e)
        {

        }
    }
}