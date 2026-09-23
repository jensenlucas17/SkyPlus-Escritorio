// SkyPlus.Desktop/frmConfiguracion.cs
// Datos locales de demostracion - sin conexion a API. Funcionalidad completa: 2da entrega.

using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using System;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmConfiguracion : Form
    {
        public frmConfiguracion()
        {
            InitializeComponent();
        }

        private void frmConfiguracion_Load(object sender, EventArgs e)
        {
            bool esAdministrador =
                SesionUsuario.Rol == "Administrador";

            // ==========================
            // SEGURIDAD - TODOS LOS ROLES
            // ==========================

            grpSeguridad.Visible = true;

            lblUsuarioActual.Text =
                $"Usuario: {SesionUsuario.Nombre} {SesionUsuario.Apellido}";

            AppEstilos.EstilizarBotonPrimario(btnCambiarPassword);


            // ==========================
            // CONFIGURACIÓN GENERAL
            // SOLO ADMINISTRADOR
            // ==========================

            grpDatosGenerales.Visible = esAdministrador;

            if (esAdministrador)
            {
                // Datos locales de demostración.
                // Se conectarán a la API posteriormente.
                txtNombreAerolinea.Text = "SkyPlus Líneas Aéreas";

                cmbMoneda.Items.Clear();
                cmbMoneda.Items.AddRange(new object[]
                {
            "ARS",
            "USD"
                });

                cmbMoneda.SelectedIndex = 0;

                AppEstilos.EstilizarBotonPrimario(
                    btnGuardarGeneral);
            }


            // ==========================
            // POSICIONAMIENTO
            // ==========================

            if (esAdministrador)
            {
                // Mantiene las posiciones originales.
                grpDatosGenerales.Location =
                    new System.Drawing.Point(246, 43);

                grpSeguridad.Location =
                    new System.Drawing.Point(246, 241);
            }
            else
            {
                // Como Datos Generales está oculto,
                // Seguridad ocupa su lugar.
                grpSeguridad.Location =
                    new System.Drawing.Point(246, 43);
            }
        }

        private void btnGuardarGeneral_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Los cambios de configuración general estarán disponibles en una próxima versión.",
                "Configuración", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCambiarPassword_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPasswordActual.Text) ||
                string.IsNullOrWhiteSpace(txtPasswordNueva.Text))
            {
                MessageBox.Show("Completá ambos campos de contraseña.", "Configuración", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(
                "El cambio de contraseña estará disponible en una próxima versión.",
                "Configuración", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}