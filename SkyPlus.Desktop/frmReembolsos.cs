using SkyPlus.Desktop.Estilos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmReembolsos : Form
    {
        private class ReembolsoDemo
        {
            public string CodigoReserva { get; set; } = string.Empty;
            public string Pasajero { get; set; } = string.Empty;
            public string Vuelo { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public string Motivo { get; set; } = string.Empty;
            public string Fecha { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
        }

        private readonly List<ReembolsoDemo> _reembolsos = new();

        // IMPORTANTE: InitializeComponent va acá
        public frmReembolsos()
        {
            InitializeComponent();
        }

        private void frmReembolsos_Load(object sender, EventArgs e)
        {
            CargarDatosDemo();
            ConfigurarTabla();
            AplicarFiltros();

            AppEstilos.EstilizarGrid(dgvReembolsos);
            AppEstilos.EstilizarBotonPrimario(btnBuscar);
            AppEstilos.EstilizarBotonSecundario(btnRefrescar);
            AppEstilos.EstilizarBotonSecundario(btnVerDetalle);
        }

        private void CargarDatosDemo()
        {
            _reembolsos.Clear();

            _reembolsos.Add(new ReembolsoDemo
            {
                CodigoReserva = "SP3A7K",
                Pasajero = "Pérez, Laura",
                Vuelo = "SP120",
                Monto = 165000,
                Motivo = "Cancelación de reserva",
                Fecha = "22/09/2026",
                Estado = "Procesado"
            });

            _reembolsos.Add(new ReembolsoDemo
            {
                CodigoReserva = "SP8B2M",
                Pasajero = "García, Diego",
                Vuelo = "SP214",
                Monto = 210000,
                Motivo = "Cancelación solicitada",
                Fecha = "21/09/2026",
                Estado = "Procesado"
            });

            _reembolsos.Add(new ReembolsoDemo
            {
                CodigoReserva = "SP5C9R",
                Pasajero = "López, Sofía",
                Vuelo = "SP305",
                Monto = 180000,
                Motivo = "Vuelo cancelado",
                Fecha = "20/09/2026",
                Estado = "Procesado"
            });

            _reembolsos.Add(new ReembolsoDemo
            {
                CodigoReserva = "SP1D4T",
                Pasajero = "Sánchez, Martín",
                Vuelo = "SP410",
                Monto = 195000,
                Motivo = "Solicitud del pasajero",
                Fecha = "20/09/2026",
                Estado = "Pendiente"
            });
        }

        private void ConfigurarTabla()
        {
            dgvReembolsos.AutoGenerateColumns = false;
            dgvReembolsos.Columns.Clear();

            dgvReembolsos.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgvReembolsos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colReserva",
                HeaderText = "Reserva",
                DataPropertyName = "CodigoReserva",
                FillWeight = 80
            });

            dgvReembolsos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colPasajero",
                HeaderText = "Pasajero",
                DataPropertyName = "Pasajero",
                FillWeight = 150
            });

            dgvReembolsos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colVuelo",
                HeaderText = "Vuelo",
                DataPropertyName = "Vuelo",
                FillWeight = 70
            });

            dgvReembolsos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMonto",
                HeaderText = "Monto",
                DataPropertyName = "Monto",
                FillWeight = 100,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2"
                }
            });

            dgvReembolsos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMotivo",
                HeaderText = "Motivo",
                DataPropertyName = "Motivo",
                FillWeight = 160
            });

            dgvReembolsos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFecha",
                HeaderText = "Fecha",
                DataPropertyName = "Fecha",
                FillWeight = 90
            });

            dgvReembolsos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colEstado",
                HeaderText = "Estado",
                DataPropertyName = "Estado",
                FillWeight = 90
            });
        }

        private void AplicarFiltros()
        {
            string texto = txtBuscar.Text.Trim().ToLower();

            var resultado = _reembolsos
                .Where(r =>
                    string.IsNullOrEmpty(texto) ||
                    r.CodigoReserva.ToLower().Contains(texto) ||
                    r.Pasajero.ToLower().Contains(texto) ||
                    r.Vuelo.ToLower().Contains(texto) ||
                    r.Motivo.ToLower().Contains(texto))
                .ToList();

            dgvReembolsos.DataSource = null;
            dgvReembolsos.DataSource = resultado;

            lblCantidad.Text = $"Reembolsos: {resultado.Count}";
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void btnRefrescar_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            CargarDatosDemo();
            AplicarFiltros();
        }

        private void btnVerDetalle_Click(object sender, EventArgs e)
        {
            if (dgvReembolsos.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione un reembolso.",
                    "Reembolsos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            MessageBox.Show(
                "Vista de detalle del reembolso.\n\n" +
                "Esta función estará disponible en una próxima versión.",
                "Detalle de reembolso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}