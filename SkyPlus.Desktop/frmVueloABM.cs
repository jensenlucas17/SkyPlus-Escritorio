// SkyPlus.Desktop/frmVueloABM.cs

using System;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmVueloABM : Form
    {
        private readonly VueloClientFake _vueloClient = new VueloClientFake();
        private readonly LugarClientFake _lugarClient = new LugarClientFake();
        private readonly AeronaveClientFake _aeronaveClient = new AeronaveClientFake();

        private readonly VueloResponse? _vueloEdicion;

        public frmVueloABM()
        {
            InitializeComponent();
            _vueloEdicion = null;
        }

        public frmVueloABM(VueloResponse vueloExistente)
        {
            InitializeComponent();
            _vueloEdicion = vueloExistente;
        }

        private void frmVueloABM_Load(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;
            Text = _vueloEdicion == null ? "Nuevo vuelo" : "Editar vuelo";

            CargarCombos();

            if (_vueloEdicion != null)
            {
                txtNumeroVuelo.Text = _vueloEdicion.NumeroVuelo;
                cmbOrigen.SelectedValue = _vueloEdicion.IdLugarOrigen;
                cmbDestino.SelectedValue = _vueloEdicion.IdLugarDestino;
                cmbAeronave.SelectedValue = _vueloEdicion.IdAeronave;
                dtpSalida.Value = _vueloEdicion.Salida;
                dtpLlegada.Value = _vueloEdicion.Llegada;
                cmbEstado.SelectedItem = _vueloEdicion.EstadoVuelo;
                txtTarifa.Text = _vueloEdicion.Tarifa.ToString("0.##");
            }
            else
            {
                dtpSalida.Value = DateTime.Now.AddDays(1);
                dtpLlegada.Value = DateTime.Now.AddDays(1).AddHours(2);
            }
        }

        private void CargarCombos()
        {
            var lugares = _lugarClient.ObtenerTodos();

            cmbOrigen.DisplayMember = nameof(LugarResponse.IataCiudad);
            cmbOrigen.ValueMember = nameof(LugarResponse.IdLugar);
            cmbOrigen.DataSource = lugares;

            cmbDestino.DisplayMember = nameof(LugarResponse.IataCiudad);
            cmbDestino.ValueMember = nameof(LugarResponse.IdLugar);
            cmbDestino.DataSource = _lugarClient.ObtenerTodos(); // lista independiente, si no comparten selección

            cmbAeronave.DisplayMember = nameof(AeronaveResponse.MatriculaModelo);
            cmbAeronave.ValueMember = nameof(AeronaveResponse.IdAeronave);
            cmbAeronave.DataSource = _aeronaveClient.ObtenerTodos();

            cmbEstado.Items.Clear();
            cmbEstado.Items.AddRange(new object[] { "Programado", "Confirmado", "Cancelado", "Finalizado" });
            if (cmbEstado.Items.Count > 0) cmbEstado.SelectedIndex = 0;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            if (string.IsNullOrWhiteSpace(txtNumeroVuelo.Text))
            {
                MostrarError("El número de vuelo es obligatorio.");
                return;
            }

            if (cmbOrigen.SelectedValue is not int idOrigen || cmbDestino.SelectedValue is not int idDestino)
            {
                MostrarError("Seleccioná origen y destino.");
                return;
            }

            if (idOrigen == idDestino)
            {
                MostrarError("El origen y el destino no pueden ser el mismo lugar.");
                return;
            }

            if (cmbAeronave.SelectedValue is not int idAeronave)
            {
                MostrarError("Seleccioná una aeronave.");
                return;
            }

            if (dtpLlegada.Value <= dtpSalida.Value)
            {
                MostrarError("La llegada debe ser posterior a la salida.");
                return;
            }

            if (!decimal.TryParse(txtTarifa.Text, out var tarifa) || tarifa <= 0)
            {
                MostrarError("Ingresá una tarifa válida, mayor a cero.");
                return;
            }

            var vuelo = new VueloResponse
            {
                NumeroVuelo = txtNumeroVuelo.Text.Trim().ToUpper(),
                IdLugarOrigen = idOrigen,
                IdLugarDestino = idDestino,
                IdAeronave = idAeronave,
                Salida = dtpSalida.Value,
                Llegada = dtpLlegada.Value,
                EstadoVuelo = cmbEstado.SelectedItem?.ToString() ?? "Programado",
                Tarifa = tarifa,
                IdUsuarioOperador = SesionUsuario.IdUsuario
            };

            if (_vueloEdicion == null)
            {
                _vueloClient.Crear(vuelo);
            }
            else
            {
                vuelo.IdVuelo = _vueloEdicion.IdVuelo;
                _vueloClient.Actualizar(vuelo);
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