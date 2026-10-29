using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Utilidades;

namespace SkyPlus.Desktop
{
    public partial class frmCheckin : Form
    {
        private readonly CheckInClient _checkInClient = new CheckInClient();
        private readonly ReservaClient _reservaClient = new ReservaClient();
        private ReservaResponse? _reservaActual;

        public frmCheckin()
        {
            InitializeComponent();
        }

        private async void frmCheckin_Load(object sender, EventArgs e)
        {
            ConfigurarTabla();

            AppEstilos.EstilizarGrid(dgvCheckin);
            AppEstilos.EstilizarBotonPrimario(btnRealizarCheckin);
            AppEstilos.EstilizarBotonSecundario(btnBuscar);
            AppEstilos.EstilizarBotonSecundario(btnRefrescar);

            LimpiarEstado();
            await CargarCheckins();
        }

        private void ConfigurarTabla()
        {
            dgvCheckin.AutoGenerateColumns = false;
            dgvCheckin.ReadOnly = true;
            dgvCheckin.AllowUserToAddRows = false;
            dgvCheckin.MultiSelect = false;
            dgvCheckin.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCheckin.Columns.Clear();

            dgvCheckin.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckInResponse.CodigoReserva), HeaderText = "Reserva", Width = 90 });
            dgvCheckin.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckInResponse.EstadoCheckin), HeaderText = "Estado", Width = 100 });
            dgvCheckin.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckInResponse.FechaLocal), HeaderText = "Fecha check-in", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
            dgvCheckin.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckInResponse.BoardingPass), HeaderText = "Detalle", Width = 380 });
        }

        private async Task CargarCheckins()
        {
            try
            {
                var lista = await _checkInClient.ObtenerTodosAsync();
                dgvCheckin.DataSource = null;
                dgvCheckin.DataSource = lista ?? new List<CheckInResponse>();
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
            }
        }

        private void LimpiarEstado()
        {
            _reservaActual = null;
            lblEstado.Text = "Ingresá el código de la reserva y presioná Buscar.";
            lblEstado.ForeColor = SystemColors.ControlText;
            btnRealizarCheckin.Enabled = false;
        }

        private async void btnBuscar_Click(object sender, EventArgs e)
        {
            var codigo = txtBuscar.Text.Trim();

            if (string.IsNullOrWhiteSpace(codigo))
            {
                MessageBox.Show("Ingresá el código de la reserva.", "Check-in", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var reservas = await _reservaClient.ObtenerTodosAsync() ?? new List<ReservaResponse>();

                var reserva = reservas.FirstOrDefault(r =>
                    string.Equals(r.CodigoReserva, codigo, StringComparison.OrdinalIgnoreCase));

                if (reserva == null)
                {
                    LimpiarEstado();
                    lblEstado.Text = "No existe una reserva con ese código.";
                    lblEstado.ForeColor = Color.Firebrick;
                    return;
                }

                _reservaActual = reserva;

                var estado = await _checkInClient.ConsultarEstadoAsync(reserva.IdReserva);
                bool puede = estado?.PuedeRealizarCheckIn == true;

                lblEstado.Text =
                    $"{reserva.CodigoReserva} · {reserva.Pasajero} · Vuelo {reserva.Vuelo} · Asiento {reserva.Asiento}\n" +
                    (estado?.Mensaje ?? "No se pudo consultar el estado.");

                lblEstado.ForeColor = puede ? Color.SeaGreen : Color.Firebrick;
                btnRealizarCheckin.Enabled = puede;
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
            }
        }

        private async void btnRefrescar_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            LimpiarEstado();
            await CargarCheckins();
        }

        private async void btnRealizarCheckin_Click(object sender, EventArgs e)
        {
            if (_reservaActual == null)
            {
                MessageBox.Show("Primero buscá una reserva.", "Check-in", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Confirmás el check-in de {_reservaActual.Pasajero} (reserva {_reservaActual.CodigoReserva})?",
                "Realizar check-in", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                btnRealizarCheckin.Enabled = false;

                await _checkInClient.CrearAsync(_reservaActual.IdReserva);

                MessageBox.Show("Check-in realizado correctamente. Ya podés generar el boarding pass.",
                    "Check-in", MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtBuscar.Clear();
                LimpiarEstado();
                await CargarCheckins();
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
                btnRealizarCheckin.Enabled = true;
            }
        }
    }
}