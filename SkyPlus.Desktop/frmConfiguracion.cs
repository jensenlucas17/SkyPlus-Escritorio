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
            // TODO: cargar valores reales desde la API en la 2da entrega.
            txtNombreAerolinea.Text = "SkyPlus Líneas Aéreas";
            cmbMoneda.Items.Clear();
            cmbMoneda.Items.AddRange(new object[] { "ARS", "USD" });
            cmbMoneda.SelectedIndex = 0;

            lblUsuarioActual.Text = $"Usuario: {SesionUsuario.Nombre} {SesionUsuario.Apellido}";

            AppEstilos.EstilizarBotonPrimario(btnGuardarGeneral);
            AppEstilos.EstilizarBotonPrimario(btnCambiarPassword);
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