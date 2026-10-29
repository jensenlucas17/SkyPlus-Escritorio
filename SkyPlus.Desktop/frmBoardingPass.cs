using System.Diagnostics;
using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Utilidades;

namespace SkyPlus.Desktop
{
    public partial class frmBoardingPass : Form
    {
        private readonly CheckInClient _checkInClient = new CheckInClient();
        private List<CheckInResponse> _checkins = new List<CheckInResponse>();

        public frmBoardingPass()
        {
            InitializeComponent();
        }

        private async void frmBoardingPass_Load(object sender, EventArgs e)
        {
            ConfigurarTabla();

            AppEstilos.EstilizarGrid(dgvBoardingPass);
            AppEstilos.EstilizarBotonPrimario(btnVerImprimir);
            AppEstilos.EstilizarBotonPrimario(btnEmbarcar);
            AppEstilos.EstilizarBotonSecundario(btnBuscar);
            AppEstilos.EstilizarBotonSecundario(btnRefrescar);

            await CargarCheckins();
        }

        private void ConfigurarTabla()
        {
            dgvBoardingPass.AutoGenerateColumns = false;
            dgvBoardingPass.ReadOnly = true;
            dgvBoardingPass.AllowUserToAddRows = false;
            dgvBoardingPass.MultiSelect = false;
            dgvBoardingPass.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBoardingPass.Columns.Clear();

            dgvBoardingPass.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckInResponse.CodigoReserva), HeaderText = "Reserva", Width = 90 });
            dgvBoardingPass.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckInResponse.EstadoCheckin), HeaderText = "Estado", Width = 100 });
            dgvBoardingPass.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckInResponse.FechaLocal), HeaderText = "Fecha check-in", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
            dgvBoardingPass.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckInResponse.BoardingPass), HeaderText = "Detalle", Width = 380 });
        }

        private async Task CargarCheckins()
        {
            try
            {
                _checkins = await _checkInClient.ObtenerTodosAsync() ?? new List<CheckInResponse>();
                AplicarFiltros();
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
            }
        }

        private void AplicarFiltros()
        {
            var texto = txtBuscar.Text.Trim();

            var resultado = _checkins.Where(c =>
                string.IsNullOrEmpty(texto) ||
                c.CodigoReserva.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                c.BoardingPass.Contains(texto, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            dgvBoardingPass.DataSource = null;
            dgvBoardingPass.DataSource = resultado;
        }

        private CheckInResponse? ObtenerSeleccionado()
        {
            if (dgvBoardingPass.CurrentRow?.DataBoundItem is CheckInResponse checkin)
                return checkin;

            MessageBox.Show("Seleccioná un check-in de la lista.", "Boarding Pass",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private async void btnRefrescar_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            await CargarCheckins();
        }

        private async void btnVerImprimir_Click(object sender, EventArgs e)
        {
            var checkin = ObtenerSeleccionado();
            if (checkin == null) return;

            try
            {
                btnVerImprimir.Enabled = false;

                var pdf = await _checkInClient.DescargarBoardingPassAsync(checkin.IdCheckin);

                var ruta = Path.Combine(Path.GetTempPath(),
                    $"SkyPlus_BoardingPass_{checkin.CodigoReserva}_{DateTime.Now:HHmmss}.pdf");

                await File.WriteAllBytesAsync(ruta, pdf);

                Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
            }
            catch (IOException ex)
            {
                MessageBox.Show("No se pudo guardar el PDF: " + ex.Message, "Boarding Pass",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnVerImprimir.Enabled = true;
            }
        }

        private async void btnEmbarcar_Click(object sender, EventArgs e)
        {
            var checkin = ObtenerSeleccionado();
            if (checkin == null) return;

            if (checkin.EstadoCheckin == "Embarcado")
            {
                MessageBox.Show("El pasajero ya embarcó.", "Embarque", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Registrar el embarque de la reserva {checkin.CodigoReserva}?",
                "Registrar embarque", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                var resultado = await _checkInClient.RegistrarEmbarqueAsync(checkin.CodigoQr);

                MessageBox.Show(resultado?.Mensaje ?? "Embarque registrado.", "Embarque",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await CargarCheckins();
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
            }
        }
    }
}