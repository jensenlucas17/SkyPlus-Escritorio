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


        // Controles nuevos del diseño (los del diseñador se reubican en ConstruirPantalla)
        private readonly Label _lblDetalleVuelo = new Label();
        private readonly Label _lblAsiento = new Label();

        public frmReservaABM()
        {
            InitializeComponent();
            ConstruirPantalla();
            _mapaAsientos.AsientoSeleccionado += Mapa_AsientoSeleccionado;
        }

        // ---------------------------------------------------------------------
        // Diseño: datos de la reserva a la izquierda, avión con asientos a la derecha
        // ---------------------------------------------------------------------
        private void ConstruirPantalla()
        {
            Text = "SkyPlus - Nueva reserva";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(1000, 700);
            ClientSize = new Size(1120, 780);
            BackColor = AppEstilos.ColorSecundario;
            Font = AppEstilos.FuenteNormal;

            // Los controles del diseñador se sacan y se vuelven a ubicar abajo
            Controls.Clear();

            // ----- Encabezado -----
            var encabezado = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = AppEstilos.ColorLateral, Padding = new Padding(24, 0, 24, 0) };
            encabezado.Controls.Add(new Label
            {
                Text = "Nueva reserva",
                Dock = DockStyle.Left,
                Width = 260,
                Font = AppEstilos.FuenteTitulo,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            });
            encabezado.Controls.Add(new Label
            {
                Text = "Elegí pasajero, vuelo y asiento. Después se agrega el equipaje y se cobra.",
                Dock = DockStyle.Fill,
                Font = AppEstilos.FuenteNormal,
                ForeColor = AppEstilos.ColorLateralTexto,
                TextAlign = ContentAlignment.MiddleRight
            });
            encabezado.Controls[1].BringToFront();

            // ----- Columna izquierda: datos -----
            var izquierda = new Panel { Dock = DockStyle.Left, Width = 420, BackColor = Color.White, Padding = new Padding(24, 18, 24, 18) };
            izquierda.Paint += (s, e) =>
            {
                using var lapiz = new Pen(AppEstilos.ColorBorde);
                e.Graphics.DrawLine(lapiz, izquierda.Width - 1, 0, izquierda.Width - 1, izquierda.Height);
            };

            var datos = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.White
            };
            const int ancho = 360;

            Label Titulo(string texto) => new Label
            {
                Text = texto,
                AutoSize = true,
                Font = AppEstilos.FuenteBoton,
                ForeColor = AppEstilos.ColorTextoOscuro,
                Margin = new Padding(0, 10, 0, 4)
            };

            void Combo(ComboBox cmb)
            {
                cmb.Width = ancho;
                cmb.Font = AppEstilos.FuenteNormal;
                cmb.FlatStyle = FlatStyle.Flat;
                cmb.Margin = new Padding(0, 0, 0, 2);
            }

            // Pasajero + botón "Nuevo"
            var filaPasajero = new FlowLayoutPanel { AutoSize = true, WrapContents = false, BackColor = Color.White, Margin = Padding.Empty };
            Combo(cmbPasajero);
            cmbPasajero.Width = ancho - 100;
            btnNuevoPasajero.Text = "+ Nuevo";
            btnNuevoPasajero.Width = 92;
            btnNuevoPasajero.Margin = new Padding(8, 0, 0, 0);
            AppEstilos.EstilizarBotonSecundario(btnNuevoPasajero);
            btnNuevoPasajero.Height = 28;
            filaPasajero.Controls.Add(cmbPasajero);
            filaPasajero.Controls.Add(btnNuevoPasajero);

            Combo(cmbOrigen);
            Combo(cmbDestino);
            Combo(cmbVuelo);

            // Tarjeta con los datos del vuelo elegido
            _lblDetalleVuelo.AutoSize = false;
            _lblDetalleVuelo.Size = new Size(ancho, 112);
            _lblDetalleVuelo.Margin = new Padding(0, 12, 0, 0);
            _lblDetalleVuelo.Padding = new Padding(12, 10, 12, 10);
            _lblDetalleVuelo.BackColor = AppEstilos.ColorFilaAlterna;
            _lblDetalleVuelo.Font = AppEstilos.FuenteNormal;
            _lblDetalleVuelo.ForeColor = AppEstilos.ColorTextoSuave;
            _lblDetalleVuelo.Text = "Elegí origen, destino y vuelo para ver sus datos.";

            _lblAsiento.AutoSize = true;
            _lblAsiento.Font = AppEstilos.FuenteTitulo;
            _lblAsiento.ForeColor = AppEstilos.ColorTextoSuave;
            _lblAsiento.Text = "Sin elegir";
            _lblAsiento.Margin = new Padding(0, 0, 0, 4);

            lblMensaje.AutoSize = true;
            lblMensaje.MaximumSize = new Size(ancho, 0);
            lblMensaje.Font = AppEstilos.FuenteNormal;
            lblMensaje.ForeColor = AppEstilos.ColorError;
            lblMensaje.Margin = new Padding(0, 10, 0, 0);

            datos.Controls.Add(Titulo("Pasajero"));
            datos.Controls.Add(filaPasajero);
            datos.Controls.Add(Titulo("Origen"));
            datos.Controls.Add(cmbOrigen);
            datos.Controls.Add(Titulo("Destino"));
            datos.Controls.Add(cmbDestino);
            datos.Controls.Add(Titulo("Vuelo"));
            datos.Controls.Add(cmbVuelo);
            datos.Controls.Add(_lblDetalleVuelo);
            datos.Controls.Add(Titulo("Asiento elegido"));
            datos.Controls.Add(_lblAsiento);
            datos.Controls.Add(lblMensaje);

            // Total y botones abajo
            var pie = new Panel { Dock = DockStyle.Bottom, Height = 110, BackColor = Color.White };
            label9.Text = "Tarifa del pasaje";
            label9.AutoSize = false;
            label9.Font = AppEstilos.FuenteNormal;
            label9.ForeColor = AppEstilos.ColorTextoSuave;
            label9.SetBounds(0, 6, 180, 22);
            lblTarifa.AutoSize = false;
            lblTarifa.Font = AppEstilos.FuenteTitulo;
            lblTarifa.ForeColor = AppEstilos.ColorPrimario;
            lblTarifa.TextAlign = ContentAlignment.MiddleRight;
            lblTarifa.SetBounds(160, 0, ancho - 160, 34);

            btnCancelar.Text = "Cancelar";
            btnGuardar.Text = "Continuar";
            btnCancelar.SetBounds(0, 56, 120, 40);
            btnGuardar.SetBounds(ancho - 200, 56, 200, 40);

            pie.Controls.Add(label9);
            pie.Controls.Add(lblTarifa);
            pie.Controls.Add(btnCancelar);
            pie.Controls.Add(btnGuardar);

            izquierda.Controls.Add(datos);
            izquierda.Controls.Add(pie);
            datos.BringToFront();

            // ----- Derecha: avión -----
            var derecha = new Panel { Dock = DockStyle.Fill, BackColor = AppEstilos.ColorSecundario, Padding = new Padding(16) };
            _mapaAsientos.Dock = DockStyle.Fill;
            derecha.Controls.Add(_mapaAsientos);

            Controls.Add(derecha);
            Controls.Add(izquierda);
            Controls.Add(encabezado);


            CancelButton = btnCancelar;
        }

        private async void frmReservaABM_Load(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            AppEstilos.EstilizarBotonPrimario(btnGuardar);
            btnGuardar.Height = 40;
            btnCancelar.Height = 40;
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
        private AsientoResponse? _asientoSeleccionado;
        private async void cmbVuelo_SelectedIndexChanged(object sender, EventArgs e)
        {
            LimpiarAsientos();

            if (cmbVuelo.SelectedItem is not VueloResponse vuelo)
            {
                lblTarifa.Text = "$ 0";
                _lblDetalleVuelo.Text = "Elegí origen, destino y vuelo para ver sus datos.";
                return;
            }

            lblTarifa.Text = vuelo.Tarifa.ToString("C0");
            _lblDetalleVuelo.Text =
                $"{vuelo.NumeroVuelo}  ·  {vuelo.EstadoVuelo}\n" +
                $"Sale: {vuelo.Salida:dddd dd/MM/yyyy HH:mm}\n" +
                $"Llega: {vuelo.Llegada:dd/MM HH:mm}\n" +
                $"Aeronave: {vuelo.Aeronave}";

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

                int libres = lista.Count(a => string.Equals(a.Estado, "Libre", StringComparison.OrdinalIgnoreCase));
                _lblDetalleVuelo.Text += $"\nAsientos libres: {libres} de {lista.Count}";

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
            _asientoSeleccionado = null;
            _lblAsiento.Text = "Sin elegir";
            _lblAsiento.ForeColor = AppEstilos.ColorTextoSuave;
            _mapaAsientos.Cargar(asientos);
        }

        private void Mapa_AsientoSeleccionado(
            object? sender,
            AsientoResponse asiento)
        {
            _idAsientoSeleccionado = asiento.IdAsiento;
            _asientoSeleccionado = asiento;
            _lblAsiento.Text = $"{asiento.Fila}{asiento.Letra}  ·  {(asiento.Letra is "A" or "F" ? "ventanilla" : asiento.Letra is "C" or "D" ? "pasillo" : "medio")}";
            _lblAsiento.ForeColor = AppEstilos.ColorPrimario;
            lblMensaje.Visible = false;
        }

        private void LimpiarAsientos()
        {
            _idAsientoSeleccionado = null;
            _asientoSeleccionado = null;
            _mapaAsientos.Cargar(new List<AsientoResponse>());
            lblTarifa.Text = "$ 0";
            _lblAsiento.Text = "Sin elegir";
            _lblAsiento.ForeColor = AppEstilos.ColorTextoSuave;
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

            if (cmbVuelo.SelectedItem is not VueloResponse vuelo || _asientoSeleccionado == null)
            {
                MostrarError("Seleccioná un vuelo y un asiento disponible.");
                return;
            }

            // Regla: el pasajero no puede tener otra reserva en este vuelo ni en un vuelo superpuesto
            btnGuardar.Enabled = false;
            string? conflicto = await BuscarConflictoPasajeroAsync(idPasajero, vuelo);
            btnGuardar.Enabled = true;
            if (conflicto != null)
            {
                MessageBox.Show(conflicto, "Reserva no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                MostrarError(conflicto);
                return;
            }

            // Siguiente paso de la venta: equipaje y pago (la reserva se crea al confirmar el pago)
            using var confirmar = new frmConfirmarVenta(
                idPasajero,
                cmbPasajero.Text,
                vuelo,
                _asientoSeleccionado);

            if (confirmar.ShowDialog(this) == DialogResult.OK)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                // Si el asiento se ocupó mientras tanto, se actualiza el mapa
                cmbVuelo_SelectedIndexChanged(cmbVuelo, EventArgs.Empty);
            }
        }

        // Devuelve el motivo si el pasajero ya está ocupado en ese horario; null si puede viajar.
        // (La API valida lo mismo; acá se avisa antes de pasar al equipaje y al pago.)
        private async Task<string?> BuscarConflictoPasajeroAsync(int idPasajero, VueloResponse vuelo)
        {
            try
            {
                var tReservas = _reservaClient.ObtenerTodosAsync();
                var tVuelos = new VueloClient().ObtenerTodosAsync();
                await Task.WhenAll(tReservas, tVuelos);

                var vuelos = (tVuelos.Result ?? new List<VueloResponse>()).ToDictionary(v => v.IdVuelo);
                var activas = (tReservas.Result ?? new List<ReservaResponse>())
                    .Where(r => r.IdPasajero == idPasajero && r.EstadoReserva == "Confirmada");

                foreach (var r in activas)
                {
                    if (r.IdVuelo == vuelo.IdVuelo)
                        return $"{r.Pasajero} ya tiene un asiento reservado en este vuelo (reserva {r.CodigoReserva}, asiento {r.Asiento}).";

                    if (vuelos.TryGetValue(r.IdVuelo, out var otro) &&
                        otro.EstadoVuelo != "Cancelado" &&
                        otro.Salida < vuelo.Llegada && otro.Llegada > vuelo.Salida)
                        return $"{r.Pasajero} ya viaja en el vuelo {otro.NumeroVuelo} " +
                               $"({otro.Salida:dd/MM HH:mm} a {otro.Llegada:HH:mm}, reserva {r.CodigoReserva}), " +
                               "que se superpone con este horario.";
                }
                return null;
            }
            catch (HttpRequestException)
            {
                return null; // si no se pudo consultar, la API lo valida igual al confirmar
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