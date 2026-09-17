// SkyPlus.Desktop/frmAeronaveABM.cs

using System;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;

namespace SkyPlus.Desktop
{
    public partial class frmAeronaveABM : Form
    {
        private readonly AeronaveClientFake _aeronaveClient = new AeronaveClientFake();
        private readonly AeronaveResponse? _aeronaveEdicion;

        public frmAeronaveABM()
        {
            InitializeComponent();
            _aeronaveEdicion = null;
        }

        public frmAeronaveABM(AeronaveResponse aeronaveExistente)
        {
            InitializeComponent();
            _aeronaveEdicion = aeronaveExistente;

            txtMatricula.Text = aeronaveExistente.Matricula;
            txtModelo.Text = aeronaveExistente.Modelo;
        }

        private void frmAeronaveABM_Load(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;
            Text = _aeronaveEdicion == null ? "Nueva aeronave" : "Editar aeronave";
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            if (string.IsNullOrWhiteSpace(txtMatricula.Text) || string.IsNullOrWhiteSpace(txtModelo.Text))
            {
                MostrarError("Matrícula y modelo son obligatorios.");
                return;
            }

            if (_aeronaveEdicion == null)
            {
                _aeronaveClient.Crear(new AeronaveResponse
                {
                    Matricula = txtMatricula.Text.Trim().ToUpper(),
                    Modelo = txtModelo.Text.Trim()
                });
            }
            else
            {
                _aeronaveClient.Actualizar(new AeronaveResponse
                {
                    IdAeronave = _aeronaveEdicion.IdAeronave,
                    Matricula = txtMatricula.Text.Trim().ToUpper(),
                    Modelo = txtModelo.Text.Trim()
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