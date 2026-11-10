using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Estilos;
using System.Drawing;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmReportes
    {
        private readonly Label _lblRankingRutas =
            CrearTitulo("Ranking de rutas más utilizadas");

        private readonly DataGridView _dgvRankingRutas =
            CrearGrid();

        private void InicializarRankingRutas()
        {
            _tabla.SuspendLayout();

            int fila = _tabla.RowCount;

            _tabla.RowCount += 2;

            _tabla.RowStyles.Add(
                new RowStyle(SizeType.AutoSize));

            _tabla.RowStyles.Add(
                new RowStyle(SizeType.AutoSize));

            _tabla.Controls.Add(
                _lblRankingRutas, 0, fila);

            _tabla.Controls.Add(
                _dgvRankingRutas, 0, fila + 1);

            AppEstilos.EstilizarGrid(_dgvRankingRutas);

            _lblRankingRutas.Visible = false;
            _dgvRankingRutas.Visible = false;

            _tabla.ResumeLayout(true);
        }

        private void MostrarRankingRutas(
            SkyPlus.Desktop.Models.ReporteVuelosResponse reporte)
        {
            Cargar(
                _dgvRankingRutas,
                reporte.RankingRutas,
                new[]
                {
                    new Col(
                        "Origen",
                        nameof(RutaRankingResponse.Origen)),

                    new Col(
                        "Destino",
                        nameof(RutaRankingResponse.Destino)),

                    new Col(
                        "Vuelos",
                        nameof(RutaRankingResponse.CantidadVuelos)),

                    new Col(
                        "Reservas confirmadas",
                        nameof(RutaRankingResponse.ReservasConfirmadas)),

                    new Col(
                        "Asientos ocupados",
                        nameof(RutaRankingResponse.AsientosOcupados)),

                    new Col(
                        "Ocupación (%)",
                        nameof(RutaRankingResponse.PorcentajeOcupacion),
                        "0.##")
                });

            _lblRankingRutas.Visible = true;
            _dgvRankingRutas.Visible = true;
        }

        private void OcultarRankingRutas()
        {
            _lblRankingRutas.Visible = false;
            _dgvRankingRutas.Visible = false;
            _dgvRankingRutas.DataSource = null;
        }
    }
}
