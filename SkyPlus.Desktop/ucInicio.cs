// SkyPlus.Desktop/ucInicio.cs
// Pantalla de inicio que se ve al entrar. Muestra datos distintos según el rol:
//   - Administrador / Gerente: indicadores y gráficos a partir de los Reportes de la API.
//   - Agente de Reservas: vuelos próximos, reservas y cobros del día.
//   - Agente de Check-in: vuelos de hoy y cuántos check-ins faltan en cada uno.
// Está armada solo con código (sin diseñador), usando las piezas de AppEstilos.

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;

namespace SkyPlus.Desktop
{
    [DesignerCategory("Code")]
    public class ucInicio : UserControl
    {
        private const int AsientosPorVuelo = 180; // la API genera siempre 30 filas x 6 letras

        private static readonly CultureInfo Cultura = new CultureInfo("es-AR");

        private readonly Panel _scroll;
        private readonly TableLayoutPanel _layout;
        private readonly Label _lblCargando;

        public ucInicio()
        {
            BackColor = AppEstilos.ColorSecundario;
            Dock = DockStyle.Fill;

            _scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(28, 22, 28, 22),
                BackColor = AppEstilos.ColorSecundario
            };

            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 0,
                Height = 0,
                BackColor = AppEstilos.ColorSecundario
            };
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _lblCargando = AppEstilos.CrearTextoSuave("Cargando datos...");

            _scroll.Controls.Add(_layout);
            Controls.Add(_scroll);
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AgregarFila(CrearEncabezado(), 76);
            AgregarFila(_lblCargando, 30);

            try
            {
                switch (SesionUsuario.Rol)
                {
                    case "Administrador":
                    case "Gerente":
                        await CargarGerenciaAsync();
                        break;
                    case "Agente de Reservas":
                        await CargarAgenteReservasAsync();
                        break;
                    case "Agente de Check-in":
                        await CargarAgenteCheckInAsync();
                        break;
                }
                QuitarCargando();
            }
            catch (HttpRequestException)
            {
                _lblCargando.Text = "No se pudieron cargar los datos de inicio. Verificá que la API esté en ejecución.";
                _lblCargando.ForeColor = AppEstilos.ColorError;
            }
            catch (Exception ex)
            {
                _lblCargando.Text = "No se pudieron cargar los datos de inicio: " + ex.Message;
                _lblCargando.ForeColor = AppEstilos.ColorError;
            }
        }

        // =====================================================================
        // Administrador / Gerente
        // =====================================================================
        private async Task CargarGerenciaAsync()
        {
            var reportes = new ReporteClient();
            DateTime hoy = DateTime.Today;
            DateTime inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            DateTime inicioSeisMeses = inicioMes.AddMonths(-5);

            var tVuelosHoy = reportes.ObtenerVuelosAsync(hoy, hoy);
            var tVuelosSemana = reportes.ObtenerVuelosAsync(hoy, hoy.AddDays(6));
            var tProximos = reportes.ObtenerVuelosAsync(hoy, hoy.AddDays(30));
            var tVentas = reportes.ObtenerVentasAsync(inicioSeisMeses, hoy);
            var tReservas = reportes.ObtenerReservasAsync(inicioSeisMeses, hoy);
            await Task.WhenAll(tVuelosHoy, tVuelosSemana, tProximos, tVentas, tReservas);

            var vuelosHoy = tVuelosHoy.Result ?? new ReporteVuelosResponse();
            var semana = tVuelosSemana.Result ?? new ReporteVuelosResponse();
            var proximos = tProximos.Result ?? new ReporteVuelosResponse();
            var ventas = tVentas.Result ?? new ReporteVentasResponse();
            var reservas = tReservas.Result ?? new ReporteReservasResponse();

            // ---- Indicadores ----
            var activosHoy = vuelosHoy.DetalleVuelos.Where(v => v.EstadoVuelo != "Cancelado").ToList();
            var ventasMes = ventas.VentasPorDia.Where(v => v.Fecha >= inicioMes).ToList();
            int reservasMes = reservas.ReservasPorDia.Where(r => r.Fecha >= inicioMes).Sum(r => r.CantidadReservas);

            AgregarFila(CrearFilaIndicadores(
                AppEstilos.CrearIndicador("Vuelos de hoy",
                    activosHoy.Count.ToString(),
                    $"{activosHoy.Sum(v => v.ReservasConfirmadas)} pasajeros confirmados"),
                AppEstilos.CrearIndicador("Ventas del mes",
                    FormatearMoneda(ventasMes.Sum(v => v.IngresoNeto)),
                    $"{ventasMes.Sum(v => v.CantidadPagos)} pagos registrados"),
                AppEstilos.CrearIndicador("Ocupación próximos 7 días",
                    $"{semana.PorcentajeOcupacionGeneral:0.#} %",
                    $"{semana.AsientosOcupados} de {semana.TotalAsientos} asientos",
                    AppEstilos.ColorPrimario),
                AppEstilos.CrearIndicador("Reservas del mes",
                    reservasMes.ToString(),
                    $"Desde el 1 de {inicioMes.ToString("MMMM", Cultura)}")
            ), 128);

            // ---- Gráficos ----
            var graficoIngresos = new GraficoBarras { FormatoValor = FormatearMonedaCorta };
            graficoIngresos.CargarDatos(Enumerable.Range(0, 6).Select(i =>
            {
                DateTime mes = inicioSeisMeses.AddMonths(i);
                decimal total = ventas.VentasPorDia
                    .Where(v => v.Fecha.Year == mes.Year && v.Fecha.Month == mes.Month)
                    .Sum(v => v.IngresoNeto);
                return (Capitalizar(mes.ToString("MMM", Cultura).TrimEnd('.')), total);
            }));

            var graficoEstados = new GraficoBarras { Horizontal = true };
            // Solo los estados reales de una venta: confirmada o cancelada
            graficoEstados.CargarDatos(reservas.ReservasPorEstado
                .Where(r => r.Estado == "Confirmada" || r.Estado == "Cancelada")
                .OrderByDescending(r => r.Cantidad)
                .Select(r => (NombreEstado(r.Estado), (decimal)r.Cantidad, ColorEstado(r.Estado))));

            AgregarFila(CrearFilaDosColumnas(
                CrearTarjetaConTitulo("Ingresos por mes", "Últimos 6 meses, descontados los reembolsos", graficoIngresos),
                CrearTarjetaConTitulo("Estado de las reservas", "Últimos 6 meses", graficoEstados),
                58), 300);

            // ---- Próximos vuelos ----
            var grid = CrearGrid(
                ("Vuelo", "Vuelo"), ("Ruta", "Ruta"), ("Salida", "Salida"),
                ("Ocupacion", "Ocupación"), ("Confirmadas", "Confirmadas"), ("Estado", "Estado"));

            foreach (var v in proximos.DetalleVuelos
                         .Where(v => v.Salida >= DateTime.Now)
                         .OrderBy(v => v.Salida)
                         .Take(8))
            {
                grid.Rows.Add(v.NumeroVuelo, $"{v.Origen} → {v.Destino}", FormatearSalida(v.Salida),
                    $"{v.AsientosOcupados} / {v.TotalAsientos}  ({v.PorcentajeOcupacion:0} %)",
                    v.ReservasConfirmadas, v.EstadoVuelo);
            }

            AgregarFila(CrearTarjetaConTitulo("Próximos vuelos", "Los próximos 30 días", grid), 360);
        }

        // =====================================================================
        // Agente de Reservas
        // =====================================================================
        private async Task CargarAgenteReservasAsync()
        {
            var tVuelos = new VueloClient().ObtenerTodosAsync();
            var tReservas = new ReservaClient().ObtenerTodosAsync();
            var tPagos = new PagoClient().ObtenerTodosAsync();
            await Task.WhenAll(tVuelos, tReservas, tPagos);

            var vuelos = tVuelos.Result ?? new List<VueloResponse>();
            var reservas = tReservas.Result ?? new List<ReservaResponse>();
            var pagos = tPagos.Result ?? new List<PagoResponse>();

            DateTime hoy = DateTime.Today;
            DateTime ahora = DateTime.Now;

            var vuelosEnVenta = vuelos
                .Where(v => v.Salida > ahora && (v.EstadoVuelo == "Programado" || v.EstadoVuelo == "Confirmado"))
                .OrderBy(v => v.Salida)
                .ToList();

            var reservasHoy = reservas.Where(r => FechaLocal(r.FechaReserva).Date == hoy).ToList();
            var pagosHoy = pagos.Where(p => p.EstadoPago == "Registrado" && FechaLocal(p.FechaPago).Date == hoy).ToList();
            // Pasajes vendidos para los vuelos de los próximos 7 días
            var vuelosSemana = vuelosEnVenta.Where(v => v.Salida <= ahora.AddDays(7)).Select(v => v.IdVuelo).ToHashSet();
            int vendidosSemana = reservas.Count(r => r.EstadoReserva == "Confirmada" && vuelosSemana.Contains(r.IdVuelo));

            AgregarFila(CrearFilaIndicadores(
                AppEstilos.CrearIndicador("Vuelos en venta (7 días)",
                    vuelosEnVenta.Count(v => v.Salida <= ahora.AddDays(7)).ToString(),
                    $"{vuelosEnVenta.Count} vuelos futuros en total"),
                AppEstilos.CrearIndicador("Ventas de hoy",
                    reservasHoy.Count(r => r.EstadoReserva == "Confirmada").ToString(),
                    $"{reservasHoy.Count(r => r.EstadoReserva == "Cancelada")} canceladas en el día"),
                AppEstilos.CrearIndicador("Pasajes vendidos (7 días)",
                    vendidosSemana.ToString(),
                    $"de {vuelosSemana.Count * AsientosPorVuelo} asientos en {vuelosSemana.Count} vuelos"),
                AppEstilos.CrearIndicador("Cobrado hoy",
                    FormatearMoneda(pagosHoy.Sum(p => p.Monto)),
                    $"{pagosHoy.Count} pagos registrados",
                    AppEstilos.ColorPrimario)
            ), 128);

            // Reservas de los últimos 7 días y cobros del mes por método de pago
            var graficoReservas = new GraficoBarras();
            graficoReservas.CargarDatos(Enumerable.Range(0, 7).Select(i =>
            {
                DateTime dia = hoy.AddDays(i - 6);
                int cantidad = reservas.Count(r => r.EstadoReserva != "Expirada" && FechaLocal(r.FechaReserva).Date == dia);
                return (dia == hoy ? "Hoy" : Capitalizar(dia.ToString("ddd d", Cultura).Replace(".", "")), (decimal)cantidad);
            }));

            DateTime inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var graficoMetodos = new GraficoBarras { Horizontal = true, FormatoValor = FormatearMonedaCorta };
            graficoMetodos.CargarDatos(pagos
                .Where(p => p.EstadoPago == "Registrado" && FechaLocal(p.FechaPago) >= inicioMes)
                .GroupBy(p => p.MetodoPago)
                .OrderByDescending(g => g.Sum(p => p.Monto))
                .Select(g => (g.Key, g.Sum(p => p.Monto))));

            AgregarFila(CrearFilaDosColumnas(
                CrearTarjetaConTitulo("Reservas por día", "Últimos 7 días", graficoReservas),
                CrearTarjetaConTitulo("Cobros del mes por método de pago", "Pagos registrados", graficoMetodos),
                58), 280);

            var grid = CrearGrid(
                ("Vuelo", "Vuelo"), ("Ruta", "Ruta"), ("Salida", "Salida"), ("Tarifa", "Tarifa"),
                ("Reservas", "Reservas"), ("Libres", "Asientos libres"), ("Estado", "Estado"));

            foreach (var v in vuelosEnVenta.Take(10))
            {
                int ocupadas = reservas.Count(r => r.IdVuelo == v.IdVuelo &&
                    (r.EstadoReserva == "Confirmada" || r.EstadoReserva == "PendientePago"));

                grid.Rows.Add(v.NumeroVuelo, $"{Iata(v.LugarOrigen)} → {Iata(v.LugarDestino)}",
                    FormatearSalida(v.Salida), FormatearMoneda(v.Tarifa), ocupadas,
                    AsientosPorVuelo - ocupadas, v.EstadoVuelo);
            }

            AgregarFila(CrearTarjetaConTitulo("Próximos vuelos en venta", "Ordenados por fecha de salida", grid), 400);
        }

        // =====================================================================
        // Agente de Check-in
        // =====================================================================
        private async Task CargarAgenteCheckInAsync()
        {
            var tVuelos = new VueloClient().ObtenerTodosAsync();
            var tReservas = new ReservaClient().ObtenerTodosAsync();
            var tCheckIns = new CheckInClient().ObtenerTodosAsync();
            await Task.WhenAll(tVuelos, tReservas, tCheckIns);

            var vuelos = tVuelos.Result ?? new List<VueloResponse>();
            var reservas = tReservas.Result ?? new List<ReservaResponse>();
            var checkIns = tCheckIns.Result ?? new List<CheckInResponse>();

            DateTime hoy = DateTime.Today;
            var checkInPorReserva = checkIns
                .GroupBy(c => c.IdReserva)
                .ToDictionary(g => g.Key, g => g.First());

            var resumen = vuelos
                .Where(v => v.EstadoVuelo != "Cancelado" &&
                            v.Salida.Date >= hoy && v.Salida.Date <= hoy.AddDays(1))
                .OrderBy(v => v.Salida)
                .Select(v =>
                {
                    var confirmadas = reservas
                        .Where(r => r.IdVuelo == v.IdVuelo && r.EstadoReserva == "Confirmada")
                        .ToList();
                    int conCheckIn = confirmadas.Count(r => checkInPorReserva.ContainsKey(r.IdReserva));
                    int embarcados = confirmadas.Count(r =>
                        checkInPorReserva.TryGetValue(r.IdReserva, out var c) && c.EstadoCheckin == "Embarcado");
                    return new
                    {
                        Vuelo = v,
                        Confirmadas = confirmadas.Count,
                        CheckIns = conCheckIn,
                        Embarcados = embarcados,
                        Faltan = confirmadas.Count - conCheckIn
                    };
                })
                .ToList();

            var deHoy = resumen.Where(r => r.Vuelo.Salida.Date == hoy).ToList();
            int faltanHoy = deHoy.Sum(r => r.Faltan);

            AgregarFila(CrearFilaIndicadores(
                AppEstilos.CrearIndicador("Vuelos de hoy",
                    deHoy.Count.ToString(),
                    deHoy.Count == 0 ? "No hay vuelos hoy" : "Próximo: " + deHoy[0].Vuelo.NumeroVuelo + " a las " + deHoy[0].Vuelo.Salida.ToString("HH:mm")),
                AppEstilos.CrearIndicador("Pasajeros confirmados",
                    deHoy.Sum(r => r.Confirmadas).ToString(),
                    "En los vuelos de hoy"),
                AppEstilos.CrearIndicador("Check-ins realizados",
                    deHoy.Sum(r => r.CheckIns).ToString(),
                    $"{deHoy.Sum(r => r.Embarcados)} pasajeros ya embarcaron",
                    AppEstilos.ColorExito),
                AppEstilos.CrearIndicador("Faltan check-in",
                    faltanHoy.ToString(),
                    "Pasajeros de hoy sin check-in",
                    faltanHoy > 0 ? AppEstilos.ColorAcento : null)
            ), 128);

            var graficoAvance = new GraficoBarras { Horizontal = true, FormatoValor = v => $"{v:0} %" };
            graficoAvance.MensajeVacio = "No hay vuelos con pasajeros confirmados para hoy.";
            graficoAvance.CargarDatos(deHoy
                .Where(r => r.Confirmadas > 0)
                .Select(r => (r.Vuelo.NumeroVuelo, (decimal)r.CheckIns * 100 / r.Confirmadas)));

            AgregarFila(CrearTarjetaConTitulo("Avance de check-in por vuelo", "Porcentaje de pasajeros confirmados que ya hicieron check-in hoy", graficoAvance),
                Math.Max(150, 70 + deHoy.Count * 40));

            var grid = CrearGrid(
                ("Vuelo", "Vuelo"), ("Ruta", "Ruta"), ("Salida", "Salida"), ("Confirmadas", "Confirmadas"),
                ("CheckIn", "Check-in"), ("Embarcados", "Embarcados"), ("Faltan", "Faltan"), ("Estado", "Estado"));

            foreach (var r in resumen)
            {
                grid.Rows.Add(r.Vuelo.NumeroVuelo, $"{Iata(r.Vuelo.LugarOrigen)} → {Iata(r.Vuelo.LugarDestino)}",
                    FormatearSalida(r.Vuelo.Salida), r.Confirmadas, r.CheckIns, r.Embarcados, r.Faltan, r.Vuelo.EstadoVuelo);
            }

            AgregarFila(CrearTarjetaConTitulo("Vuelos de hoy y mañana", "Ingresá a Check-in para atender a los pasajeros", grid), 320);
        }

        // =====================================================================
        // Armado de la pantalla
        // =====================================================================
        private void AgregarFila(Control control, int alto)
        {
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(0, 0, 0, 16);
            _layout.RowCount++;
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, alto + 16));
            _layout.Controls.Add(control, 0, _layout.RowCount - 1);
            _layout.Height += alto + 16;
            ActualizarScroll();
        }

        // Con controles acoplados, el panel necesita saber el alto total para mostrar la barra de desplazamiento
        private void ActualizarScroll() =>
            _scroll.AutoScrollMinSize = new Size(0, _layout.Height + _scroll.Padding.Vertical);

        private void QuitarCargando()
        {
            int fila = _layout.GetRow(_lblCargando);
            _layout.Controls.Remove(_lblCargando);
            if (fila >= 0 && fila < _layout.RowStyles.Count)
            {
                _layout.Height -= (int)_layout.RowStyles[fila].Height;
                _layout.RowStyles[fila].Height = 0;
                ActualizarScroll();
            }
        }

        private Control CrearEncabezado()
        {
            int hora = DateTime.Now.Hour;
            string saludo = hora < 12 ? "Buenos días" : hora < 20 ? "Buenas tardes" : "Buenas noches";

            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = AppEstilos.ColorSecundario
            };
            panel.Controls.Add(AppEstilos.CrearTitulo($"{saludo}, {SesionUsuario.Nombre}"));
            panel.Controls.Add(AppEstilos.CrearTextoSuave(
                Capitalizar(DateTime.Now.ToString("dddd d 'de' MMMM", Cultura)) + " · Resumen de la operación"));
            return panel;
        }

        private static TableLayoutPanel CrearFilaIndicadores(params Control[] indicadores)
        {
            var fila = new TableLayoutPanel
            {
                ColumnCount = indicadores.Length,
                RowCount = 1,
                BackColor = AppEstilos.ColorSecundario,
                Margin = Padding.Empty
            };
            fila.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            for (int i = 0; i < indicadores.Length; i++)
            {
                fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / indicadores.Length));
                indicadores[i].Dock = DockStyle.Fill;
                indicadores[i].Margin = new Padding(0, 0, i == indicadores.Length - 1 ? 0 : 14, 0);
                fila.Controls.Add(indicadores[i], i, 0);
            }
            return fila;
        }

        private static TableLayoutPanel CrearFilaDosColumnas(Control izquierda, Control derecha, float porcentajeIzquierda)
        {
            var fila = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppEstilos.ColorSecundario,
                Margin = Padding.Empty
            };
            fila.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, porcentajeIzquierda));
            fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 - porcentajeIzquierda));

            izquierda.Dock = DockStyle.Fill;
            izquierda.Margin = new Padding(0, 0, 14, 0);
            derecha.Dock = DockStyle.Fill;
            derecha.Margin = Padding.Empty;

            fila.Controls.Add(izquierda, 0, 0);
            fila.Controls.Add(derecha, 1, 0);
            return fila;
        }

        // Tarjeta blanca con título, subtítulo y un contenido que ocupa el resto
        private static Panel CrearTarjetaConTitulo(string titulo, string subtitulo, Control contenido)
        {
            var tarjeta = AppEstilos.CrearTarjeta();
            tarjeta.Padding = new Padding(18, 14, 18, 14);

            var lblTitulo = new Label
            {
                Text = titulo,
                Font = AppEstilos.FuenteSubtitulo,
                ForeColor = AppEstilos.ColorTextoOscuro,
                Dock = DockStyle.Top,
                Height = 24
            };
            var lblSubtitulo = new Label
            {
                Text = subtitulo,
                Font = AppEstilos.FuentePequena,
                ForeColor = AppEstilos.ColorTextoSuave,
                Dock = DockStyle.Top,
                Height = 26
            };

            contenido.Dock = DockStyle.Fill;
            contenido.BackColor = Color.White;

            tarjeta.Controls.Add(contenido);
            tarjeta.Controls.Add(lblSubtitulo);
            tarjeta.Controls.Add(lblTitulo);
            contenido.BringToFront(); // el Fill tiene que quedar adelante para respetar a los Top
            return tarjeta;
        }

        private static DataGridView CrearGrid(params (string Nombre, string Encabezado)[] columnas)
        {
            var grid = new DataGridView
            {
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false
            };
            AppEstilos.EstilizarGrid(grid);

            foreach (var (nombre, encabezado) in columnas)
                grid.Columns.Add(nombre, encabezado);

            // Colorea la columna Estado para que se lea de un vistazo
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Estado" || e.Value is not string estado)
                    return;
                e.CellStyle!.ForeColor = ColorEstado(estado);
                e.CellStyle.Font = AppEstilos.FuenteBoton;
            };
            grid.SelectionChanged += (s, e) => grid.ClearSelection(); // solo lectura: sin fila marcada
            return grid;
        }

        // =====================================================================
        // Formatos
        // =====================================================================
        private static DateTime FechaLocal(DateTime fecha) =>
            fecha.Kind == DateTimeKind.Utc ? fecha.ToLocalTime() : fecha;

        private static string Iata(string lugar) =>
            lugar.Split(" - ")[0].Trim();

        private static string FormatearSalida(DateTime salida)
        {
            if (salida.Date == DateTime.Today) return "Hoy " + salida.ToString("HH:mm");
            if (salida.Date == DateTime.Today.AddDays(1)) return "Mañana " + salida.ToString("HH:mm");
            return salida.ToString("dd/MM HH:mm");
        }

        private static string FormatearMoneda(decimal valor) =>
            valor.ToString("C0", Cultura);

        private static string FormatearMonedaCorta(decimal valor)
        {
            if (valor >= 1_000_000) return "$ " + (valor / 1_000_000).ToString("0.#", Cultura) + " M";
            if (valor >= 1_000) return "$ " + (valor / 1_000).ToString("0", Cultura) + " mil";
            return "$ " + valor.ToString("0", Cultura);
        }

        private static string Capitalizar(string texto) =>
            string.IsNullOrEmpty(texto) ? texto : char.ToUpper(texto[0], Cultura) + texto.Substring(1);

        private static string NombreEstado(string estado) => estado switch
        {
            "PendientePago" => "Pendiente de pago",
            _ => estado
        };

        private static Color ColorEstado(string estado) => estado switch
        {
            "Confirmada" or "Confirmado" => AppEstilos.ColorPrimario,
            "Programado" => AppEstilos.ColorTextoOscuro,
            "PendientePago" or "Cancelada" or "Cancelado" => AppEstilos.ColorAcento,
            "Finalizado" or "Expirada" => AppEstilos.ColorTextoSuave,
            _ => AppEstilos.ColorTextoOscuro
        };
    }
}
