// SkyPlus.Desktop/frmReservaABM.cs

using System;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;
using System.Collections.Generic;

namespace SkyPlus.Desktop
{
    public partial class frmReservaABM : Form
    {
        private readonly ReservaClient _reservaClient = new ReservaClient();
        private readonly PasajeroClient _pasajeroClient = new PasajeroClient();
        private readonly VueloClient _vueloClient = new VueloClient();
        private readonly AsientoClient _asientoClient = new AsientoClient();

        public frmReservaABM()
        {
            InitializeComponent();
        }

        private async void frmReservaABM_Load(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            AppEstilos.EstilizarBotonPrimario(btnGuardar);
            AppEstilos.EstilizarBotonSecundario(btnCancelar);

            await CargarPasajeros();
            await CargarVuelos();
        }

        private async System.Threading.Tasks.Task CargarPasajeros()
        {
            var pasajeros = await _pasajeroClient.ObtenerTodosAsync();

            cmbPasajero.DataSource = null;

            cmbPasajero.DisplayMember = nameof(PasajeroResponse.NombreCompleto);
            cmbPasajero.ValueMember = nameof(PasajeroResponse.IdPasajero);
            cmbPasajero.DataSource = pasajeros;

            // Permite escribir
            cmbPasajero.DropDownStyle = ComboBoxStyle.DropDown;

            // Autocompletado
            cmbPasajero.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbPasajero.AutoCompleteSource = AutoCompleteSource.ListItems;

            cmbPasajero.SelectedIndex = -1;
        }

        private async void btnNuevoPasajero_Click(object sender, EventArgs e)
        {
            using var form = new frmPasajeroABM(_pasajeroClient, null);

            if (form.ShowDialog() == DialogResult.OK)
            {
                await CargarPasajeros();
            }
        }

        private List<VueloResponse> _vuelos = new();

        private async System.Threading.Tasks.Task CargarVuelos()
        {
            _vuelos = await _vueloClient.ObtenerTodosAsync()
                      ?? new List<VueloResponse>();

            CargarOrigenes();
        }

        // Clase interna para representar las opciones de lugar en el ComboBox
        private class LugarOpcion
        {
            public int IdLugar { get; set; }
            public string Nombre { get; set; } = string.Empty;
        }

        private void CargarOrigenes()
        {
            var origenes = _vuelos
                .Where(v => !string.IsNullOrWhiteSpace(v.LugarOrigen))
                .GroupBy(v => v.IdLugarOrigen)
                .Select(g => new LugarOpcion
                {
                    IdLugar = g.Key,
                    Nombre = g.First().LugarOrigen
                })
                .OrderBy(l => l.Nombre)
                .ToList();

            cmbOrigen.DataSource = null;
            cmbOrigen.DisplayMember = nameof(LugarOpcion.Nombre);
            cmbOrigen.ValueMember = nameof(LugarOpcion.IdLugar);
            cmbOrigen.DataSource = origenes;
            cmbOrigen.SelectedIndex = -1;

            cmbDestino.DataSource = null;
            cmbVuelo.DataSource = null;

            LimpiarAsientos();
        }

       
        private void cmbOrigen_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbDestino.DataSource = null;
            cmbVuelo.DataSource = null;
            LimpiarAsientos();

            if (cmbOrigen.SelectedValue is not int idOrigen)
                return;

            var destinos = _vuelos
                .Where(v =>
                    v.IdLugarOrigen == idOrigen &&
                    !string.IsNullOrWhiteSpace(v.LugarDestino))
                .GroupBy(v => v.IdLugarDestino)
                .Select(g => new LugarOpcion
                {
                    IdLugar = g.Key,
                    Nombre = g.First().LugarDestino
                })
                .OrderBy(l => l.Nombre)
                .ToList();

            cmbDestino.DisplayMember = nameof(LugarOpcion.Nombre);
            cmbDestino.ValueMember = nameof(LugarOpcion.IdLugar);
            cmbDestino.DataSource = destinos;
            cmbDestino.SelectedIndex = -1;

            if (destinos.Count == 0)
            {
                MostrarError(
                    "No hay destinos disponibles desde el origen seleccionado.");
            }
            else
            {
                lblMensaje.Visible = false;
            }
        }

        private void cmbDestino_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbVuelo.DataSource = null;
            LimpiarAsientos();

            if (cmbOrigen.SelectedValue is not int idOrigen ||
                cmbDestino.SelectedValue is not int idDestino)
                return;

            var vuelosDisponibles = _vuelos
                .Where(v =>
                    v.IdLugarOrigen == idOrigen &&
                    v.IdLugarDestino == idDestino &&
                    !string.Equals(v.EstadoVuelo, "Cancelado",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(v.EstadoVuelo, "Finalizado",
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(v => v.Salida)
                .ToList();

            cmbVuelo.DisplayMember = nameof(VueloResponse.DescripcionReserva);
            cmbVuelo.ValueMember = nameof(VueloResponse.IdVuelo);
            cmbVuelo.DataSource = vuelosDisponibles;
            cmbVuelo.SelectedIndex = -1;

            if (vuelosDisponibles.Count == 0)
            {
                MostrarError(
                    "No hay vuelos disponibles para el origen y destino seleccionados.");
            }
            else
            {
                lblMensaje.Visible = false;
            }
        }

        private int? _idAsientoSeleccionado;
        private async void cmbVuelo_SelectedIndexChanged(object sender, EventArgs e)
        {
            LimpiarAsientos();

            if (cmbVuelo.SelectedItem is not VueloResponse vuelo)
            {
                lblTarifa.Text = "TOTAL: $0";
                return;
            }

            lblTarifa.Text = $"TOTAL: {vuelo.Tarifa:C0}";

            try
            {
                var asientos = await _asientoClient.ObtenerPorVueloAsync(
                    vuelo.IdVuelo);

                var lista = asientos ?? new List<AsientoResponse>();

                MostrarAsientos(lista);

                bool hayDisponibles = lista.Any(a =>
                    string.Equals(
                        a.Estado,
                        "Libre",
                        StringComparison.OrdinalIgnoreCase));

                if (!hayDisponibles)
                {
                    MostrarError(
                        "Este vuelo no tiene asientos disponibles.");
                }
                else
                {
                    lblMensaje.Visible = false;
                }
            }
            catch (HttpRequestException)
            {
                MostrarError(
                    "No se pudieron obtener los asientos del vuelo.");
            }
            catch (Exception ex)
            {
                MostrarError(
                    "Ocurrió un error al cargar los asientos: " +
                    ex.Message);
            }
        }

        private void MostrarAsientos(List<AsientoResponse> asientos)
        {
            flpAsientos.Controls.Clear();

            foreach (var asiento in asientos
                .OrderBy(a => a.Fila)
                .ThenBy(a => a.Letra))
            {
                bool disponible = string.Equals(
                    asiento.Estado,
                    "Libre",
                    StringComparison.OrdinalIgnoreCase);

                var boton = new Button
                {
                    Text = asiento.CodigoAsiento,
                    Width = 55,
                    Height = 45,
                    Margin = new Padding(6),
                    Tag = asiento,
                    Enabled = disponible
                };

                if (disponible)
                {
                    boton.BackColor = System.Drawing.Color.White;
                    boton.ForeColor = System.Drawing.Color.Black;
                }
                else
                {
                    boton.BackColor = System.Drawing.Color.LightGray;
                    boton.ForeColor = System.Drawing.Color.DarkGray;
                }

                boton.Click += Asiento_Click;

                flpAsientos.Controls.Add(boton);
            }
        }

        private void Asiento_Click(object? sender, EventArgs e)
        {
            if (sender is not Button boton ||
                boton.Tag is not AsientoResponse asiento)
                return;

            _idAsientoSeleccionado = asiento.IdAsiento;

            foreach (Control control in flpAsientos.Controls)
            {
                if (control is Button btn && btn.Enabled)
                {
                    btn.BackColor = System.Drawing.Color.White;
                    btn.ForeColor = System.Drawing.Color.Black;
                }
            }

            boton.BackColor = System.Drawing.Color.DodgerBlue;
            boton.ForeColor = System.Drawing.Color.White;
        }

        private void LimpiarAsientos()
        {
            _idAsientoSeleccionado = null;

            if (flpAsientos != null)
            {
                flpAsientos.Controls.Clear();
            }

            lblTarifa.Text = "TOTAL: $0";
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            if (cmbPasajero.SelectedValue is not int idPasajero)
            {
                MostrarError("Seleccioná un pasajero.");
                return;
            }

            if (cmbOrigen.SelectedValue is not int)
            {
                MostrarError("Seleccioná un origen.");
                return;
            }

            if (cmbDestino.SelectedValue is not int)
            {
                MostrarError("Seleccioná un destino.");
                return;
            }

            if (cmbVuelo.SelectedValue is not int idVuelo)
            {
                MostrarError("Seleccioná un vuelo.");
                return;
            }

            if (!_idAsientoSeleccionado.HasValue)
            {
                MostrarError("Seleccioná un asiento disponible.");
                return;
            }

            btnGuardar.Enabled = false;

            try
            {
                await _reservaClient.CrearAsync(
                    idPasajero,
                    idVuelo,
                    _idAsientoSeleccionado.Value,
                    SesionUsuario.IdUsuario);

                MessageBox.Show(
                    "Reserva creada correctamente.",
                    "SkyPlus",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (HttpRequestException)
            {
                MostrarError("No se pudo conectar con la API.");
            }
            catch (Exception ex)
            {
                MostrarError("No se pudo crear la reserva: " + ex.Message);
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