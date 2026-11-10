using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using System.Drawing;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmReportes
    {
        private sealed class OpcionFiltro
        {
            public int? Id { get; set; }
            public string Nombre { get; set; } = "";

            public override string ToString() => Nombre;
        }

        private readonly VueloClient _vueloClient = new();
        private readonly LugarClient _lugarClient = new();

        private readonly ComboBox _cmbAgrupacion = new();
        private readonly ComboBox _cmbVuelo = new();
        private readonly ComboBox _cmbOrigen = new();
        private readonly ComboBox _cmbDestino = new();

        private readonly ComboBox _cmbEstadoVuelo = new();
        private readonly ComboBox _cmbEstadoReserva = new();
        private readonly ComboBox _cmbMetodoPago = new();
        private readonly ComboBox _cmbTipoPago = new();

        private readonly FlowLayoutPanel _panelFiltrosAvanzados = new();

        private bool _cargandoCatalogos;

        private static Panel CrearCampo(
            string titulo,
            Control control,
            int ancho = 205)
        {
            var panel = new Panel
            {
                Width = ancho,
                Height = 66,
                Margin = new Padding(5, 4, 5, 4)
            };

            var etiqueta = new Label
            {
                Text = titulo,
                Location = new Point(0, 0),
                Size = new Size(ancho - 8, 21),
                Font = new Font("Segoe UI", 9F)
            };

            control.Location = new Point(0, 25);
            control.Size = new Size(ancho - 8, 28);
            control.Font = new Font("Segoe UI", 9F);

            if (control is ComboBox combo)
            {
                combo.DropDownStyle =
                    ComboBoxStyle.DropDownList;

                combo.FlatStyle = FlatStyle.Flat;
                combo.DropDownWidth = Math.Max(280, ancho);
            }

            panel.Controls.Add(etiqueta);
            panel.Controls.Add(control);

            return panel;
        }

        private static void PrepararCombo(
            ComboBox combo,
            params string[] opciones)
        {
            combo.DropDownStyle =
                ComboBoxStyle.DropDownList;

            combo.Items.Clear();
            combo.Items.AddRange(opciones);
            combo.SelectedIndex = 0;
        }

        private static void CargarOpciones(
            ComboBox combo,
            IEnumerable<OpcionFiltro> opciones)
        {
            combo.BeginUpdate();
            combo.Items.Clear();

            foreach (var opcion in opciones)
                combo.Items.Add(opcion);

            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;

            combo.EndUpdate();
        }

        private void InicializarFiltrosAvanzados()
        {
            PrepararCombo(
                _cmbAgrupacion,
                "Dia", "Semana", "Mes");

            PrepararCombo(
                _cmbEstadoVuelo,
                "Todos",
                "Programado",
                "Confirmado",
                "Cancelado",
                "Finalizado");

            PrepararCombo(
                _cmbEstadoReserva,
                "Todos",
                "Confirmada",
                "PendientePago",
                "Cancelada",
                "Expirada");

            PrepararCombo(
                _cmbMetodoPago,
                "Todos",
                "Efectivo",
                "Transferencia",
                "Tarjeta");

            PrepararCombo(
                _cmbTipoPago,
                "Todos",
                "Inicial",
                "Adicional");

            CargarOpciones(_cmbVuelo, new[]
            {
                new OpcionFiltro
                {
                    Nombre = "Todos los vuelos"
                }
            });

            CargarOpciones(_cmbOrigen, new[]
            {
                new OpcionFiltro
                {
                    Nombre = "Todos los orígenes"
                }
            });

            CargarOpciones(_cmbDestino, new[]
            {
                new OpcionFiltro
                {
                    Nombre = "Todos los destinos"
                }
            });

            _panelFiltrosAvanzados.Dock = DockStyle.Top;
            _panelFiltrosAvanzados.AutoSize = true;
            _panelFiltrosAvanzados.AutoSizeMode =
                AutoSizeMode.GrowAndShrink;

            _panelFiltrosAvanzados.WrapContents = true;
            _panelFiltrosAvanzados.Padding =
                new Padding(4, 8, 4, 12);

            _panelFiltrosAvanzados.Margin =
                new Padding(0, 0, 0, 12);

            _panelFiltrosAvanzados.Controls.Add(
                CrearCampo("Agrupación", _cmbAgrupacion, 155));

            _panelFiltrosAvanzados.Controls.Add(
                CrearCampo("Vuelo", _cmbVuelo, 235));

            _panelFiltrosAvanzados.Controls.Add(
                CrearCampo("Origen", _cmbOrigen, 235));

            _panelFiltrosAvanzados.Controls.Add(
                CrearCampo("Destino", _cmbDestino, 235));

            _panelFiltrosAvanzados.Controls.Add(
                CrearCampo("Estado del vuelo", _cmbEstadoVuelo));

            _panelFiltrosAvanzados.Controls.Add(
                CrearCampo("Estado de reserva", _cmbEstadoReserva));

            _panelFiltrosAvanzados.Controls.Add(
                CrearCampo("Método de pago", _cmbMetodoPago));

            _panelFiltrosAvanzados.Controls.Add(
                CrearCampo("Tipo de pago", _cmbTipoPago));

            _tabla.SuspendLayout();

            foreach (Control control in _tabla.Controls)
            {
                int fila = _tabla.GetRow(control);
                _tabla.SetRow(control, fila + 1);
            }

            _tabla.RowCount++;
            _tabla.RowStyles.Insert(
                0, new RowStyle(SizeType.AutoSize));

            _tabla.Controls.Add(
                _panelFiltrosAvanzados, 0, 0);

            _tabla.ResumeLayout(true);

            cmbTipoReporte.SelectedIndexChanged +=
                (_, _) => ActualizarFiltrosVisibles();

            _panelFiltrosAvanzados.SizeChanged +=
                (_, _) => AjustarAnchoFiltros();

            ActualizarFiltrosVisibles();

            // Ejecutar la carga después de mostrar
            // el formulario para no bloquear su apertura.
            Shown += async (_, _) =>
                await CargarCatalogosAsync();
        }

        private void AjustarAnchoFiltros()
        {
            int ancho = Math.Max(
                140, _panelFiltrosAvanzados.ClientSize.Width - 20);

            foreach (Control campo in
                _panelFiltrosAvanzados.Controls)
            {
                int anchoBase = campo.Controls
                    .OfType<ComboBox>()
                    .Any(c =>
                        c == _cmbVuelo ||
                        c == _cmbOrigen ||
                        c == _cmbDestino)
                    ? 235
                    : 205;

                if (campo.Controls.Contains(_cmbAgrupacion))
                    anchoBase = 155;

                int nuevoAncho = Math.Min(
                    anchoBase, ancho);

                campo.Width = nuevoAncho;

                foreach (Control hijo in campo.Controls)
                    hijo.Width = nuevoAncho - 8;
            }
        }

        private async Task CargarCatalogosAsync()
        {
            if (_cargandoCatalogos)
                return;

            _cargandoCatalogos = true;

            try
            {
                var vuelos = await
                    _vueloClient.ObtenerTodosAsync();

                var lugares = await
                    _lugarClient.ObtenerTodosAsync();

                var opcionesVuelos = new List<OpcionFiltro>
                {
                    new()
                    {
                        Nombre = "Todos los vuelos"
                    }
                };

                if (vuelos != null)
                {
                    opcionesVuelos.AddRange(
                        vuelos
                            .OrderBy(v => v.NumeroVuelo)
                            .Select(v => new OpcionFiltro
                            {
                                Id = v.IdVuelo,
                                Nombre =
                                    $"{v.NumeroVuelo} - " +
                                    $"{v.Salida:dd/MM/yyyy HH:mm}"
                            }));
                }

                CargarOpciones(_cmbVuelo, opcionesVuelos);

                var opcionesLugares = new List<OpcionFiltro>();

                if (lugares != null)
                {
                    opcionesLugares.AddRange(
                        lugares
                            .OrderBy(l => l.Ciudad)
                            .Select(l => new OpcionFiltro
                            {
                                Id = l.IdLugar,
                                Nombre =
                                    $"{l.CodigoIata} - {l.Ciudad}"
                            }));
                }

                var origenes = new List<OpcionFiltro>
                {
                    new()
                    {
                        Nombre = "Todos los orígenes"
                    }
                };

                origenes.AddRange(opcionesLugares);

                var destinos = new List<OpcionFiltro>
                {
                    new()
                    {
                        Nombre = "Todos los destinos"
                    }
                };

                destinos.AddRange(opcionesLugares);

                CargarOpciones(_cmbOrigen, origenes);
                CargarOpciones(_cmbDestino, destinos);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron cargar los vuelos " +
                    "y lugares disponibles.\n\n" +
                    ex.Message,
                    "Filtros de reportes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            finally
            {
                _cargandoCatalogos = false;
            }
        }

        private void ActualizarFiltrosVisibles()
        {
            string tipo = cmbTipoReporte.Text;

            bool ventas = tipo == "Ventas";

            _cmbMetodoPago.Parent!.Visible = ventas;
            _cmbTipoPago.Parent!.Visible = ventas;

            _cmbEstadoReserva.Parent!.Visible =
                tipo != "Vuelos";

            _cmbEstadoVuelo.Parent!.Visible = true;

            _cmbAgrupacion.Parent!.Visible =
                ventas ||
                tipo == "Reservas" ||
                tipo == "Check-ins";

            _panelFiltrosAvanzados.PerformLayout();
        }

        private static int? IdSeleccionado(ComboBox combo)
        {
            return (combo.SelectedItem as OpcionFiltro)?.Id;
        }

        private static string? TextoOpcional(ComboBox combo)
        {
            string valor = combo.Text;

            return valor == "Todos" ||
                   string.IsNullOrWhiteSpace(valor)
                ? null
                : valor;
        }

        private ReporteFiltrosRequest ObtenerFiltros()
        {
            string tipo = cmbTipoReporte.Text;

            return new ReporteFiltrosRequest
            {
                Desde = dtpDesde.Value.Date,
                Hasta = dtpHasta.Value.Date,

                IdVuelo = IdSeleccionado(_cmbVuelo),

                IdLugarOrigen =
                    IdSeleccionado(_cmbOrigen),

                IdLugarDestino =
                    IdSeleccionado(_cmbDestino),

                EstadoVuelo =
                    TextoOpcional(_cmbEstadoVuelo),

                EstadoReserva = tipo == "Vuelos"
                    ? null
                    : TextoOpcional(_cmbEstadoReserva),

                MetodoPago = tipo == "Ventas"
                    ? TextoOpcional(_cmbMetodoPago)
                    : null,

                TipoPago = tipo == "Ventas"
                    ? TextoOpcional(_cmbTipoPago)
                    : null,

                Agrupacion =
                    _cmbAgrupacion.Parent?.Visible == true
                        ? _cmbAgrupacion.Text
                        : "Dia",

                CompararPeriodoAnterior =
                    tipo == "Vuelos" &&
                    _chkCompararPeriodoAnterior.Checked
            };
        }

        private void LimpiarFiltrosAvanzados()
        {
            foreach (var combo in new[]
            {
                _cmbAgrupacion,
                _cmbVuelo,
                _cmbOrigen,
                _cmbDestino,
                _cmbEstadoVuelo,
                _cmbEstadoReserva,
                _cmbMetodoPago,
                _cmbTipoPago
            })
            {
                if (combo.Items.Count > 0)
                    combo.SelectedIndex = 0;
            }

            ActualizarFiltrosVisibles();
        }
    }
}
