using System;
using System.Linq;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmUsuarioABM : Form
    {
        private readonly UsuarioClient _usuarioClient = new UsuarioClient();
        private readonly RolClient _rolClient = new RolClient();

        private readonly int? _idUsuarioEdicion; // null = alta, con valor = edición

        // Constructor para ALTA
        public frmUsuarioABM()
        {
            InitializeComponent();
            _idUsuarioEdicion = null;
        }

        // Constructor para EDICIÓN
        public frmUsuarioABM(UsuarioResponse usuarioExistente)
        {
            InitializeComponent();
            _idUsuarioEdicion = usuarioExistente.IdUsuario;

            txtNombre.Text = usuarioExistente.Nombre;
            txtApellido.Text = usuarioExistente.Apellido;
            txtEmail.Text = usuarioExistente.EmailCorporativo;
            cmbEstado.SelectedItem = usuarioExistente.EstadoCuenta;

            // El rol se termina de fijar en frmUsuarioABM_Load, una vez cargado el combo
            Tag = usuarioExistente.IdRol;
        }

        private async void frmUsuarioABM_Load(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            bool esEdicion = _idUsuarioEdicion.HasValue;

            Text = esEdicion ? "Editar usuario" : "Nuevo usuario";

            // Password solo aplica al alta (RF#01); en edición se resetea aparte
            lblPassword.Visible = !esEdicion;
            txtPassword.Visible = !esEdicion;

            // Estado solo tiene sentido en edición (un usuario nuevo nace Activo)
            lblEstado.Visible = esEdicion;
            cmbEstado.Visible = esEdicion;

            await CargarRolesAsync();

            AppEstilos.EstilizarBotonPrimario(btnGuardar);
            AppEstilos.EstilizarBotonSecundario(btnCancelar);
        }

        private async System.Threading.Tasks.Task CargarRolesAsync()
        {
            try
            {
                var roles = await _rolClient.ObtenerTodosAsync();

                cmbRol.DisplayMember = nameof(RolResponse.NombreRol);
                cmbRol.ValueMember = nameof(RolResponse.IdRol);
                cmbRol.DataSource = roles;

                // Si es edición, seleccionar el rol que ya tenía el usuario
                if (Tag is int idRolActual)
                {
                    cmbRol.SelectedValue = idRolActual;
                }
            }
            catch (Exception ex)
            {
                MostrarError("No se pudieron cargar los roles: " + ex.Message);
            }
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            // Validaciones básicas (RF#18)
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MostrarError("Nombre, apellido y email son obligatorios.");
                return;
            }

            if (!txtEmail.Text.Contains("@"))
            {
                MostrarError("Ingresá un email corporativo válido.");
                return;
            }

            if (cmbRol.SelectedValue is not int idRolSeleccionado)
            {
                MostrarError("Seleccioná un rol.");
                return;
            }

            btnGuardar.Enabled = false;

            try
            {
                if (_idUsuarioEdicion.HasValue)
                {
                    // EDICIÓN
                    var request = new ActualizarUsuarioRequest
                    {
                        IdRol = idRolSeleccionado,
                        Nombre = txtNombre.Text.Trim(),
                        Apellido = txtApellido.Text.Trim(),
                        EmailCorporativo = txtEmail.Text.Trim(),
                        EstadoCuenta = cmbEstado.SelectedItem?.ToString() ?? "Activo"
                    };

                    await _usuarioClient.ActualizarAsync(_idUsuarioEdicion.Value, request);
                }
                else
                {
                    // ALTA
                    if (string.IsNullOrWhiteSpace(txtPassword.Text) || txtPassword.Text.Length < 6)
                    {
                        MostrarError("La contraseña debe tener al menos 6 caracteres.");
                        btnGuardar.Enabled = true;
                        return;
                    }

                    var request = new CrearUsuarioRequest
                    {
                        IdRol = idRolSeleccionado,
                        Nombre = txtNombre.Text.Trim(),
                        Apellido = txtApellido.Text.Trim(),
                        EmailCorporativo = txtEmail.Text.Trim(),
                        Password = txtPassword.Text
                    };

                    await _usuarioClient.CrearAsync(request);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MostrarError("No se pudo guardar: " + ex.Message);
            }
            finally
            {
                btnGuardar.Enabled = true;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void MostrarError(string mensaje)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.Visible = true;
        }
    }
}