using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using System;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmUsuarioABM : Form
    {
        private readonly UsuarioClient _usuarioClient = new UsuarioClient();
        private readonly RolClient _rolClient = new RolClient();

        private readonly int? _idUsuarioEdicion; // null = alta, con valor = edición
        private string _estadoInicial = "Activo";

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
            txtDni.Text = usuarioExistente.Dni ?? string.Empty;

            _estadoInicial = usuarioExistente.EstadoCuenta;

            // Se utiliza Tag para conservar el rol actual
            Tag = usuarioExistente.IdRol;
        }

        private async void frmUsuarioABM_Load(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            bool esEdicion = _idUsuarioEdicion.HasValue;

            Text = esEdicion ? "Editar usuario" : "Nuevo usuario";

            // Contraseña solo se solicita al crear
            lblPassword.Visible = !esEdicion;
            txtPassword.Visible = !esEdicion;

            // Estado solo se muestra al editar
            lblEstado.Visible = esEdicion;
            cmbEstado.Visible = esEdicion;

            // Opciones de estado
            cmbEstado.Items.Clear();
            cmbEstado.Items.Add("Activo");
            cmbEstado.Items.Add("Inactivo");

            if (esEdicion)
            {
                cmbEstado.SelectedItem = _estadoInicial;
            }

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

        private static bool NombreValido(string? valor)
        {
            return !string.IsNullOrWhiteSpace(valor) &&
                System.Text.RegularExpressions.Regex.IsMatch(
                    valor,
                    @"^\p{L}+(?:[ '\-]\p{L}+)*$");
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            string dni = txtDni.Text.Trim();

            // Validaciones básicas (RF#18)
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(dni))
            {
                MostrarError("Nombre, apellido, email y DNI son obligatorios.");
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

            if (!System.Text.RegularExpressions.Regex.IsMatch(
                dni, @"^[0-9]{7,8}$"))
            {
                MostrarError("El DNI debe contener 7 u 8 números, sin puntos.");
                return;
            }

            if (!NombreValido(txtNombre.Text.Trim()))
            {
                MostrarError("El nombre solo puede contener letras.");
                txtNombre.Focus();
                return;
            }

            if (!NombreValido(txtApellido.Text.Trim()))
            {
                MostrarError("El apellido solo puede contener letras.");
                txtApellido.Focus();
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
                        Dni = dni,
                        EmailCorporativo = txtEmail.Text.Trim(),
                        EstadoCuenta = cmbEstado.SelectedItem?.ToString() ?? "Activo"
                    };

                    var resultado = await _usuarioClient.ActualizarAsync(
                        _idUsuarioEdicion.Value, request);

                    if (resultado == null)
                    {
                        MostrarError("No se pudo actualizar el usuario. Verificá los datos ingresados.");
                        return;
                    }
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
                        Dni = dni,
                        EmailCorporativo = txtEmail.Text.Trim(),
                        Password = txtPassword.Text
                    };

                    var resultado = await _usuarioClient.CrearAsync(request);

                    if (resultado == null)
                    {
                        MostrarError("No se pudo crear el usuario. Verificá que el DNI y el email no estén registrados.");
                        return;
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (HttpRequestException)
            {
                MostrarError(
                    "No se pudo guardar el usuario. " +
                    "Verificá los datos o si ya existe el DNI o email.");
            }
            catch (Exception ex)
            {
                MostrarError("Ocurrió un error: " + ex.Message);
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
            lblMensaje.ForeColor = Color.Red;

            lblMensaje.AutoSize = false;
            lblMensaje.TextAlign = ContentAlignment.MiddleCenter;

            lblMensaje.Width = ClientSize.Width - 40;
            lblMensaje.Height = 48;

            lblMensaje.Left = 20;
            lblMensaje.Top = btnGuardar.Top - lblMensaje.Height - 12;

            lblMensaje.Visible = true;
            lblMensaje.BringToFront();
        }
    }
}