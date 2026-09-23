using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;

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
            AppEstilos.EstilizarGrid(dgvUsuarios);
            AppEstilos.EstilizarBotonPrimario(btnNuevo);
            AppEstilos.EstilizarBotonSecundario(btnEditar);
            AppEstilos.EstilizarBotonSecundario(btnRefrescar);
            AppEstilos.EstilizarBotonSecundario(btnDesactivarReactivar);
            cmbEstadoFiltro.Items.Clear();
            cmbEstadoFiltro.Items.AddRange(new object[]
            {
                "Todos",
                "Activo",
                "Inactivo"
            });
            cmbEstadoFiltro.SelectedIndex = 0;

            _ = CargarRolesFiltroAsync();
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
            var rolFiltro = cmbRolFiltro.SelectedItem?.ToString() ?? "Todos";

            var filtrados = _usuariosCache.Where(u =>
                (string.IsNullOrEmpty(texto) ||
                    u.Nombre.ToLower().Contains(texto) ||
                    u.Apellido.ToLower().Contains(texto) ||
                    u.EmailCorporativo.ToLower().Contains(texto))
                &&
                (estadoFiltro == "Todos" || u.EstadoCuenta == estadoFiltro)
                &&
                (rolFiltro == "Todos" || u.Rol == rolFiltro)
            ).ToList();

            dgvUsuarios.DataSource = filtrados;
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e) => AplicarFiltros();

        private void cmbEstadoFiltro_SelectedIndexChanged(object sender, EventArgs e) => AplicarFiltros();

        private async void btnRefrescar_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();

            if (cmbEstadoFiltro.Items.Count > 0)
                cmbEstadoFiltro.SelectedIndex = 0;

            if (cmbRolFiltro.Items.Count > 0)
                cmbRolFiltro.SelectedIndex = 0;

            await CargarUsuariosAsync();
        }

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

        private async System.Threading.Tasks.Task CargarRolesFiltroAsync()
        {
            try
            {
                var rolClient = new RolClient();
                var roles = await rolClient.ObtenerTodosAsync();

                cmbRolFiltro.Items.Clear();
                cmbRolFiltro.Items.Add("Todos");

                if (roles != null)
                {
                    foreach (var rol in roles)
                    {
                        cmbRolFiltro.Items.Add(rol.NombreRol);
                    }
                }

                cmbRolFiltro.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron cargar los roles: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void cmbRolFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void btnEliminar_Click_1(object sender, EventArgs e)
        {

        }
    }
}