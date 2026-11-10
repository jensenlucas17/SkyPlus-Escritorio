
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Data;

namespace SkyPlus.Desktop
{
    public partial class frmReportes
    {
        private Button _btnPdf = null!;
        private Button _btnExcel = null!;
        private bool _reporteGenerado;
        private List<Indicador> _indicadoresExportacion = new();

        private void InicializarExportacion()
        {
            _btnPdf = new Button
            {
                Text = "Exportar PDF",
                Location = new Point(545, 46),
                Size = new System.Drawing.Size(110, 31),
                Enabled = false
            };

            _btnExcel = new Button
            {
                Text = "Exportar Excel",
                Location = new Point(665, 46),
                Size = new System.Drawing.Size(110, 31),
                Enabled = false
            };

            _btnPdf.Click += (_, _) => ExportarPdf();
            _btnExcel.Click += (_, _) => ExportarExcel();

            pnlFiltros.Controls.Add(_btnPdf);
            pnlFiltros.Controls.Add(_btnExcel);
        }

        private DataTable ObtenerTabla(DataGridView grid)
        {
            var tabla = new DataTable();

            foreach (DataGridViewColumn columna in grid.Columns)
            {
                if (columna.Visible)
                    tabla.Columns.Add(columna.HeaderText);
            }

            foreach (DataGridViewRow fila in grid.Rows)
            {
                if (fila.IsNewRow) continue;

                var valores = grid.Columns
                    .Cast<DataGridViewColumn>()
                    .Where(c => c.Visible)
                    .Select(c => fila.Cells[c.Index].FormattedValue?
                        .ToString() ?? "")
                    .Cast<object>()
                    .ToArray();

                tabla.Rows.Add(valores);
            }

            return tabla;
        }

        private void ExportarExcel()
        {
            if (!_reporteGenerado) return;

            using var dialogo = new SaveFileDialog
            {
                Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                FileName = $"SkyPlus_{cmbTipoReporte.Text}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var libro = new XLWorkbook();
                var hoja = libro.Worksheets.Add("Reporte");

                hoja.Cell(1, 1).Value = $"SkyPlus - {cmbTipoReporte.Text}";
                hoja.Cell(2, 1).Value =
                    $"Período: {dtpDesde.Value:dd/MM/yyyy} - {dtpHasta.Value:dd/MM/yyyy}";

                hoja.Range(1, 1, 1, 5).Merge();
                hoja.Cell(1, 1).Style.Font.Bold = true;
                hoja.Cell(1, 1).Style.Font.FontSize = 18;

                int filaActual = 4;

                hoja.Cell(filaActual++, 1).Value = "RESUMEN";

                foreach (var indicador in _indicadoresExportacion)
                {
                    hoja.Cell(filaActual, 1).Value = indicador.Nombre;
                    hoja.Cell(filaActual++, 2).Value = indicador.Valor;
                }

                filaActual += 2;

                AgregarTablaExcel(
                    hoja,
                    ObtenerTabla(_dgvDetalle1),
                    _lblDetalle1.Text,
                    ref filaActual);

                if (_dgvDetalle2.Visible)
                {
                    filaActual += 2;

                    AgregarTablaExcel(
                        hoja,
                        ObtenerTabla(_dgvDetalle2),
                        _lblDetalle2.Text,
                        ref filaActual);
                }

                hoja.Columns().AdjustToContents();
                libro.SaveAs(dialogo.FileName);

                MessageBox.Show("Excel exportado correctamente.",
                    "SkyPlus", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo exportar Excel:\n{ex.Message}");
            }
        }

        private static void AgregarTablaExcel(
            IXLWorksheet hoja,
            DataTable tabla,
            string titulo,
            ref int filaActual)
        {
            hoja.Cell(filaActual++, 1).Value = titulo;

            int encabezado = filaActual;

            for (int c = 0; c < tabla.Columns.Count; c++)
                hoja.Cell(filaActual, c + 1).Value =
                    tabla.Columns[c].ColumnName;

            filaActual++;

            foreach (DataRow registro in tabla.Rows)
            {
                for (int c = 0; c < tabla.Columns.Count; c++)
                    hoja.Cell(filaActual, c + 1).Value =
                        registro[c]?.ToString() ?? "";

                filaActual++;
            }

            if (tabla.Columns.Count > 0)
            {
                var rango = hoja.Range(
                    encabezado, 1,
                    encabezado, tabla.Columns.Count);

                rango.Style.Font.Bold = true;
                rango.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                rango.Style.Font.FontColor = XLColor.White;
            }
        }

        private void ExportarPdf()
        {
            if (!_reporteGenerado) return;

            using var dialogo = new SaveFileDialog
            {
                Filter = "Archivo PDF (*.pdf)|*.pdf",
                FileName = $"SkyPlus_{cmbTipoReporte.Text}_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                var resumen = _indicadoresExportacion.ToList();
                var tabla1 = ObtenerTabla(_dgvDetalle1);
                var tabla2 = _dgvDetalle2.Visible
                    ? ObtenerTabla(_dgvDetalle2)
                    : null;

                string titulo1 = _lblDetalle1.Text;
                string titulo2 = _lblDetalle2.Text;
                string tipo = cmbTipoReporte.Text;
                string periodo =
                    $"{dtpDesde.Value:dd/MM/yyyy} al {dtpHasta.Value:dd/MM/yyyy}";

                Document.Create(documento =>
                {
                    documento.Page(pagina =>
                    {
                        pagina.Size(PageSizes.A4.Landscape());
                        pagina.Margin(25);
                        pagina.DefaultTextStyle(x => x.FontSize(9));

                        pagina.Header().Column(c =>
                        {
                            c.Item().Text($"SkyPlus - {tipo}")
                                .Bold().FontSize(18);
                            c.Item().Text($"Período: {periodo}");
                        });

                        pagina.Content().PaddingTop(15).Column(c =>
                        {
                            c.Spacing(12);

                            c.Item().Text("Resumen").Bold().FontSize(13);

                            foreach (var indicador in resumen)
                                c.Item().Text(
                                    $"{indicador.Nombre}: {indicador.Valor}");

                            AgregarTablaPdf(c, titulo1, tabla1);

                            if (tabla2 != null)
                                AgregarTablaPdf(c, titulo2, tabla2);
                        });

                        pagina.Footer().AlignRight()
                            .Text(x =>
                            {
                                x.Span("Página ");
                                x.CurrentPageNumber();
                            });
                    });
                }).GeneratePdf(dialogo.FileName);

                MessageBox.Show("PDF exportado correctamente.",
                    "SkyPlus", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo exportar PDF:\n{ex.Message}");
            }
        }

        private static void AgregarTablaPdf(
            ColumnDescriptor columna,
            string titulo,
            DataTable datos)
        {
            columna.Item().Text(titulo).Bold().FontSize(13);

            if (datos.Columns.Count == 0) return;

            columna.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(def =>
                {
                    foreach (DataColumn _ in datos.Columns)
                        def.RelativeColumn();
                });

                tabla.Header(encabezado =>
                {
                    foreach (DataColumn campo in datos.Columns)
                    {
                        encabezado.Cell()
                            .Background(Colors.Blue.Darken3)
                            .Padding(4)
                            .Text(campo.ColumnName)
                            .FontColor(Colors.White)
                            .FontSize(8);
                    }
                });

                foreach (DataRow fila in datos.Rows)
                {
                    foreach (DataColumn campo in datos.Columns)
                    {
                        tabla.Cell()
                            .BorderBottom(0.5f)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(4)
                            .Text(fila[campo]?.ToString() ?? "")
                            .FontSize(8);
                    }
                }
            });
        }
    }
}
