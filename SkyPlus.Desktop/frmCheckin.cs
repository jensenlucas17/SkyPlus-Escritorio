// SkyPlus.Desktop/frmCheckin.cs
// TODO: reemplazar datos demo por CheckinClient real cuando Lucas lo arme.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmCheckin : Form
    {
        private class CheckinDemo
        {
            public string CodigoReserva { get; set; } = string.Empty;
            public string Pasajero { get; set; } = string.Empty;
            public string Vuelo { get; set; } = string.Empty;
            public string Asiento { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
            public string FechaCheckin { get; set; } = string.Empty;
        }

        private readonly List<CheckinDemo> _checkins = new();

        public frmCheckin()
        {
            InitializeComponent();
        }

        private void frmCheckin_Load(object sender, EventArgs e)
        {
            CargarDatosDemo();
            ConfigurarTabla();
            AplicarFiltros();

            AppEstilos.EstilizarGrid(dgvCheckin);
            AppEstilos.EstilizarBotonPrimario(btnRealizarCheckin);
            AppEstilos.EstilizarBotonSecundario(btnBuscar);
            AppEstilos.EstilizarBotonSecundario(btnRefrescar);
        }

        private void CargarDatosDemo()
        {
            _checkins.Clear();

            _checkins.Add(new CheckinDemo { CodigoReserva = "SP4K8D", Pasajero = "Gómez, Juan", Vuelo = "SP101", Asiento = "12A", Estado = "Pendiente", FechaCheckin = "-" });
            _checkins.Add(new CheckinDemo { CodigoReserva = "SP7M2Q", Pasajero = "Fernández, María", Vuelo = "SP205", Asiento = "08C", Estado = "Realizado", FechaCheckin = "22/09/2026 07:15" });
            _checkins.Add(new CheckinDemo { CodigoReserva = "SP9L5A", Pasajero = "Rodríguez, Carlos", Vuelo = "SP310", Asiento = "21F", Estado = "Pendiente", FechaCheckin = "-" });
        }

        private void ConfigurarTabla()
        {
            dgvCheckin.AutoGenerateColumns = false;
            dgvCheckin.Columns.Clear();

            dgvCheckin.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckinDemo.CodigoReserva), HeaderText = "Reserva", Width = 90 });
            dgvCheckin.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckinDemo.Pasajero), HeaderText = "Pasajero", Width = 180 });
            dgvCheckin.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckinDemo.Vuelo), HeaderText = "Vuelo", Width = 80 });
            dgvCheckin.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckinDemo.Asiento), HeaderText = "Asiento", Width = 80 });
            dgvCheckin.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckinDemo.Estado), HeaderText = "Estado", Width = 100 });
            dgvCheckin.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckinDemo.FechaCheckin), HeaderText = "Fecha check-in", Width = 140 });
        }

        private void AplicarFiltros()
        {
            var texto = txtBuscar.Text.Trim().ToLower();

            var resultado = _checkins.Where(c =>
                string.IsNullOrEmpty(texto) ||
                c.CodigoReserva.ToLower().Contains(texto) ||
                c.Pasajero.ToLower().Contains(texto) ||
                c.Vuelo.ToLower().Contains(texto)
            ).ToList();

            dgvCheckin.DataSource = null;
            dgvCheckin.DataSource = resultado;
        }

        private void btnBuscar_Click(object sender, EventArgs e) => AplicarFiltros();

        private void btnRefrescar_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            CargarDatosDemo();
            AplicarFiltros();
        }

        private void btnRealizarCheckin_Click(object sender, EventArgs e)
        {
            if (dgvCheckin.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una reserva.", "Check-in", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show(
                "Esta función estará disponible en una próxima versión.",
                "Realizar check-in", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}