using System;
using System.Net.Http;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmLogin : Form
    {
        private readonly AuthClient _authClient = new AuthClient();

        public frmLogin()
        {
            InitializeComponent();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            AppEstilos.EstilizarBotonPrimario(btnIniciarSesion);
            lblMensaje.Visible = false;
            txtPassword.PasswordChar = '●';
        }

        private void chkVerPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = chkVerPassword.Checked ? '\0' : '●';
        }

        private async void btnIniciarSesion_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            // Validaciones básicas (RF#18)
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                MostrarError("Ingresá un email corporativo válido.");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MostrarError("La contraseña es obligatoria.");
                return;
            }

            btnIniciarSesion.Enabled = false;
            btnIniciarSesion.Text = "Ingresando...";

            try
            {
                var datos = await _authClient.LoginAsync(email, password);

                if (datos == null)
                {
                    MostrarError("No se pudo iniciar sesión. Intentá nuevamente.");
                    return;
                }

                SesionUsuario.IdUsuario = datos.IdUsuario;
                SesionUsuario.Nombre = datos.Nombre;
                SesionUsuario.Apellido = datos.Apellido;
                SesionUsuario.Email = datos.Email;
                SesionUsuario.IdRol = datos.IdRol;
                SesionUsuario.Rol = datos.Rol;

                var dashboard = new frmDashboard();
                dashboard.Show();
                this.Hide();
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    MostrarError("Email o contraseña incorrectos.");
                }
                else
                {
                    MostrarError("No se pudo conectar con el servidor. Verificá tu conexión.");
                }
            }
            catch (Exception ex)
            {
                MostrarError("Ocurrió un error inesperado: " + ex.Message);
            }
            finally
            {
                btnIniciarSesion.Enabled = true;
                btnIniciarSesion.Text = "Iniciar sesión";
            }
        }

        private void MostrarError(string mensaje)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.Visible = true;
        }

        private void frmLogin_Load_1(object sender, EventArgs e)
        {

        }
    }
}