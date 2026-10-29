// SkyPlus.Desktop/frmReservaABM.cs

using System;
using System.Linq;
using System.Net.Http;
using System.Drawing;
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
        private readonly ucMapaAsientos _mapaAsientos = new ucMapaAsientos();


        public frmReservaABM()
        {
            InitializeComponent();

            // Configuración de la ventana
            Text = "SkyPlus - Nueva reserva";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;

            MinimumSize = new Size(780, 650);
            ClientSize = new Size(900, 740);

            // Ampliar los campos de selección
            cmbPasajero.Width = 230;
            cmbOrigen.Width = 230;
            cmbDestino.Width = 230;
            cmbVuelo.Width = 300;

            btnNuevoPasajero.Left = cmbPasajero.Right + 15;

            // Configurar el contenedor del mapa
            flpAsientos.Controls.Clear();
            flpAsientos.AutoScroll = false;
            flpAsientos.WrapContents = false;
            flpAsientos.FlowDirection = FlowDirection.LeftToRight;
            flpAsientos.Padding = Padding.Empty;
            flpAsientos.Margin = Padding.Empty;

            flpAsientos.Location = new Point(33, 270);
            flpAsientos.Size = new Size(820, 360);

            flpAsientos.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            // Incorporar el mapa visual
            _mapaAsientos.Dock = DockStyle.None;
            _mapaAsientos.Margin = Padding.Empty;
            _mapaAsientos.MinimumSize = Size.Empty;
            _mapaAsientos.Location = Point.Empty;
            _mapaAsientos.Size = flpAsientos.ClientSize;

            flpAsientos.Controls.Add(_mapaAsientos);

            // Adaptar el mapa al tamaño del contenedor
            flpAsientos.ClientSizeChanged += (sender, e) =>
            {
                _mapaAsientos.Size = flpAsientos.ClientSize;
            };

            _mapaAsientos.AsientoSeleccionado +=
                Mapa_AsientoSeleccionado;

            // Reubicar los controles inferiores
            label9.Location = new Point(33, 655);
            lblTarifa.Location = new Point(560, 655);

            btnCancelar.Location = new Point(615, 685);
            btnGuardar.Location = new Point(710, 685);

            btnCancelar.Size = new Size(90, 32);
            btnGuardar.Size = new Size(100, 32);

            // Mantener los controles inferiores alineados
            label9.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Left;

            lblTarifa.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Right;

            btnCancelar.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Right;

            btnGuardar.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Right;

            // Mensajes de validación
            lblMensaje.Location = new Point(130, 655);
            lblMensaje.MaximumSize = new Size(400, 0);
            lblMensaje.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Left;
        }


        private async void frmReservaABM_Load(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            AppEstilos.EstilizarBotonPrimario(btnGuardar);
            AppEstilos.EstilizarBotonSecundario(btnCancelar);

            try
            {
                await CargarPasajeros();
                await CargarVuelos();
            }
            catch (HttpRequestException ex)
            {
                MostrarError(ObtenerMensajeHttp(ex));
            }
            catch (Exception ex)
            {
                MostrarError("No se pudieron cargar los datos: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task CargarPasajeros()
        {
            var pasajeros = (await _pasajeroClient.ObtenerTodosAsync()
                ?? new List<PasajeroResponse>())
                .Where(p => string.Equals(p.Estado, "Activo",
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

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
                try
                {
                    await CargarPasajeros();
                }
                catch (HttpRequestException ex)
                {
                    MostrarError(ObtenerMensajeHttp(ex));
                }
                catch (Exception ex)
                {
                    MostrarError("No se pudo actualizar la lista de pasajeros: " + ex.Message);
                }
            }
        }

        private List<VueloResponse> _vuelos = new();

        private async System.Threading.Tasks.Task CargarVuelos()
        {
            _vuelos = (await _vueloClient.ObtenerTodosAsync()
                      ?? new List<VueloResponse>())
                .Where(v =>
                    string.Equals(v.EstadoVuelo, "Programado", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(v.EstadoVuelo, "Confirmado", StringComparison.OrdinalIgnoreCase))
                .ToList();

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

                if (cmbVuelo.SelectedItem is not VueloResponse vueloActual ||
                    vueloActual.IdVuelo != vuelo.IdVuelo)
                {
                    return;
                }

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
            catch (HttpRequestException ex)
            {
                MostrarError(ObtenerMensajeHttp(ex));
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
            _idAsientoSeleccionado = null;
            _mapaAsientos.Cargar(asientos);
        }

        private void Mapa_AsientoSeleccionado(
            object? sender,
            AsientoResponse asiento)
        {
            _idAsientoSeleccionado = asiento.IdAsiento;
        }

        private void LimpiarAsientos()
        {
            _idAsientoSeleccionado = null;
            _mapaAsientos.Cargar(new List<AsientoResponse>());
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

            if (SesionUsuario.IdUsuario <= 0)
            {
                MostrarError("No se pudo identificar al agente de la sesión.");
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
            catch (HttpRequestException ex)
            {
                MostrarError(ObtenerMensajeHttp(ex));
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

        private static string ObtenerMensajeHttp(HttpRequestException ex)
        {
            return ex.StatusCode.HasValue
                ? ex.Message
                : "No se pudo conectar con la API de SkyPlus. " +
                  "Verificá que esté ejecutándose.";
        }

        private void MostrarError(string mensaje)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.Visible = true;
        }
    }
}