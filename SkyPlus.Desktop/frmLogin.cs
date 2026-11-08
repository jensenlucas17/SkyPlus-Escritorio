using System;
using System.Drawing;
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
                SesionUsuario.Token = datos.Token;
                SesionUsuario.ExpiracionToken = datos.ExpiracionToken;

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

        // Este es el Load que está conectado en el diseñador
        private void frmLogin_Load_1(object sender, EventArgs e)
        {
            DisenarLogin();
            frmLogin_Load(sender, e);
        }

        // Se llama al cerrar sesión, para volver a usar esta misma ventana
        public void Reiniciar()
        {
            txtPassword.Clear();
            chkVerPassword.Checked = false;
            lblMensaje.Visible = false;
            btnIniciarSesion.Enabled = true;
            btnIniciarSesion.Text = "Iniciar sesión";
        }

        // ---------------------------------------------------------------------
        // Diseño: panel de marca a la izquierda y formulario a la derecha.
        // Reubica los controles del diseñador; no cambia su lógica.
        // ---------------------------------------------------------------------
        private void DisenarLogin()
        {
            Text = "SkyPlus - Iniciar sesión";
            ClientSize = new Size(900, 560);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

            var pnlMarca = new Panel
            {
                Dock = DockStyle.Left,
                Width = 380,
                BackColor = AppEstilos.ColorLateral
            };
            pnlMarca.Paint += DibujarMarca;
            Controls.Add(pnlMarca);

            const int x = 450;
            const int ancho = 360;

            lblTitulo.Text = "Iniciar sesión";
            lblTitulo.Font = AppEstilos.FuenteTitulo;
            lblTitulo.ForeColor = AppEstilos.ColorTextoOscuro;
            lblTitulo.Location = new Point(x, 118);

            var lblSubtitulo = AppEstilos.CrearTextoSuave("Ingresá con tu email corporativo y tu contraseña.");
            lblSubtitulo.Location = new Point(x + 2, 156);
            Controls.Add(lblSubtitulo);

            label1.Text = "Email corporativo";
            label1.Font = AppEstilos.FuenteBoton;
            label1.ForeColor = AppEstilos.ColorTextoOscuro;
            label1.Location = new Point(x, 206);

            txtEmail.Font = new Font("Segoe UI", 11F);
            txtEmail.BorderStyle = BorderStyle.FixedSingle;
            txtEmail.SetBounds(x, 230, ancho, 28);
            txtEmail.PlaceholderText = "nombre@skyplus.com";

            label3.Text = "Contraseña";
            label3.Font = AppEstilos.FuenteBoton;
            label3.ForeColor = AppEstilos.ColorTextoOscuro;
            label3.Location = new Point(x, 280);

            txtPassword.Font = new Font("Segoe UI", 11F);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.SetBounds(x, 304, ancho, 28);

            chkVerPassword.Font = AppEstilos.FuenteNormal;
            chkVerPassword.ForeColor = AppEstilos.ColorTextoSuave;
            chkVerPassword.Location = new Point(x, 346);

            AppEstilos.EstilizarBotonPrimario(btnIniciarSesion);
            btnIniciarSesion.Text = "Iniciar sesión";
            btnIniciarSesion.SetBounds(x, 388, ancho, 42);

            lblMensaje.Font = AppEstilos.FuenteNormal;
            lblMensaje.ForeColor = AppEstilos.ColorError;
            lblMensaje.MaximumSize = new Size(ancho, 0);
            lblMensaje.Location = new Point(x, 442);

            ActiveControl = txtEmail;
        }

        private void DibujarMarca(object? sender, PaintEventArgs e)
        {
            var panel = (Panel)sender!;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (var pincel = new SolidBrush(AppEstilos.ColorPrimario))
                g.FillEllipse(pincel, 48, 150, 44, 44);
            using (var fuenteLogo = new Font("Segoe UI", 18F, FontStyle.Bold))
                TextRenderer.DrawText(g, "S", fuenteLogo, new Rectangle(48, 150, 44, 44), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            using (var fuenteMarca = new Font("Segoe UI", 28F, FontStyle.Bold))
                TextRenderer.DrawText(g, "SkyPlus", fuenteMarca, new Point(44, 206), Color.White);

            TextRenderer.DrawText(g, "Sistema de venta presencial\nde pasajes aéreos", AppEstilos.FuenteSubtitulo,
                new Rectangle(48, 262, panel.Width - 80, 60), AppEstilos.ColorLateralTexto, TextFormatFlags.WordBreak);

            TextRenderer.DrawText(g, "Taller II · 2026", AppEstilos.FuentePequena,
                new Point(48, panel.Height - 44), AppEstilos.ColorLateralTitulo);
        }
    }
}