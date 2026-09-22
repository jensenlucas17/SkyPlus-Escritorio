// SkyPlus.Desktop/frmReservaABM.cs

using System;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;

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

            cmbPasajero.DisplayMember = nameof(PasajeroResponse.NombreCompleto);
            cmbPasajero.ValueMember = nameof(PasajeroResponse.IdPasajero);
            cmbPasajero.DataSource = pasajeros;
        }

        private async System.Threading.Tasks.Task CargarVuelos()
        {
            var vuelos = await _vueloClient.ObtenerTodosAsync();

            cmbVuelo.DisplayMember = nameof(VueloResponse.NumeroVuelo);
            cmbVuelo.ValueMember = nameof(VueloResponse.IdVuelo);
            cmbVuelo.DataSource = vuelos;
        }

        private async void cmbVuelo_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbAsiento.DataSource = null;
            cmbAsiento.Items.Clear();

            if (cmbVuelo.SelectedValue is not int idVuelo) return;

            try
            {
                var asientos = await _asientoClient.ObtenerPorVueloAsync(idVuelo);

                var disponibles = (asientos ?? new System.Collections.Generic.List<AsientoResponse>())
                    .Where(a => a.Estado == "Libre")
                    .ToList();

                cmbAsiento.DisplayMember = nameof(AsientoResponse.CodigoAsiento);
                cmbAsiento.ValueMember = nameof(AsientoResponse.IdAsiento);
                cmbAsiento.DataSource = disponibles;

                var vuelo = (cmbVuelo.SelectedItem as VueloResponse);
                lblTarifa.Text = vuelo != null ? $"Tarifa: {vuelo.Tarifa:C0}" : string.Empty;

                if (disponibles.Count == 0)
                {
                    MostrarError("Este vuelo no tiene asientos disponibles.");
                }
                else
                {
                    lblMensaje.Visible = false;
                }
            }
            catch (HttpRequestException)
            {
                MostrarError("No se pudo obtener los asientos del vuelo.");
            }
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            if (cmbPasajero.SelectedValue is not int idPasajero)
            {
                MostrarError("Seleccioná un pasajero.");
                return;
            }

            if (cmbVuelo.SelectedValue is not int idVuelo)
            {
                MostrarError("Seleccioná un vuelo.");
                return;
            }

            if (cmbAsiento.SelectedValue is not int idAsiento)
            {
                MostrarError("Seleccioná un asiento disponible.");
                return;
            }

            btnGuardar.Enabled = false;

            try
            {
                await _reservaClient.CrearAsync(idPasajero, idVuelo, idAsiento, SesionUsuario.IdUsuario);

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