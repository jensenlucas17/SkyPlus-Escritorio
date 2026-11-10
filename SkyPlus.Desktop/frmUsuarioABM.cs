
using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmUsuarioABM : Form
    {
        private readonly UsuarioClient _usuarioClient = new UsuarioClient();
        private readonly RolClient _rolClient = new RolClient();

        // null = alta, con valor = edición
        private readonly int? _idUsuarioEdicion;

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

            // Conservar el rol actual del usuario
            Tag = usuarioExistente.IdRol;
        }

        private async void frmUsuarioABM_Load(object sender, EventArgs e)
        {
            bool esEdicion = _idUsuarioEdicion.HasValue;

            Text = esEdicion ? "Editar usuario" : "Nuevo usuario";

            // Diseño nuevo (mismo estilo que Nuevo pasajero)
            FormularioDialogo.Disenar(this,
                esEdicion ? "Editar usuario" : "Nuevo usuario",
                esEdicion ? "Modificá los datos o el estado de la cuenta." : "Cuenta de acceso al sistema para un empleado.",
                lblMensaje, btnGuardar, esEdicion ? "Guardar cambios" : "Crear usuario", btnCancelar,
                (label1, "Nombre", txtNombre, 0, 0, ""),
                (label2, "Apellido", txtApellido, 1, 0, ""),
                (lblDNI, "DNI", txtDni, 0, 1, "Sin puntos"),
                (label3, "Email corporativo", txtEmail, 1, 1, "nombre@skyplus.com"),
                (label4, "Rol", cmbRol, 0, 2, ""),
                (lblPassword, "Contraseña inicial", txtPassword, 1, 2, "12+ caracteres, mayúscula y número"),
                (lblEstado, "Estado de la cuenta", cmbEstado, 1, 2, ""));
            lblMensaje.Visible = false;

            // Contraseña solo al crear
            lblPassword.Visible = !esEdicion;
            txtPassword.Visible = !esEdicion;

            // Estado solo al editar
            lblEstado.Visible = esEdicion;
            cmbEstado.Visible = esEdicion;

            cmbEstado.Items.Clear();
            cmbEstado.Items.Add("Activo");
            cmbEstado.Items.Add("Inactivo");

            if (esEdicion)
            {
                cmbEstado.SelectedItem = _estadoInicial;
            }

            await CargarRolesAsync();

        }

        private async System.Threading.Tasks.Task CargarRolesAsync()
        {
            try
            {
                var roles = await _rolClient.ObtenerTodosAsync();

                cmbRol.DisplayMember = nameof(RolResponse.NombreRol);
                cmbRol.ValueMember = nameof(RolResponse.IdRol);
                cmbRol.DataSource = roles;

                // Seleccionar el rol existente al editar
                if (Tag is int idRolActual)
                {
                    cmbRol.SelectedValue = idRolActual;
                }
            }
            catch (HttpRequestException ex)
            {
                MostrarError(
                    "No se pudieron cargar los roles: " + ex.Message);
            }
            catch (Exception ex)
            {
                MostrarError(
                    "Ocurrió un error al cargar los roles: " + ex.Message);
            }
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            string nombre = txtNombre.Text.Trim();
            string apellido = txtApellido.Text.Trim();
            string dni = txtDni.Text.Trim();
            string email = txtEmail.Text.Trim();

            // Validaciones básicas de interfaz
            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(dni))
            {
                MostrarError(
                    "Nombre, apellido, email y DNI son obligatorios.");
                return;
            }

            if (cmbRol.SelectedValue is not int idRolSeleccionado)
            {
                MostrarError("Seleccioná un rol.");
                return;
            }

            if (!_idUsuarioEdicion.HasValue &&
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MostrarError("La contraseña es obligatoria.");
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
                        Nombre = nombre,
                        Apellido = apellido,
                        Dni = dni,
                        EmailCorporativo = email,
                        EstadoCuenta =
                            cmbEstado.SelectedItem?.ToString() ?? "Activo"
                    };

                    var resultado = await _usuarioClient.ActualizarAsync(
                        _idUsuarioEdicion.Value,
                        request);

                    if (resultado == null)
                    {
                        MostrarError(
                            "No se pudo actualizar el usuario.");
                        return;
                    }
                }
                else
                {
                    // ALTA
                    var request = new CrearUsuarioRequest
                    {
                        IdRol = idRolSeleccionado,
                        Nombre = nombre,
                        Apellido = apellido,
                        Dni = dni,
                        EmailCorporativo = email,
                        Password = txtPassword.Text
                    };

                    var resultado = await _usuarioClient.CrearAsync(request);

                    if (resultado == null)
                    {
                        MostrarError(
                            "No se pudo crear el usuario.");
                        return;
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    MostrarError(
                        "Tu sesión expiró. Iniciá sesión nuevamente.");
                }
                else if (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    MostrarError(
                        "No tenés permisos para gestionar usuarios.");
                }
                else if (ex.StatusCode.HasValue)
                {
                    // Mostrar el mensaje enviado por la API
                    MostrarError(ex.Message);
                }
                else
                {
                    MostrarError(
                        "No se pudo conectar con la API: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MostrarError(
                    "Ocurrió un error: " + ex.Message);
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
            FormularioDialogo.MostrarError(lblMensaje, mensaje);
        }
    }
}
