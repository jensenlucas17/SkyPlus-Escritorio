// SkyPlus.Desktop/frmReportes.cs
// Datos locales de demostracion - sin conexion a API. Funcionalidad completa: 2da entrega.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmReportes : Form
    {
        private class ReporteFilaDemo
        {
            public string Vuelo { get; set; } = string.Empty;
            public string Fecha { get; set; } = string.Empty;
            public int AsientosVendidos { get; set; }
            public decimal Recaudacion { get; set; }
        }

        public frmReportes()
        {
            InitializeComponent();
        }

        private void frmReportes_Load(object sender, EventArgs e)
        {
            cmbTipoReporte.Items.Clear();
            cmbTipoReporte.Items.AddRange(new object[]
            {
                "Ventas por vuelo",
                "Ocupación por vuelo",
                "Cancelaciones por período",
                "Ingresos por método de pago"
            });
            cmbTipoReporte.SelectedIndex = 0;

            dtpDesde.Value = DateTime.Today.AddDays(-7);
            dtpHasta.Value = DateTime.Today;

            ConfigurarTabla();

            AppEstilos.EstilizarGrid(dgvReporte);
            AppEstilos.EstilizarBotonPrimario(btnGenerar);
        }

        private void ConfigurarTabla()
        {
            dgvReporte.AutoGenerateColumns = false;
            dgvReporte.Columns.Clear();

            dgvReporte.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReporteFilaDemo.Vuelo), HeaderText = "Vuelo", Width = 90 });
            dgvReporte.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReporteFilaDemo.Fecha), HeaderText = "Fecha", Width = 100 });
            dgvReporte.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReporteFilaDemo.AsientosVendidos), HeaderText = "Asientos vendidos", Width = 130 });
            dgvReporte.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReporteFilaDemo.Recaudacion), HeaderText = "Recaudación", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "C0" } });
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            //  reemplazar por datos reales agregados desde la API en la 2da entrega.
            if (dtpHasta.Value < dtpDesde.Value)
            {
                MessageBox.Show("La fecha 'Hasta' no puede ser anterior a 'Desde'.",
                    "Configuración", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var datosDemo = new List<ReporteFilaDemo>
            {
                new ReporteFilaDemo { Vuelo = "SP101", Fecha = "20/09/2026", AsientosVendidos = 142, Recaudacion = 26270000 },
                new ReporteFilaDemo { Vuelo = "SP205", Fecha = "21/09/2026", AsientosVendidos = 98,  Recaudacion = 21560000 },
                new ReporteFilaDemo { Vuelo = "SP310", Fecha = "21/09/2026", AsientosVendidos = 156, Recaudacion = 27300000 },
            };

            dgvReporte.DataSource = null;
            dgvReporte.DataSource = datosDemo;

            lblResultado.Text = $"Reporte '{cmbTipoReporte.SelectedItem}' generado — {datosDemo.Count} resultados " +
                                 $"({dtpDesde.Value:dd/MM/yyyy} a {dtpHasta.Value:dd/MM/yyyy}).";
        }
    }
}