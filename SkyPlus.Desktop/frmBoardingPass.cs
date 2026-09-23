// SkyPlus.Desktop/frmBoardingPass.cs
// Datos locales de demostracion - sin conexion a API. Funcionalidad completa: 2da entrega.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmBoardingPass : Form
    {
        private class BoardingPassDemo
        {
            public string CodigoReserva { get; set; } = string.Empty;
            public string Pasajero { get; set; } = string.Empty;
            public string Vuelo { get; set; } = string.Empty;
            public string Asiento { get; set; } = string.Empty;
            public string Puerta { get; set; } = string.Empty;
            public string HoraEmbarque { get; set; } = string.Empty;
        }

        private readonly List<BoardingPassDemo> _boardingPasses = new();

        public frmBoardingPass()
        {
            InitializeComponent();
        }

        private void frmBoardingPass_Load(object sender, EventArgs e)
        {
            CargarDatosDemo();
            ConfigurarTabla();
            AplicarFiltros();

            AppEstilos.EstilizarGrid(dgvBoardingPass);
            AppEstilos.EstilizarBotonPrimario(btnVerImprimir);
            AppEstilos.EstilizarBotonSecundario(btnBuscar);
            AppEstilos.EstilizarBotonSecundario(btnRefrescar);
        }

        private void CargarDatosDemo()
        {
            _boardingPasses.Clear();

            _boardingPasses.Add(new BoardingPassDemo { CodigoReserva = "SP7M2Q", Pasajero = "Fernández, María", Vuelo = "SP205", Asiento = "08C", Puerta = "B12", HoraEmbarque = "07:45" });
            _boardingPasses.Add(new BoardingPassDemo { CodigoReserva = "SP4K8D", Pasajero = "Gómez, Juan", Vuelo = "SP101", Asiento = "12A", Puerta = "A03", HoraEmbarque = "09:20" });
        }

        private void ConfigurarTabla()
        {
            dgvBoardingPass.AutoGenerateColumns = false;
            dgvBoardingPass.Columns.Clear();

            dgvBoardingPass.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(BoardingPassDemo.CodigoReserva), HeaderText = "Reserva", Width = 90 });
            dgvBoardingPass.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(BoardingPassDemo.Pasajero), HeaderText = "Pasajero", Width = 180 });
            dgvBoardingPass.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(BoardingPassDemo.Vuelo), HeaderText = "Vuelo", Width = 80 });
            dgvBoardingPass.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(BoardingPassDemo.Asiento), HeaderText = "Asiento", Width = 80 });
            dgvBoardingPass.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(BoardingPassDemo.Puerta), HeaderText = "Puerta", Width = 70 });
            dgvBoardingPass.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(BoardingPassDemo.HoraEmbarque), HeaderText = "Hora embarque", Width = 120 });
        }

        private void AplicarFiltros()
        {
            var texto = txtBuscar.Text.Trim().ToLower();

            var resultado = _boardingPasses.Where(b =>
                string.IsNullOrEmpty(texto) ||
                b.CodigoReserva.ToLower().Contains(texto) ||
                b.Pasajero.ToLower().Contains(texto) ||
                b.Vuelo.ToLower().Contains(texto)
            ).ToList();

            dgvBoardingPass.DataSource = null;
            dgvBoardingPass.DataSource = resultado;
        }

        private void btnBuscar_Click(object sender, EventArgs e) => AplicarFiltros();

        private void btnRefrescar_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            CargarDatosDemo();
            AplicarFiltros();
        }

        private void btnVerImprimir_Click(object sender, EventArgs e)
        {
            if (dgvBoardingPass.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un boarding pass.", "Boarding Pass", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show(
                "La generación e impresión del boarding pass estará disponible en una próxima versión.",
                "Boarding Pass", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}