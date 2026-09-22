// SkyPlus.Desktop/frmPasajeroABM.cs

using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using System;
using System.Net.Http;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmPasajeroABM : Form
    {
        private readonly PasajeroClient _pasajeroClient;
        private readonly PasajeroResponse? _pasajeroEdicion;

        public frmPasajeroABM(PasajeroClient pasajeroClient, PasajeroResponse? pasajeroExistente)
        {
            InitializeComponent();
            _pasajeroClient = pasajeroClient;
            _pasajeroEdicion = pasajeroExistente;
        }

        private void frmPasajeroABM_Load(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;
            Text = _pasajeroEdicion == null ? "Nuevo pasajero" : "Editar pasajero";
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

            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                MostrarError("Nombre, apellido y documento son obligatorios.");
                return;
            }

            btnGuardar.Enabled = false;

            try
            {
                if (_pasajeroEdicion == null)
                {
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
            catch (HttpRequestException)
            {
                MostrarError("No se pudo conectar con la API.");
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