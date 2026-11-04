
using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using System;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmPasajeroABM : Form
    {
        private readonly PasajeroClient _pasajeroClient;
        private readonly PasajeroResponse? _pasajeroEdicion;

        public frmPasajeroABM(
            PasajeroClient pasajeroClient,
            PasajeroResponse? pasajeroExistente)
        {
            InitializeComponent();

            _pasajeroClient = pasajeroClient;
            _pasajeroEdicion = pasajeroExistente;
        }

        private void frmPasajeroABM_Load(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            Text = _pasajeroEdicion == null
                ? "Nuevo pasajero"
                : "Editar pasajero";

            AppEstilos.EstilizarBotonPrimario(btnGuardar);
            AppEstilos.EstilizarBotonSecundario(btnCancelar);

            if (_pasajeroEdicion != null)
            {
                txtNombre.Text = _pasajeroEdicion.Nombre;
                txtApellido.Text = _pasajeroEdicion.Apellido;
                txtDocumento.Text = _pasajeroEdicion.Documento;
                txtNacionalidad.Text = _pasajeroEdicion.Nacionalidad;
                txtEmail.Text = _pasajeroEdicion.Email;
                txtTelefono.Text = _pasajeroEdicion.Telefono;
            }
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            // Validaciones básicas de interfaz
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MostrarError("El nombre es obligatorio.");
                txtNombre.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MostrarError("El apellido es obligatorio.");
                txtApellido.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                MostrarError("El documento es obligatorio.");
                txtDocumento.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNacionalidad.Text))
            {
                MostrarError("La nacionalidad es obligatoria.");
                txtNacionalidad.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MostrarError("El email es obligatorio.");
                txtEmail.Focus();
                return;
            }

            // El teléfono es opcional.
            // Las validaciones de formato se realizan en la API.

            btnGuardar.Enabled = false;

            try
            {
                if (_pasajeroEdicion == null)
                {
                    // CREAR PASAJERO
                    await _pasajeroClient.CrearAsync(
                        txtNombre.Text.Trim(),
                        txtApellido.Text.Trim(),
                        txtDocumento.Text.Trim(),
                        txtNacionalidad.Text.Trim(),
                        txtEmail.Text.Trim(),
                        txtTelefono.Text.Trim());
                }
                else
                {
                    // ACTUALIZAR PASAJERO
                    await _pasajeroClient.ActualizarAsync(
                        _pasajeroEdicion.IdPasajero,
                        txtNombre.Text.Trim(),
                        txtApellido.Text.Trim(),
                        txtDocumento.Text.Trim(),
                        txtNacionalidad.Text.Trim(),
                        txtEmail.Text.Trim(),
                        txtTelefono.Text.Trim());
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    MostrarError(
                        "Tu sesión expiró o no es válida. Iniciá sesión nuevamente.");
                }
                else if (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    MostrarError(
                        "No tenés permisos para realizar esta operación.");
                }
                else if (ex.StatusCode.HasValue)
                {
                    // Mostrar el mensaje específico enviado por la API
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
            lblMensaje.ForeColor = Color.Red;

            lblMensaje.AutoSize = false;
            lblMensaje.TextAlign = ContentAlignment.MiddleCenter;

            lblMensaje.Width = ClientSize.Width - 30;
            lblMensaje.Height = 48;

            lblMensaje.Left = 15;
            lblMensaje.Top =
                btnGuardar.Top - lblMensaje.Height - 12;

            lblMensaje.Visible = true;
            lblMensaje.BringToFront();
        }
    }
}
