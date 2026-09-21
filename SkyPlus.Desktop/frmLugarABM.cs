// SkyPlus.Desktop/frmLugarABM.cs

using System;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmLugarABM : Form
    {
        private readonly LugarClientFake _lugarClient = new LugarClientFake();
        private readonly LugarResponse? _lugarEdicion;

        public frmLugarABM()
        {
            InitializeComponent();
            _lugarEdicion = null;
        }

        public frmLugarABM(LugarResponse lugarExistente)
        {
            InitializeComponent();
            _lugarEdicion = lugarExistente;

            txtCodigoIata.Text = lugarExistente.CodigoIata;
            txtNombre.Text = lugarExistente.Nombre;
            txtCiudad.Text = lugarExistente.Ciudad;
            txtPais.Text = lugarExistente.Pais;
        }

        private void frmLugarABM_Load(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;
            Text = _lugarEdicion == null ? "Nuevo aeropuerto" : "Editar aeropuerto";
            txtCodigoIata.MaxLength = 3;
            txtCodigoIata.CharacterCasing = CharacterCasing.Upper;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            if (txtCodigoIata.Text.Trim().Length != 3)
            {
                MostrarError("El código IATA debe tener exactamente 3 letras.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtCiudad.Text) ||
                string.IsNullOrWhiteSpace(txtPais.Text))
            {
                MostrarError("Nombre, ciudad y país son obligatorios.");
                return;
            }

            if (_lugarEdicion == null)
            {
                _lugarClient.Crear(new LugarResponse
                {
                    CodigoIata = txtCodigoIata.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Ciudad = txtCiudad.Text.Trim(),
                    Pais = txtPais.Text.Trim()
                });
            }
            else
            {
                _lugarClient.Actualizar(new LugarResponse
                {
                    IdLugar = _lugarEdicion.IdLugar,
                    CodigoIata = txtCodigoIata.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Ciudad = txtCiudad.Text.Trim(),
                    Pais = txtPais.Text.Trim()
                });
            }

            DialogResult = DialogResult.OK;
            Close();
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