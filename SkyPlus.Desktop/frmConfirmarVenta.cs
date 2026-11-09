// SkyPlus.Desktop/frmConfirmarVenta.cs
// Se abre desde "Nueva reserva" cuando ya se eligieron pasajero, vuelo y asiento.
//   Paso 1: equipaje (con los precios de cada tipo) o "continuar sin equipaje".
//   Paso 2: resumen con el total y el método de pago.
// Al confirmar el pago se hace todo junto en la API:
//   crear la reserva → contratar el equipaje → registrar el pago (queda Confirmada).
// Si algo falla después de crear la reserva, se cancela para liberar el asiento.

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Utilidades;

namespace SkyPlus.Desktop
{
    [DesignerCategory("Code")]
    public class frmConfirmarVenta : Form
    {
        private readonly int _idPasajero;
        private readonly string _pasajero;
        private readonly VueloResponse _vuelo;
        private readonly AsientoResponse _asiento;

        private readonly ReservaClient _reservaClient = new ReservaClient();
        private readonly EquipajeClient _equipajeClient = new EquipajeClient();
        private readonly PagoClient _pagoClient = new PagoClient();

        private List<TipoEquipajeResponse> _tipos = new();
        private readonly Dictionary<int, NumericUpDown> _cantidades = new();

        private readonly Label _lblPaso = new();
        private readonly Panel _pnlPaso1 = new();
        private readonly Panel _pnlPaso2 = new();
        private readonly TableLayoutPanel _tablaTipos = new();
        private readonly Label _lblSubtotalEquipaje = new();
        private readonly Label _lblNotaMano = new();
        private readonly Button _btnSinEquipaje = new();
        private readonly Button _btnConEquipaje = new();

        private readonly TableLayoutPanel _tablaTotal = new();
        private readonly ComboBox _cmbMetodo = new();
        private readonly Button _btnVolver = new();
        private readonly Button _btnPagar = new();

        // Código de la reserva creada (para que frmReservaABM lo muestre si quiere)
        public string CodigoReserva { get; private set; } = string.Empty;

        public frmConfirmarVenta(int idPasajero, string pasajero, VueloResponse vuelo, AsientoResponse asiento)
        {
            _idPasajero = idPasajero;
            _pasajero = pasajero;
            _vuelo = vuelo;
            _asiento = asiento;

            Text = "SkyPlus - Confirmar venta";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(640, 600);
            BackColor = AppEstilos.ColorSecundario;
            Font = AppEstilos.FuenteNormal;

            ConstruirPantalla();
            Load += async (s, e) => await CargarTiposAsync();
        }

        // =====================================================================
        // Paso 1: equipaje
        // =====================================================================
        private async Task CargarTiposAsync()
        {
            try
            {
                var tipos = await _equipajeClient.ObtenerTiposAsync() ?? new List<TipoEquipajeResponse>();

                // El equipaje de mano sin costo va incluido: no se contrata ni se pesa
                var incluidos = tipos.Where(t => t.CostoAdicional == 0).ToList();
                _tipos = tipos.Where(t => t.CostoAdicional > 0).OrderBy(t => t.CostoAdicional).ToList();

                _lblNotaMano.Text = incluidos.Count > 0
                    ? "Incluido en el pasaje: " + string.Join(", ", incluidos.Select(t => $"{t.Nombre.ToLower()} (hasta {t.LimiteKg:0.##} kg)")) + "."
                    : "";

                LlenarTablaTipos();
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
                _lblNotaMano.Text = "No se pudieron cargar los tipos de equipaje. Podés continuar sin equipaje.";
            }
            ActualizarSubtotal();
        }

        private void LlenarTablaTipos()
        {
            _tablaTipos.SuspendLayout();
            _tablaTipos.Controls.Clear();
            _tablaTipos.RowStyles.Clear();
            _cantidades.Clear();
            _tablaTipos.RowCount = _tipos.Count + 1;

            AgregarCelda("Tipo", 0, 0, encabezado: true);
            AgregarCelda("Hasta", 1, 0, encabezado: true);
            AgregarCelda("Precio", 2, 0, encabezado: true);
            AgregarCelda("Cantidad", 3, 0, encabezado: true);
            _tablaTipos.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            for (int i = 0; i < _tipos.Count; i++)
            {
                var t = _tipos[i];
                int fila = i + 1;
                _tablaTipos.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

                AgregarCelda(t.Nombre, 0, fila);
                AgregarCelda($"{t.LimiteKg:0.##} kg", 1, fila);
                AgregarCelda(t.CostoAdicional.ToString("C0"), 2, fila);

                var cantidad = new NumericUpDown
                {
                    Minimum = 0,
                    Maximum = 3,
                    Width = 64,
                    Font = AppEstilos.FuenteNormal,
                    BorderStyle = BorderStyle.FixedSingle,
                    Anchor = AnchorStyles.Left,
                    TextAlign = HorizontalAlignment.Center
                };
                cantidad.ValueChanged += (s, e) => ActualizarSubtotal();
                _cantidades[t.IdTipoEquipaje] = cantidad;
                _tablaTipos.Controls.Add(cantidad, 3, fila);
            }

            _tablaTipos.ResumeLayout();
        }

        private void AgregarCelda(string texto, int columna, int fila, bool encabezado = false)
        {
            _tablaTipos.Controls.Add(new Label
            {
                Text = texto,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = encabezado ? AppEstilos.FuenteBoton : AppEstilos.FuenteNormal,
                ForeColor = encabezado ? AppEstilos.ColorTextoSuave : AppEstilos.ColorTextoOscuro
            }, columna, fila);
        }

        private List<TipoEquipajeResponse> EquipajeElegido() =>
            _tipos.SelectMany(t => Enumerable.Repeat(t, (int)_cantidades[t.IdTipoEquipaje].Value)).ToList();

        private void ActualizarSubtotal()
        {
            var elegido = EquipajeElegido();
            decimal subtotal = elegido.Sum(t => t.CostoAdicional);
            _lblSubtotalEquipaje.Text = elegido.Count == 0
                ? "Sin equipaje despachado"
                : $"{elegido.Count} valija(s) · {subtotal:C0}";
            _btnConEquipaje.Enabled = elegido.Count > 0;
        }

        // =====================================================================
        // Paso 2: pago
        // =====================================================================
        private void IrAlPago(bool conEquipaje)
        {
            if (!conEquipaje)
                foreach (var n in _cantidades.Values) n.Value = 0;

            var elegido = EquipajeElegido();

            _tablaTotal.SuspendLayout();
            _tablaTotal.Controls.Clear();
            _tablaTotal.RowStyles.Clear();
            _tablaTotal.RowCount = 0;

            AgregarLineaTotal($"Pasaje {_vuelo.NumeroVuelo} · asiento {_asiento.Fila}{_asiento.Letra}", _vuelo.Tarifa);
            foreach (var grupo in elegido.GroupBy(t => t.IdTipoEquipaje))
            {
                var tipo = grupo.First();
                AgregarLineaTotal($"{grupo.Count()} × {tipo.Nombre}", grupo.Sum(t => t.CostoAdicional));
            }
            if (elegido.Count == 0)
                AgregarLineaTotal("Sin equipaje despachado", 0);

            decimal total = _vuelo.Tarifa + elegido.Sum(t => t.CostoAdicional);
            AgregarLineaTotal("TOTAL A PAGAR", total, destacado: true);
            _tablaTotal.ResumeLayout();

            _btnPagar.Text = $"Confirmar pago {total:C0}";
            _lblPaso.Text = "Paso 2 de 2 · Pago";
            _pnlPaso1.Visible = false;
            _pnlPaso2.Visible = true;
            AcceptButton = _btnPagar;
        }

        private void AgregarLineaTotal(string concepto, decimal monto, bool destacado = false)
        {
            int fila = _tablaTotal.RowCount++;
            _tablaTotal.RowStyles.Add(new RowStyle(SizeType.Absolute, destacado ? 46 : 30));
            var fuente = destacado ? AppEstilos.FuenteSubtitulo : AppEstilos.FuenteNormal;
            var color = destacado ? AppEstilos.ColorTextoOscuro : AppEstilos.ColorTextoSuave;

            _tablaTotal.Controls.Add(new Label { Text = concepto, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = fuente, ForeColor = color }, 0, fila);
            _tablaTotal.Controls.Add(new Label { Text = monto.ToString("C0"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = destacado ? AppEstilos.FuenteTitulo : fuente, ForeColor = destacado ? AppEstilos.ColorPrimario : AppEstilos.ColorTextoOscuro }, 1, fila);
        }

        private void VolverAlEquipaje()
        {
            _lblPaso.Text = "Paso 1 de 2 · Equipaje";
            _pnlPaso2.Visible = false;
            _pnlPaso1.Visible = true;
            AcceptButton = null;
        }

        private async Task ConfirmarPagoAsync()
        {
            string metodo = _cmbMetodo.SelectedItem as string ?? "Efectivo";
            var elegido = EquipajeElegido();
            ReservaResponse? reserva = null;

            _btnPagar.Enabled = false;
            _btnVolver.Enabled = false;
            UseWaitCursor = true;

            try
            {
                // 1) Reserva (queda pendiente de pago unos minutos)
                reserva = await _reservaClient.CrearAsync(_idPasajero, _vuelo.IdVuelo, _asiento.IdAsiento, SesionUsuario.IdUsuario);
                if (reserva == null)
                    throw new HttpRequestException("La API no devolvió la reserva creada.");

                // 2) Equipaje: cada valija queda como cargo pendiente
                foreach (var tipo in elegido)
                    await _equipajeClient.ContratarAsync(reserva.IdReserva, tipo.IdTipoEquipaje);

                // 3) Pago inicial: la API cobra pasaje + equipaje y confirma la reserva
                var pago = await _pagoClient.CrearAsync(reserva.IdReserva, SesionUsuario.IdUsuario, metodo);

                CodigoReserva = reserva.CodigoReserva;
                UseWaitCursor = false;

                MessageBox.Show(
                    $"Venta confirmada.\n\nReserva: {reserva.CodigoReserva}\nPasajero: {_pasajero}\n" +
                    $"Vuelo: {_vuelo.NumeroVuelo} · {_vuelo.Salida:dd/MM/yyyy HH:mm} · asiento {_asiento.Fila}{_asiento.Letra}\n" +
                    $"Total cobrado: {(pago?.Monto ?? 0):C0} ({metodo})",
                    "SkyPlus", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (HttpRequestException ex)
            {
                UseWaitCursor = false;

                // Si la reserva llegó a crearse, se cancela para no dejar el asiento tomado
                if (reserva != null)
                {
                    try { await _reservaClient.CancelarAsync(reserva.IdReserva); }
                    catch (HttpRequestException) { /* vence sola a los 5 minutos */ }
                }

                MessageBox.Show(
                    "No se pudo completar la venta: " + ex.Message +
                    (reserva != null ? "\n\nLa reserva se canceló y el asiento quedó libre." : ""),
                    "SkyPlus", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                _btnPagar.Enabled = true;
                _btnVolver.Enabled = true;
            }
        }

        // =====================================================================
        // Armado de la pantalla
        // =====================================================================
        private void ConstruirPantalla()
        {
            // ----- Encabezado -----
            var encabezado = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = AppEstilos.ColorLateral, Padding = new Padding(24, 10, 24, 10) };
            var lblTitulo = new Label { Text = "Confirmar venta", Dock = DockStyle.Left, AutoSize = false, Width = 300, Font = AppEstilos.FuenteTitulo, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft };
            _lblPaso.Text = "Paso 1 de 2 · Equipaje";
            _lblPaso.Dock = DockStyle.Fill;
            _lblPaso.TextAlign = ContentAlignment.MiddleRight;
            _lblPaso.Font = AppEstilos.FuenteNormal;
            _lblPaso.ForeColor = AppEstilos.ColorLateralTexto;
            encabezado.Controls.Add(_lblPaso);
            encabezado.Controls.Add(lblTitulo);

            // ----- Resumen de la reserva -----
            var resumen = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 118,
                ColumnCount = 2,
                RowCount = 4,
                BackColor = Color.White,
                Padding = new Padding(24, 12, 24, 8)
            };
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            string ruta = $"{_vuelo.LugarOrigen} → {_vuelo.LugarDestino}";
            AgregarDato(resumen, 0, "Pasajero", _pasajero);
            AgregarDato(resumen, 1, "Vuelo", $"{_vuelo.NumeroVuelo} · {ruta}");
            AgregarDato(resumen, 2, "Salida", $"{_vuelo.Salida:dddd dd/MM/yyyy HH:mm}");
            AgregarDato(resumen, 3, "Asiento", $"{_asiento.Fila}{_asiento.Letra} · Tarifa {_vuelo.Tarifa:C0}");

            // ----- Paso 1 -----
            _pnlPaso1.Dock = DockStyle.Fill;
            _pnlPaso1.BackColor = AppEstilos.ColorSecundario;
            _pnlPaso1.Padding = new Padding(24, 16, 24, 16);

            var lblPregunta = new Label { Text = "¿El pasajero despacha equipaje?", Dock = DockStyle.Top, Height = 30, Font = AppEstilos.FuenteSubtitulo, ForeColor = AppEstilos.ColorTextoOscuro };
            _lblNotaMano.Dock = DockStyle.Top;
            _lblNotaMano.Height = 24;
            _lblNotaMano.Font = AppEstilos.FuentePequena;
            _lblNotaMano.ForeColor = AppEstilos.ColorTextoSuave;

            _tablaTipos.Dock = DockStyle.Fill;
            _tablaTipos.ColumnCount = 4;
            _tablaTipos.BackColor = Color.White;
            _tablaTipos.Padding = new Padding(14, 6, 14, 6);
            _tablaTipos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            _tablaTipos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            _tablaTipos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            _tablaTipos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));

            _lblSubtotalEquipaje.Dock = DockStyle.Fill;
            _lblSubtotalEquipaje.TextAlign = ContentAlignment.MiddleLeft;
            _lblSubtotalEquipaje.Font = AppEstilos.FuenteBoton;
            _lblSubtotalEquipaje.ForeColor = AppEstilos.ColorTextoOscuro;

            _btnSinEquipaje.Text = "Continuar sin equipaje";
            _btnSinEquipaje.Width = 190;
            _btnSinEquipaje.Dock = DockStyle.Right;
            AppEstilos.EstilizarBotonSecundario(_btnSinEquipaje);
            _btnSinEquipaje.Click += (s, e) => IrAlPago(conEquipaje: false);

            _btnConEquipaje.Text = "Continuar con equipaje";
            _btnConEquipaje.Width = 200;
            _btnConEquipaje.Dock = DockStyle.Right;
            AppEstilos.EstilizarBotonPrimario(_btnConEquipaje);
            _btnConEquipaje.Click += (s, e) => IrAlPago(conEquipaje: true);

            var barra1 = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = AppEstilos.ColorSecundario, Padding = new Padding(0, 12, 0, 0) };
            barra1.Controls.Add(_lblSubtotalEquipaje);
            barra1.Controls.Add(_btnSinEquipaje);
            barra1.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 10, BackColor = AppEstilos.ColorSecundario });
            barra1.Controls.Add(_btnConEquipaje);
            _lblSubtotalEquipaje.BringToFront();

            _pnlPaso1.Controls.Add(_tablaTipos);
            _pnlPaso1.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 6, BackColor = AppEstilos.ColorSecundario });
            _pnlPaso1.Controls.Add(_lblNotaMano);
            _pnlPaso1.Controls.Add(lblPregunta);
            _pnlPaso1.Controls.Add(barra1);
            _tablaTipos.BringToFront();

            // ----- Paso 2 -----
            _pnlPaso2.Dock = DockStyle.Fill;
            _pnlPaso2.BackColor = AppEstilos.ColorSecundario;
            _pnlPaso2.Padding = new Padding(24, 16, 24, 16);
            _pnlPaso2.Visible = false;

            var lblDetalle = new Label { Text = "Detalle del cobro", Dock = DockStyle.Top, Height = 30, Font = AppEstilos.FuenteSubtitulo, ForeColor = AppEstilos.ColorTextoOscuro };

            _tablaTotal.Dock = DockStyle.Top;
            _tablaTotal.Height = 190;
            _tablaTotal.ColumnCount = 2;
            _tablaTotal.BackColor = Color.White;
            _tablaTotal.Padding = new Padding(14, 6, 14, 6);
            _tablaTotal.AutoScroll = true;
            _tablaTotal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            _tablaTotal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            var filaMetodo = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, BackColor = AppEstilos.ColorSecundario, Padding = new Padding(0, 10, 0, 0) };
            var lblMetodo = new Label { Text = "Método de pago:", AutoSize = true, Font = AppEstilos.FuenteBoton, ForeColor = AppEstilos.ColorTextoOscuro, Margin = new Padding(0, 6, 10, 0) };
            _cmbMetodo.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbMetodo.FlatStyle = FlatStyle.Flat;
            _cmbMetodo.Width = 180;
            _cmbMetodo.Font = AppEstilos.FuenteNormal;
            _cmbMetodo.Items.AddRange(new object[] { "Efectivo", "Transferencia", "Tarjeta" });
            _cmbMetodo.SelectedIndex = 0;
            filaMetodo.Controls.Add(lblMetodo);
            filaMetodo.Controls.Add(_cmbMetodo);

            _btnVolver.Text = "Volver";
            _btnVolver.Width = 110;
            _btnVolver.Dock = DockStyle.Left;
            AppEstilos.EstilizarBotonSecundario(_btnVolver);
            _btnVolver.Click += (s, e) => VolverAlEquipaje();

            _btnPagar.Text = "Confirmar pago";
            _btnPagar.Width = 260;
            _btnPagar.Dock = DockStyle.Right;
            AppEstilos.EstilizarBotonPrimario(_btnPagar);
            _btnPagar.Click += async (s, e) => await ConfirmarPagoAsync();

            var barra2 = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = AppEstilos.ColorSecundario, Padding = new Padding(0, 12, 0, 0) };
            barra2.Controls.Add(_btnVolver);
            barra2.Controls.Add(_btnPagar);

            _pnlPaso2.Controls.Add(barra2);
            _pnlPaso2.Controls.Add(filaMetodo);
            _pnlPaso2.Controls.Add(_tablaTotal);
            _pnlPaso2.Controls.Add(lblDetalle);

            // ----- Armado general -----
            Controls.Add(_pnlPaso2);
            Controls.Add(_pnlPaso1);
            Controls.Add(resumen);
            Controls.Add(encabezado);
            _pnlPaso1.BringToFront();
            _pnlPaso2.BringToFront();

            ActualizarSubtotal();
        }

        private static void AgregarDato(TableLayoutPanel tabla, int fila, string titulo, string valor)
        {
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            tabla.Controls.Add(new Label { Text = titulo, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = AppEstilos.FuentePequena, ForeColor = AppEstilos.ColorTextoSuave }, 0, fila);
            tabla.Controls.Add(new Label { Text = valor, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = AppEstilos.FuenteBoton, ForeColor = AppEstilos.ColorTextoOscuro, AutoEllipsis = true }, 1, fila);
        }
    }
}
