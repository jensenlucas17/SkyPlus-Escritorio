
using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmReembolsos : Form
    {
        private readonly ReembolsoClient _cliente = new();
        private List<ReembolsoResponse> _reembolsos = new();

        private readonly Button btnRegistrarReembolso = new()
        {
            Name = "btnRegistrarReembolso",
            Text = "Registrar reembolso",
            Size = new System.Drawing.Size(155, 30),
            Location = new System.Drawing.Point(340, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        public frmReembolsos()
        {
            InitializeComponent();

            pnlBotones.Controls.Add(btnRegistrarReembolso);

            btnRegistrarReembolso.Click +=
                btnRegistrarReembolso_Click;
        }

        private async void frmReembolsos_Load(
            object sender, EventArgs e)
        {
            ConfigurarTabla();

            AppEstilos.EstilizarGrid(dgvReembolsos);
            AppEstilos.EstilizarBotonPrimario(btnBuscar);
            AppEstilos.EstilizarBotonSecundario(btnRefrescar);
            AppEstilos.EstilizarBotonSecundario(btnVerDetalle);
            AppEstilos.EstilizarBotonPrimario(btnRegistrarReembolso);

            await CargarReembolsosAsync();
        }

        private async Task CargarReembolsosAsync()
        {
            btnRefrescar.Enabled = false;

            try
            {
                _reembolsos = await _cliente.ObtenerTodosAsync();
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudieron cargar los reembolsos.\n\n{ex.Message}",
                    "SkyPlus",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                    btnRefrescar.Enabled = true;
            }
        }

        private void ConfigurarTabla()
        {
            dgvReembolsos.AutoGenerateColumns = false;
            dgvReembolsos.Columns.Clear();

            dgvReembolsos.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgvReembolsos.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvReembolsos.MultiSelect = false;
            dgvReembolsos.ReadOnly = true;
            dgvReembolsos.AllowUserToAddRows = false;

            dgvReembolsos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colReserva",
                    HeaderText = "Reserva",
                    DataPropertyName = "CodigoReserva",
                    FillWeight = 80
                });

            dgvReembolsos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colPasajero",
                    HeaderText = "Pasajero",
                    DataPropertyName = "Pasajero",
                    FillWeight = 150
                });

            dgvReembolsos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colVuelo",
                    HeaderText = "Vuelo",
                    DataPropertyName = "Vuelo",
                    FillWeight = 70
                });

            dgvReembolsos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colMonto",
                    HeaderText = "Monto",
                    DataPropertyName = "MontoReembolsado",
                    FillWeight = 100,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "C2"
                    }
                });

            dgvReembolsos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colMotivo",
                    HeaderText = "Motivo",
                    DataPropertyName = "Motivo",
                    FillWeight = 160
                });

            dgvReembolsos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colFecha",
                    HeaderText = "Fecha",
                    DataPropertyName = "FechaReembolso",
                    FillWeight = 100,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "dd/MM/yyyy HH:mm"
                    }
                });

            dgvReembolsos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colEstado",
                    HeaderText = "Estado",
                    DataPropertyName = "EstadoReembolso",
                    FillWeight = 90
                });
        }

        private void AplicarFiltros()
        {
            string texto = txtBuscar.Text.Trim();

            var resultado = _reembolsos
                .Where(r =>
                    string.IsNullOrWhiteSpace(texto) ||
                    r.CodigoReserva.Contains(
                        texto, StringComparison.OrdinalIgnoreCase) ||
                    r.Pasajero.Contains(
                        texto, StringComparison.OrdinalIgnoreCase) ||
                    r.Vuelo.Contains(
                        texto, StringComparison.OrdinalIgnoreCase) ||
                    r.Motivo.Contains(
                        texto, StringComparison.OrdinalIgnoreCase) ||
                    r.EstadoReembolso.Contains(
                        texto, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.FechaReembolso)
                .ToList();

            dgvReembolsos.DataSource = null;
            dgvReembolsos.DataSource = resultado;

            lblCantidad.Text = $"Reembolsos: {resultado.Count}";
        }

        private void txtBuscar_TextChanged(
            object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void btnBuscar_Click(
            object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private async void btnRefrescar_Click(
            object sender, EventArgs e)
        {
            txtBuscar.Clear();
            await CargarReembolsosAsync();
        }

        private void btnVerDetalle_Click(
            object sender, EventArgs e)
        {
            if (dgvReembolsos.CurrentRow?.DataBoundItem
                is not ReembolsoResponse reembolso)
            {
                MessageBox.Show(
                    "Seleccioná un reembolso.",
                    "Reembolsos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string detalle =
                $"Reembolso N.º: {reembolso.IdReembolso}\n" +
                $"Pago N.º: {reembolso.IdPago}\n" +
                $"Reserva: {reembolso.CodigoReserva}\n" +
                $"Pasajero: {reembolso.Pasajero}\n" +
                $"Vuelo: {reembolso.Vuelo}\n\n" +
                $"Monto: {reembolso.MontoReembolsado:C2}\n" +
                $"Motivo: {reembolso.Motivo}\n" +
                $"Fecha: {reembolso.FechaReembolso:dd/MM/yyyy HH:mm}\n" +
                $"Estado: {reembolso.EstadoReembolso}";

            MessageBox.Show(
                detalle,
                "Detalle del reembolso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private async void btnRegistrarReembolso_Click(
    object? sender, EventArgs e)
        {
            using var formulario = new frmRegistrarReembolso();

            if (formulario.ShowDialog(this) == DialogResult.OK)
            {
                await CargarReembolsosAsync();
            }
        }
    }
}
