using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Utilidades;

namespace SkyPlus.Desktop
{
    public partial class frmReportes : Form
    {
        private record Indicador(string Nombre, string Valor);
        private record Col(string Titulo, string Propiedad, string? Formato = null);

        private readonly ReporteClient _reporteClient = new ReporteClient();

        private readonly TableLayoutPanel _tabla = new TableLayoutPanel();
        private readonly Label _lblResumen = CrearTitulo("Resumen");
        private readonly DataGridView _dgvResumen = CrearGrid();
        private readonly Label _lblDetalle1 = CrearTitulo(string.Empty);
        private readonly DataGridView _dgvDetalle1 = CrearGrid();
        private readonly Label _lblDetalle2 = CrearTitulo(string.Empty);
        private readonly DataGridView _dgvDetalle2 = CrearGrid();

        public frmReportes()
        {
            InitializeComponent();
        }

        private void frmReportes_Load(object sender, EventArgs e)
        {
            cmbTipoReporte.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoReporte.Items.Clear();
            cmbTipoReporte.Items.AddRange(new object[]
            {
                "Ventas", "Reservas", "Vuelos", "Pasajeros", "Equipajes", "Check-ins"
            });
            cmbTipoReporte.SelectedIndex = 0;

            dtpDesde.Format = DateTimePickerFormat.Short;
            dtpHasta.Format = DateTimePickerFormat.Short;
            dtpDesde.Value = DateTime.Today.AddDays(-30);
            dtpHasta.Value = DateTime.Today;

            ArmarPantalla();

            AppEstilos.EstilizarBotonPrimario(btnGenerar);
            lblResultado.Text = "Elegí un informe y un rango de fechas, y presioná Generar.";
        }

        // ---------- Construcción de la pantalla ----------

        private static Label CrearTitulo(string texto) => new Label
        {
            Text = texto,
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(0, 8, 0, 2)
        };

        private static DataGridView CrearGrid() => new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };

        private void ArmarPantalla()
        {
            _tabla.Dock = DockStyle.Fill;
            _tabla.ColumnCount = 1;
            _tabla.RowCount = 6;
            _tabla.Padding = new Padding(8);

            _tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            _tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            _tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

            _tabla.Controls.Add(_lblResumen, 0, 0);
            _tabla.Controls.Add(_dgvResumen, 0, 1);
            _tabla.Controls.Add(_lblDetalle1, 0, 2);
            _tabla.Controls.Add(_dgvDetalle1, 0, 3);
            _tabla.Controls.Add(_lblDetalle2, 0, 4);
            _tabla.Controls.Add(_dgvDetalle2, 0, 5);

            foreach (var grid in new[] { _dgvResumen, _dgvDetalle1, _dgvDetalle2 })
                AppEstilos.EstilizarGrid(grid);

            pnlResultados.Controls.Add(_tabla);
        }

        private static void Cargar(DataGridView grid, object datos, IEnumerable<Col> columnas)
        {
            grid.DataSource = null;
            grid.Columns.Clear();

            foreach (var c in columnas)
            {
                var columna = new DataGridViewTextBoxColumn
                {
                    HeaderText = c.Titulo,
                    DataPropertyName = c.Propiedad,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    MinimumWidth = 70
                };

                if (c.Formato != null)
                    columna.DefaultCellStyle.Format = c.Formato;

                grid.Columns.Add(columna);
            }

            grid.DataSource = datos;
        }

        private void Presentar(
            List<Indicador> resumen,
            string titulo1, object detalle1, Col[] columnas1,
            string? titulo2 = null, object? detalle2 = null, Col[]? columnas2 = null)
        {
            Cargar(_dgvResumen, resumen, new[]
            {
                new Col("Indicador", nameof(Indicador.Nombre)),
                new Col("Valor", nameof(Indicador.Valor))
            });

            _lblDetalle1.Text = titulo1;
            Cargar(_dgvDetalle1, detalle1, columnas1);

            bool hayDetalle2 = titulo2 != null && detalle2 != null && columnas2 != null;

            _lblDetalle2.Visible = hayDetalle2;
            _dgvDetalle2.Visible = hayDetalle2;

            if (hayDetalle2)
            {
                _lblDetalle2.Text = titulo2!;
                Cargar(_dgvDetalle2, detalle2!, columnas2!);

                _tabla.RowStyles[3] = new RowStyle(SizeType.Percent, 40);
                _tabla.RowStyles[4] = new RowStyle(SizeType.AutoSize);
                _tabla.RowStyles[5] = new RowStyle(SizeType.Percent, 30);
            }
            else
            {
                _tabla.RowStyles[3] = new RowStyle(SizeType.Percent, 70);
                _tabla.RowStyles[4] = new RowStyle(SizeType.Absolute, 0);
                _tabla.RowStyles[5] = new RowStyle(SizeType.Absolute, 0);
            }
        }

        private static string Moneda(decimal valor) => valor.ToString("C0");
        private static string Porcentaje(decimal valor) => $"{valor:0.##} %";

        // ---------- Botones ----------

        private async void btnGenerar_Click(object sender, EventArgs e)
        {
            var desde = dtpDesde.Value.Date;
            var hasta = dtpHasta.Value.Date;

            if (hasta < desde)
            {
                MessageBox.Show("La fecha 'Hasta' no puede ser anterior a 'Desde'.",
                    "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnGenerar.Enabled = false;
                lblResultado.Text = "Generando informe...";

                switch (cmbTipoReporte.SelectedItem?.ToString())
                {
                    case "Ventas":
                        MostrarVentas(await _reporteClient.ObtenerVentasAsync(desde, hasta) ?? new ReporteVentasResponse());
                        break;
                    case "Reservas":
                        MostrarReservas(await _reporteClient.ObtenerReservasAsync(desde, hasta) ?? new ReporteReservasResponse());
                        break;
                    case "Vuelos":
                        MostrarVuelos(await _reporteClient.ObtenerVuelosAsync(desde, hasta) ?? new ReporteVuelosResponse());
                        break;
                    case "Pasajeros":
                        MostrarPasajeros(await _reporteClient.ObtenerPasajerosAsync(desde, hasta) ?? new ReportePasajerosResponse());
                        break;
                    case "Equipajes":
                        MostrarEquipajes(await _reporteClient.ObtenerEquipajesAsync(desde, hasta) ?? new ReporteEquipajesResponse());
                        break;
                    case "Check-ins":
                        MostrarCheckIns(await _reporteClient.ObtenerCheckInsAsync(desde, hasta) ?? new ReporteCheckInsResponse());
                        break;
                }

                lblResultado.Text =
                    $"Informe de {cmbTipoReporte.SelectedItem} del {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}.";
            }
            catch (HttpRequestException ex)
            {
                lblResultado.Text = string.Empty;
                MensajesApi.MostrarError(ex);
            }
            finally
            {
                btnGenerar.Enabled = true;
            }
        }

        // Si tenés un botón btnLimpiar, este método lo atiende; si no, queda sin usar.
        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            cmbTipoReporte.SelectedIndex = 0;
            dtpDesde.Value = DateTime.Today.AddDays(-30);
            dtpHasta.Value = DateTime.Today;

            _dgvResumen.DataSource = null;
            _dgvDetalle1.DataSource = null;
            _dgvDetalle2.DataSource = null;
            _lblDetalle1.Text = string.Empty;
            _lblDetalle2.Text = string.Empty;
            lblResultado.Text = "Elegí un informe y un rango de fechas, y presioná Generar.";
        }

        // ---------- Un método por informe ----------

        private void MostrarVentas(ReporteVentasResponse r)
        {
            Presentar(
                new List<Indicador>
                {
                    new("Cantidad de pagos", r.CantidadPagos.ToString()),
                    new("Pagos iniciales", r.PagosIniciales.ToString()),
                    new("Pagos adicionales", r.PagosAdicionales.ToString()),
                    new("Total cobrado", Moneda(r.TotalCobrado)),
                    new("Total reembolsado", Moneda(r.TotalReembolsado)),
                    new("Ingreso neto", Moneda(r.IngresoNeto))
                },
                "Ventas por día", r.VentasPorDia,
                new[]
                {
                    new Col("Fecha", nameof(VentaDiariaResponse.Fecha), "dd/MM/yyyy"),
                    new Col("Pagos", nameof(VentaDiariaResponse.CantidadPagos)),
                    new Col("Cobrado", nameof(VentaDiariaResponse.TotalCobrado), "C0"),
                    new Col("Reembolsado", nameof(VentaDiariaResponse.TotalReembolsado), "C0"),
                    new Col("Ingreso neto", nameof(VentaDiariaResponse.IngresoNeto), "C0")
                });
        }

        private void MostrarReservas(ReporteReservasResponse r)
        {
            Presentar(
                new List<Indicador>
                {
                    new("Total de reservas", r.TotalReservas.ToString()),
                    new("Confirmadas", r.Confirmadas.ToString()),
                    new("Pendientes de pago", r.PendientesPago.ToString()),
                    new("Canceladas", r.Canceladas.ToString()),
                    new("Expiradas", r.Expiradas.ToString())
                },
                "Reservas por estado", r.ReservasPorEstado,
                new[]
                {
                    new Col("Estado", nameof(ReservaPorEstadoResponse.Estado)),
                    new Col("Cantidad", nameof(ReservaPorEstadoResponse.Cantidad)),
                    new Col("Porcentaje (%)", nameof(ReservaPorEstadoResponse.Porcentaje), "0.##")
                },
                "Reservas por día", r.ReservasPorDia,
                new[]
                {
                    new Col("Fecha", nameof(ReservaDiariaResponse.Fecha), "dd/MM/yyyy"),
                    new Col("Reservas", nameof(ReservaDiariaResponse.CantidadReservas))
                });
        }

        private void MostrarVuelos(ReporteVuelosResponse r)
        {
            Presentar(
                new List<Indicador>
                {
                    new("Total de vuelos", r.TotalVuelos.ToString()),
                    new("Programados", r.Programados.ToString()),
                    new("Confirmados", r.Confirmados.ToString()),
                    new("Cancelados", r.Cancelados.ToString()),
                    new("Finalizados", r.Finalizados.ToString()),
                    new("Total de asientos", r.TotalAsientos.ToString()),
                    new("Asientos ocupados", r.AsientosOcupados.ToString()),
                    new("Asientos disponibles", r.AsientosDisponibles.ToString()),
                    new("Ocupación general", Porcentaje(r.PorcentajeOcupacionGeneral))
                },
                "Detalle de vuelos", r.DetalleVuelos,
                new[]
                {
                    new Col("N° vuelo", nameof(VueloDetalleReporteResponse.NumeroVuelo)),
                    new Col("Origen", nameof(VueloDetalleReporteResponse.Origen)),
                    new Col("Destino", nameof(VueloDetalleReporteResponse.Destino)),
                    new Col("Salida", nameof(VueloDetalleReporteResponse.Salida), "dd/MM/yyyy HH:mm"),
                    new Col("Llegada", nameof(VueloDetalleReporteResponse.Llegada), "dd/MM/yyyy HH:mm"),
                    new Col("Estado", nameof(VueloDetalleReporteResponse.EstadoVuelo)),
                    new Col("Asientos", nameof(VueloDetalleReporteResponse.TotalAsientos)),
                    new Col("Ocupados", nameof(VueloDetalleReporteResponse.AsientosOcupados)),
                    new Col("Disponibles", nameof(VueloDetalleReporteResponse.AsientosDisponibles)),
                    new Col("Reservas conf.", nameof(VueloDetalleReporteResponse.ReservasConfirmadas)),
                    new Col("Ocupación (%)", nameof(VueloDetalleReporteResponse.PorcentajeOcupacion), "0.##")
                },
                "Vuelos por estado", r.VuelosPorEstado,
                new[]
                {
                    new Col("Estado", nameof(VueloPorEstadoResponse.Estado)),
                    new Col("Cantidad", nameof(VueloPorEstadoResponse.Cantidad))
                });
        }

        private void MostrarPasajeros(ReportePasajerosResponse r)
        {
            Presentar(
                new List<Indicador>
                {
                    new("Pasajeros registrados", r.TotalPasajerosRegistrados.ToString()),
                    new("Activos", r.PasajerosActivos.ToString()),
                    new("Inactivos", r.PasajerosInactivos.ToString()),
                    new("Con reservas en el período", r.PasajerosConReservas.ToString()),
                    new("Con reservas confirmadas", r.PasajerosConReservasConfirmadas.ToString()),
                    new("Total de reservas del período", r.TotalReservasPeriodo.ToString())
                },
                "Pasajeros por vuelo", r.PasajerosPorVuelo,
                new[]
                {
                    new Col("N° vuelo", nameof(PasajeroPorVueloResponse.NumeroVuelo)),
                    new Col("Salida", nameof(PasajeroPorVueloResponse.Salida), "dd/MM/yyyy HH:mm"),
                    new Col("Reservas", nameof(PasajeroPorVueloResponse.TotalReservas)),
                    new Col("Confirmadas", nameof(PasajeroPorVueloResponse.ReservasConfirmadas)),
                    new Col("Pasajeros únicos", nameof(PasajeroPorVueloResponse.PasajerosUnicos))
                },
                "Pasajeros frecuentes", r.PasajerosFrecuentes,
                new[]
                {
                    new Col("Pasajero", nameof(PasajeroFrecuenteResponse.NombreCompleto)),
                    new Col("Documento", nameof(PasajeroFrecuenteResponse.Documento)),
                    new Col("Reservas", nameof(PasajeroFrecuenteResponse.TotalReservas)),
                    new Col("Confirmadas", nameof(PasajeroFrecuenteResponse.ReservasConfirmadas))
                });
        }

        private void MostrarEquipajes(ReporteEquipajesResponse r)
        {
            Presentar(
                new List<Indicador>
                {
                    new("Total de equipajes", r.TotalEquipajes.ToString()),
                    new("Activos", r.EquipajesActivos.ToString()),
                    new("Pendientes de pago", r.EquipajesPendientesPago.ToString()),
                    new("Cancelados", r.EquipajesCancelados.ToString()),
                    new("Pesados", r.EquipajesPesados.ToString()),
                    new("Con exceso de peso", r.EquipajesConExceso.ToString()),
                    new("Peso total (kg)", r.PesoTotalKg.ToString("0.##")),
                    new("Exceso total (kg)", r.ExcesoTotalKg.ToString("0.##")),
                    new("Importe contratado", Moneda(r.ImporteContratado)),
                    new("Cargos generados", r.CargosGenerados.ToString()),
                    new("Cargos pagados", r.CargosPagados.ToString()),
                    new("Cargos pendientes", r.CargosPendientes.ToString()),
                    new("Total cargos generados", Moneda(r.TotalCargosGenerados)),
                    new("Total cargos pagados", Moneda(r.TotalCargosPagados)),
                    new("Total cargos pendientes", Moneda(r.TotalCargosPendientes))
                },
                "Equipajes por tipo", r.EquipajesPorTipo,
                new[]
                {
                    new Col("Tipo de equipaje", nameof(EquipajePorTipoResponse.TipoEquipaje)),
                    new Col("Cantidad", nameof(EquipajePorTipoResponse.Cantidad)),
                    new Col("Peso total (kg)", nameof(EquipajePorTipoResponse.PesoTotalKg), "0.##"),
                    new Col("Importe contratado", nameof(EquipajePorTipoResponse.ImporteContratado), "C0")
                });
        }

        private void MostrarCheckIns(ReporteCheckInsResponse r)
        {
            Presentar(
                new List<Indicador>
                {
                    new("Total de check-ins", r.TotalCheckIns.ToString()),
                    new("Embarcados", r.Embarcados.ToString()),
                    new("Pendientes de embarque", r.PendientesEmbarque.ToString()),
                    new("Porcentaje de embarque", Porcentaje(r.PorcentajeEmbarque))
                },
                "Check-ins por vuelo", r.CheckInsPorVuelo,
                new[]
                {
                    new Col("N° vuelo", nameof(CheckInsPorVueloResponse.NumeroVuelo)),
                    new Col("Salida", nameof(CheckInsPorVueloResponse.Salida), "dd/MM/yyyy HH:mm"),
                    new Col("Reservas conf.", nameof(CheckInsPorVueloResponse.ReservasConfirmadas)),
                    new Col("Check-ins", nameof(CheckInsPorVueloResponse.TotalCheckIns)),
                    new Col("Embarcados", nameof(CheckInsPorVueloResponse.Embarcados)),
                    new Col("Pendientes", nameof(CheckInsPorVueloResponse.PendientesEmbarque)),
                    new Col("Embarque (%)", nameof(CheckInsPorVueloResponse.PorcentajeEmbarque), "0.##")
                },
                "Check-ins por día", r.CheckInsPorDia,
                new[]
                {
                    new Col("Fecha", nameof(CheckInsPorDiaResponse.Fecha), "dd/MM/yyyy"),
                    new Col("Check-ins", nameof(CheckInsPorDiaResponse.CantidadCheckIns))
                });
        }
    }
}