using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmVentas : Form
    {
        private class VentaDemo
        {
            public string CodigoReserva { get; set; } = string.Empty;
            public string Pasajero { get; set; } = string.Empty;
            public string Vuelo { get; set; } = string.Empty;
            public string Asiento { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public string MetodoPago { get; set; } = string.Empty;
            public string Fecha { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
        }

        private readonly List<VentaDemo> _ventas = new();

        public frmVentas()
        {
            InitializeComponent();

            CargarDatosDemo();
            ConfigurarTabla();
            AplicarFiltros();
        }

        private void CargarDatosDemo()
        {
            _ventas.Clear();

            _ventas.Add(new VentaDemo
            {
                CodigoReserva = "SP4K8D",
                Pasajero = "Gómez, Juan",
                Vuelo = "SP101",
                Asiento = "12A",
                Monto = 185000,
                MetodoPago = "Tarjeta",
                Fecha = "22/09/2026",
                Estado = "Pagado"
            });

            _ventas.Add(new VentaDemo
            {
                CodigoReserva = "SP7M2Q",
                Pasajero = "Fernández, María",
                Vuelo = "SP205",
                Asiento = "08C",
                Monto = 220000,
                MetodoPago = "Transferencia",
                Fecha = "22/09/2026",
                Estado = "Pagado"
            });

            _ventas.Add(new VentaDemo
            {
                CodigoReserva = "SP9L5A",
                Pasajero = "Rodríguez, Carlos",
                Vuelo = "SP310",
                Asiento = "21F",
                Monto = 175000,
                MetodoPago = "Tarjeta",
                Fecha = "21/09/2026",
                Estado = "Pagado"
            });

            _ventas.Add(new VentaDemo
            {
                CodigoReserva = "SP2N6B",
                Pasajero = "Martínez, Ana",
                Vuelo = "SP412",
                Asiento = "05B",
                Monto = 198000,
                MetodoPago = "Efectivo",
                Fecha = "21/09/2026",
                Estado = "Pagado"
            });
        }

        private void ConfigurarTabla()
        {
            dgvVentas.AutoGenerateColumns = false;
            dgvVentas.Columns.Clear();

            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colReserva",
                HeaderText = "Reserva",
                DataPropertyName = "CodigoReserva",
                Width = 90
            });

            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colPasajero",
                HeaderText = "Pasajero",
                DataPropertyName = "Pasajero",
                Width = 180
            });

            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colVuelo",
                HeaderText = "Vuelo",
                DataPropertyName = "Vuelo",
                Width = 80
            });

            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colAsiento",
                HeaderText = "Asiento",
                DataPropertyName = "Asiento",
                Width = 80
            });

            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMonto",
                HeaderText = "Monto",
                DataPropertyName = "Monto",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2"
                }
            });

            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMetodo",
                HeaderText = "Método de pago",
                DataPropertyName = "MetodoPago",
                Width = 130
            });

            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFecha",
                HeaderText = "Fecha",
                DataPropertyName = "Fecha",
                Width = 100
            });

            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colEstado",
                HeaderText = "Estado",
                DataPropertyName = "Estado",
                Width = 100
            });
        }

        private void AplicarFiltros()
        {
            string texto = txtBuscar.Text.Trim().ToLower();

            var resultado = _ventas
                .Where(v =>
                    string.IsNullOrEmpty(texto) ||
                    v.CodigoReserva.ToLower().Contains(texto) ||
                    v.Pasajero.ToLower().Contains(texto) ||
                    v.Vuelo.ToLower().Contains(texto))
                .ToList();

            dgvVentas.DataSource = null;
            dgvVentas.DataSource = resultado;

            lblCantidad.Text = $"Ventas: {resultado.Count}";
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
            if (dgvVentas.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione una venta.",
                    "Ventas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            MessageBox.Show(
                "Vista de detalle de venta.\n\n" +
                "Esta función estará disponible en una próxima versión.",
                "Detalle de venta",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void frmVentas_Load(object sender, EventArgs e)
        {
        }

    }
}