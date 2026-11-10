// SkyPlus.Desktop/frmVueloABM.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmVueloABM : Form
    {
        private readonly VueloClient _vueloClient;
        private readonly LugarClient _lugarClient;
        private readonly AeronaveClient _aeronaveClient;

        private readonly VueloResponse? _vueloEdicion;

        public frmVueloABM(
            VueloClient vueloClient,
            VueloResponse? vueloExistente)
        {
            InitializeComponent();

            _vueloClient = vueloClient;
            _lugarClient = new LugarClient();
            _aeronaveClient = new AeronaveClient();

            _vueloEdicion = vueloExistente;
        }

        private async void frmVueloABM_Load(
            object sender,
            EventArgs e)
        {
            Text = _vueloEdicion == null
                ? "Nuevo vuelo"
                : "Editar vuelo";

            // Diseño nuevo (mismo estilo que Nuevo pasajero)
            foreach (var dtp in new[] { dtpSalida, dtpLlegada })
            {
                dtp.Format = DateTimePickerFormat.Custom;
                dtp.CustomFormat = "dd/MM/yyyy  HH:mm";
            }
            FormularioDialogo.Disenar(this,
                Text,
                _vueloEdicion == null
                    ? "La aeronave tiene que estar libre (60 min de escala entre vuelos)."
                    : $"{_vueloEdicion.NumeroVuelo} · {_vueloEdicion.LugarOrigen} → {_vueloEdicion.LugarDestino}",
                lblMensaje, btnGuardar, _vueloEdicion == null ? "Crear vuelo" : "Guardar cambios", btnCancelar,
                (label1, "Número de vuelo", txtNumeroVuelo, 0, 0, "Ej.: SP312"),
                (label4, "Aeronave", cmbAeronave, 1, 0, ""),
                (label2, "Origen", cmbOrigen, 0, 1, ""),
                (label3, "Destino", cmbDestino, 1, 1, ""),
                (label5, "Salida", dtpSalida, 0, 2, ""),
                (label6, "Llegada", dtpLlegada, 1, 2, ""),
                (label7, "Estado", cmbEstado, 0, 3, ""),
                (label8, "Tarifa (ARS)", txtTarifa, 1, 3, "Ej.: 125000"));
            lblMensaje.Visible = false;

            try
            {
                await CargarCombos();

                if (_vueloEdicion != null)
                {
                    txtNumeroVuelo.Text =
                        _vueloEdicion.NumeroVuelo;

                    cmbOrigen.SelectedValue =
                        _vueloEdicion.IdLugarOrigen;

                    cmbDestino.SelectedValue =
                        _vueloEdicion.IdLugarDestino;

                    cmbAeronave.SelectedValue =
                        _vueloEdicion.IdAeronave;

                    dtpSalida.Value =
                        _vueloEdicion.Salida;

                    dtpLlegada.Value =
                        _vueloEdicion.Llegada;

                    cmbEstado.SelectedItem =
                        _vueloEdicion.EstadoVuelo;

                    txtTarifa.Text =
                        _vueloEdicion.Tarifa
                            .ToString("0.##");
                }
                else
                {
                    dtpSalida.Value =
                        DateTime.Now.AddDays(1);

                    dtpLlegada.Value =
                        DateTime.Now
                            .AddDays(1)
                            .AddHours(2);

                    if (cmbEstado.Items.Count > 0)
                    {
                        cmbEstado.SelectedIndex = 0;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MostrarError(ex.StatusCode.HasValue
                    ? ex.Message
                    : "No se pudo conectar con la API de SkyPlus. Verificá que esté ejecutándose.");
            }
            catch (Exception ex)
            {
                MostrarError(
                    "Ocurrió un error al cargar los datos: " +
                    ex.Message);
            }
        }

        private async Task CargarCombos()
        {
            // =========================
            // LUGARES DESDE LA API
            // =========================

            var lugares =
                await _lugarClient.ObtenerTodosAsync();

            if (lugares == null)
            {
                lugares =
                    new List<LugarResponse>();
            }

            lugares = lugares
                .Where(l => l.Estado == "Activo" ||
                    (_vueloEdicion != null &&
                     (l.IdLugar == _vueloEdicion.IdLugarOrigen ||
                      l.IdLugar == _vueloEdicion.IdLugarDestino)))
                .ToList();

            cmbOrigen.DisplayMember =
                nameof(LugarResponse.IataCiudad);

            cmbOrigen.ValueMember =
                nameof(LugarResponse.IdLugar);

            cmbOrigen.DataSource =
                lugares;

            var lugaresDestino =
                new List<LugarResponse>(lugares);

            cmbDestino.DisplayMember =
                nameof(LugarResponse.IataCiudad);

            cmbDestino.ValueMember =
                nameof(LugarResponse.IdLugar);

            cmbDestino.DataSource =
                lugaresDestino;


            // =========================
            // AERONAVES DESDE LA API
            // =========================

            var aeronaves =
                await _aeronaveClient.ObtenerTodosAsync();

            if (aeronaves == null)
            {
                aeronaves =
                    new List<AeronaveResponse>();
            }

            aeronaves = aeronaves
                .Where(a => a.Estado == "Activo" ||
                    (_vueloEdicion != null &&
                     a.IdAeronave == _vueloEdicion.IdAeronave))
                .ToList();

            cmbAeronave.DisplayMember =
                nameof(AeronaveResponse.MatriculaModelo);

            cmbAeronave.ValueMember =
                nameof(AeronaveResponse.IdAeronave);

            cmbAeronave.DataSource =
                aeronaves;


            // =========================
            // ESTADOS DEL VUELO
            // =========================

            cmbEstado.Items.Clear();

            cmbEstado.Items.AddRange(
                new object[]
                {
                    "Programado",
                    "En embarque",
                    "En vuelo",
                    "Demorado",
                    "Cancelado",
                    "Finalizado"
                });

            if (cmbEstado.Items.Count > 0)
            {
                cmbEstado.SelectedIndex = 0;
            }
        }

        private async void btnGuardar_Click(
            object sender,
            EventArgs e)
        {
            lblMensaje.Visible = false;

            // =========================
            // VALIDAR NÚMERO DE VUELO
            // =========================

            var numeroVuelo =
                txtNumeroVuelo.Text.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(numeroVuelo))
            {
                MostrarError(
                    "El número de vuelo es obligatorio.");

                txtNumeroVuelo.Focus();
                return;
            }


            // =========================
            // VALIDAR ORIGEN Y DESTINO
            // =========================

            if (cmbOrigen.SelectedValue is not int idOrigen ||
                cmbDestino.SelectedValue is not int idDestino)
            {
                MostrarError(
                    "Seleccioná origen y destino.");

                return;
            }

            if (idOrigen == idDestino)
            {
                MostrarError(
                    "El origen y el destino no pueden " +
                    "ser el mismo lugar.");

                return;
            }


            // =========================
            // VALIDAR AERONAVE
            // =========================

            if (cmbAeronave.SelectedValue
                is not int idAeronave)
            {
                MostrarError(
                    "Seleccioná una aeronave.");

                return;
            }


            // =========================
            // VALIDAR FECHAS
            // =========================

            if (dtpLlegada.Value <=
                dtpSalida.Value)
            {
                MostrarError(
                    "La llegada debe ser posterior " +
                    "a la salida.");

                return;
            }


            // =========================
            // VALIDAR TARIFA
            // =========================

            if (!decimal.TryParse(
                    txtTarifa.Text,
                    out var tarifa) ||
                tarifa <= 0)
            {
                MostrarError(
                    "Ingresá una tarifa válida, " +
                    "mayor a cero.");

                txtTarifa.Focus();
                return;
            }


            // =========================
            // ESTADO
            // =========================

            var estado =
                cmbEstado.SelectedItem?.ToString()
                ?? "Programado";


            // =========================
            // USUARIO OPERADOR
            // =========================

            if (SesionUsuario.IdUsuario <= 0)
            {
                MostrarError(
                    "No se pudo identificar al " +
                    "usuario operador de la sesión.");

                return;
            }


            try
            {
                // =========================
                // CREAR VUELO
                // =========================

                if (_vueloEdicion == null)
                {
                    await _vueloClient.CrearAsync(
                        numeroVuelo,
                        idAeronave,
                        SesionUsuario.IdUsuario,
                        idOrigen,
                        idDestino,
                        dtpSalida.Value,
                        dtpLlegada.Value,
                        estado,
                        tarifa);

                    MessageBox.Show(
                        "Vuelo creado correctamente.",
                        "Éxito",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                // =========================
                // ACTUALIZAR VUELO
                // =========================

                else
                {
                    await _vueloClient.ActualizarAsync(
                        _vueloEdicion.IdVuelo,
                        numeroVuelo,
                        idAeronave,
                        SesionUsuario.IdUsuario,
                        idOrigen,
                        idDestino,
                        dtpSalida.Value,
                        dtpLlegada.Value,
                        estado,
                        tarifa);

                    MessageBox.Show(
                        "Vuelo actualizado correctamente.",
                        "Éxito",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                DialogResult =
                    DialogResult.OK;

                Close();
            }
            catch (HttpRequestException ex)
            {
                MostrarError(ex.StatusCode.HasValue
                    ? ex.Message
                    : "No se pudo conectar con la API de SkyPlus. Verificá que esté ejecutándose.");
            }
            catch (Exception ex)
            {
                MostrarError(
                    "Ocurrió un error al guardar el vuelo:\n\n" +
                    ex.Message);
            }
        }

        private void btnCancelar_Click(
            object sender,
            EventArgs e)
        {
            DialogResult =
                DialogResult.Cancel;

            Close();
        }

        private void MostrarError(
            string mensaje)
        {
            FormularioDialogo.MostrarError(lblMensaje, mensaje);
        }
    }
}