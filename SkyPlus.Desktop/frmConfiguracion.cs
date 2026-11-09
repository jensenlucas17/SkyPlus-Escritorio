// SkyPlus.Desktop/frmConfiguracion.cs
// Configuración:
//   - Mi cuenta (todos los roles): datos del usuario y cambio de contraseña (conectado a la API).
//   - Datos generales de la aerolínea (solo Administrador; todavía de demostración).
// La pantalla se arma por código; los controles del diseñador se reemplazan.

using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Utilidades;

namespace SkyPlus.Desktop
{
    public partial class frmConfiguracion : Form
    {
        private const int LargoMinimo = 12;

        private readonly CuentaClient _cuentaClient = new CuentaClient();

        private readonly TextBox _txtActual = new TextBox();
        private readonly TextBox _txtNueva = new TextBox();
        private readonly TextBox _txtRepetir = new TextBox();
        private readonly CheckBox _chkVer = new CheckBox();
        private readonly Button _btnCambiar = new Button();
        private readonly List<(Label Etiqueta, Func<bool> Cumple)> _requisitos = new();

        public frmConfiguracion()
        {
            InitializeComponent();
        }

        private void frmConfiguracion_Load(object sender, EventArgs e)
        {
            ConstruirPantalla();
            ActualizarRequisitos();
        }

        // =====================================================================
        // Cambio de contraseña
        // =====================================================================
        private void ActualizarRequisitos()
        {
            foreach (var (etiqueta, cumple) in _requisitos)
            {
                bool ok = cumple();
                etiqueta.ForeColor = ok ? AppEstilos.ColorExito : AppEstilos.ColorTextoSuave;
                etiqueta.Text = (ok ? "✓  " : "•  ") + etiqueta.Tag;
            }

            _btnCambiar.Enabled = _txtActual.Text.Length > 0 && _requisitos.All(r => r.Cumple());
        }

        private async void btnCambiar_Click(object? sender, EventArgs e)
        {
            try
            {
                _btnCambiar.Enabled = false;
                UseWaitCursor = true;

                string mensaje = await _cuentaClient.CambiarPasswordAsync(_txtActual.Text, _txtNueva.Text);

                UseWaitCursor = false;
                MessageBox.Show(mensaje + "\n\nLa próxima vez que inicies sesión usá la contraseña nueva.",
                    "Cambiar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _txtActual.Clear();
                _txtNueva.Clear();
                _txtRepetir.Clear();
            }
            catch (HttpRequestException ex)
            {
                UseWaitCursor = false;
                MensajesApi.MostrarError(ex);
            }
            finally
            {
                UseWaitCursor = false;
                ActualizarRequisitos();
            }
        }

        // =====================================================================
        // Armado de la pantalla
        // =====================================================================
        private void ConstruirPantalla()
        {
            Controls.Clear(); // se reemplazan los GroupBox del diseñador
            BackColor = AppEstilos.ColorSecundario;
            Font = AppEstilos.FuenteNormal;
            AutoScroll = true;
            Padding = new Padding(32, 24, 32, 24);

            var columna = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = AppEstilos.ColorSecundario
            };

            columna.Controls.Add(CrearTarjetaCuenta());
            columna.Controls.Add(CrearTarjetaSeguridad());
            if (SesionUsuario.Rol == "Administrador")
                columna.Controls.Add(CrearTarjetaGeneral());

            Controls.Add(columna);
        }

        private Panel CrearTarjeta(int alto)
        {
            var tarjeta = new Panel
            {
                Size = new Size(720, alto),
                BackColor = Color.White,
                Padding = new Padding(28, 22, 28, 22),
                Margin = new Padding(0, 0, 0, 18)
            };
            tarjeta.Paint += (s, e) =>
            {
                using var lapiz = new Pen(AppEstilos.ColorBorde);
                e.Graphics.DrawRectangle(lapiz, 0, 0, tarjeta.Width - 1, tarjeta.Height - 1);
            };
            return tarjeta;
        }

        private static Label Titulo(string texto, int y) => new Label
        {
            Text = texto,
            Font = AppEstilos.FuenteSubtitulo,
            ForeColor = AppEstilos.ColorTextoOscuro,
            AutoSize = true,
            Location = new Point(28, y)
        };

        private static Label Suave(string texto, int x, int y) => new Label
        {
            Text = texto,
            Font = AppEstilos.FuenteNormal,
            ForeColor = AppEstilos.ColorTextoSuave,
            AutoSize = true,
            Location = new Point(x, y)
        };

        // ----- Mi cuenta -----
        private Panel CrearTarjetaCuenta()
        {
            var tarjeta = CrearTarjeta(130);

            var avatar = new Panel { Size = new Size(64, 64), Location = new Point(28, 32), BackColor = Color.White };
            string iniciales = $"{Inicial(SesionUsuario.Nombre)}{Inicial(SesionUsuario.Apellido)}";
            avatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var pincel = new SolidBrush(AppEstilos.ColorPrimario);
                e.Graphics.FillEllipse(pincel, 0, 0, 63, 63);
                using var fuente = new Font("Segoe UI Semibold", 18F);
                TextRenderer.DrawText(e.Graphics, iniciales, fuente, new Rectangle(0, 0, 64, 64), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            var lblNombre = new Label
            {
                Text = $"{SesionUsuario.Nombre} {SesionUsuario.Apellido}",
                Font = AppEstilos.FuenteTitulo,
                ForeColor = AppEstilos.ColorTextoOscuro,
                AutoSize = true,
                Location = new Point(110, 32)
            };
            var lblDatos = Suave($"{SesionUsuario.Rol}  ·  {SesionUsuario.Email}", 112, 68);

            tarjeta.Controls.AddRange(new Control[] { avatar, lblNombre, lblDatos });
            return tarjeta;
        }

        // ----- Seguridad -----
        private Panel CrearTarjetaSeguridad()
        {
            var tarjeta = CrearTarjeta(420);

            tarjeta.Controls.Add(Titulo("Cambiar contraseña", 22));
            tarjeta.Controls.Add(Suave("Por seguridad, pedimos tu contraseña actual antes de guardar la nueva.", 30, 52));

            int y = 92;
            foreach (var (texto, caja) in new[]
                     {
                         ("Contraseña actual", _txtActual),
                         ("Contraseña nueva", _txtNueva),
                         ("Repetir contraseña nueva", _txtRepetir)
                     })
            {
                tarjeta.Controls.Add(new Label
                {
                    Text = texto,
                    Font = AppEstilos.FuenteBoton,
                    ForeColor = AppEstilos.ColorTextoOscuro,
                    AutoSize = true,
                    Location = new Point(30, y)
                });
                caja.Font = new Font("Segoe UI", 11F);
                caja.BorderStyle = BorderStyle.FixedSingle;
                caja.UseSystemPasswordChar = true;
                caja.SetBounds(30, y + 24, 340, 30);
                caja.TextChanged += (s, e) => ActualizarRequisitos();
                tarjeta.Controls.Add(caja);
                y += 70;
            }

            _chkVer.Text = "Mostrar contraseñas";
            _chkVer.AutoSize = true;
            _chkVer.ForeColor = AppEstilos.ColorTextoSuave;
            _chkVer.Location = new Point(30, y);
            _chkVer.CheckedChanged += (s, e) =>
            {
                foreach (var caja in new[] { _txtActual, _txtNueva, _txtRepetir })
                    caja.UseSystemPasswordChar = !_chkVer.Checked;
            };
            tarjeta.Controls.Add(_chkVer);

            // Requisitos a la derecha, se tildan a medida que se escribe
            var lblReq = new Label
            {
                Text = "La contraseña nueva debe tener:",
                Font = AppEstilos.FuenteBoton,
                ForeColor = AppEstilos.ColorTextoOscuro,
                AutoSize = true,
                Location = new Point(410, 92)
            };
            tarjeta.Controls.Add(lblReq);

            var reglas = new (string Texto, Func<bool> Cumple)[]
            {
                ($"Al menos {LargoMinimo} caracteres", () => _txtNueva.Text.Length >= LargoMinimo),
                ("Una letra mayúscula", () => _txtNueva.Text.Any(char.IsUpper)),
                ("Una letra minúscula", () => _txtNueva.Text.Any(char.IsLower)),
                ("Un número", () => _txtNueva.Text.Any(char.IsDigit)),
                ("Distinta de la actual", () => _txtNueva.Text.Length > 0 && _txtNueva.Text != _txtActual.Text),
                ("Las dos contraseñas nuevas coinciden", () => _txtNueva.Text.Length > 0 && _txtNueva.Text == _txtRepetir.Text)
            };

            int yReq = 122;
            foreach (var (texto, cumple) in reglas)
            {
                var etiqueta = new Label
                {
                    Tag = texto,
                    Font = AppEstilos.FuenteNormal,
                    AutoSize = true,
                    Location = new Point(410, yReq)
                };
                _requisitos.Add((etiqueta, cumple));
                tarjeta.Controls.Add(etiqueta);
                yReq += 28;
            }

            _btnCambiar.Text = "Cambiar contraseña";
            _btnCambiar.SetBounds(30, 340, 220, 42);
            AppEstilos.EstilizarBotonPrimario(_btnCambiar);
            _btnCambiar.Height = 42;
            _btnCambiar.Click += btnCambiar_Click;
            tarjeta.Controls.Add(_btnCambiar);

            return tarjeta;
        }

        // ----- Datos generales (solo Administrador) -----
        private Panel CrearTarjetaGeneral()
        {
            var tarjeta = CrearTarjeta(230);
            tarjeta.Controls.Add(Titulo("Datos generales de la aerolínea", 22));
            tarjeta.Controls.Add(Suave("Solo el Administrador ve esta sección.", 30, 52));

            tarjeta.Controls.Add(new Label { Text = "Nombre", Font = AppEstilos.FuenteBoton, AutoSize = true, Location = new Point(30, 92) });
            var txtNombre = new TextBox { Text = "SkyPlus Líneas Aéreas", Font = new Font("Segoe UI", 11F), BorderStyle = BorderStyle.FixedSingle };
            txtNombre.SetBounds(30, 116, 340, 30);

            tarjeta.Controls.Add(new Label { Text = "Moneda", Font = AppEstilos.FuenteBoton, AutoSize = true, Location = new Point(410, 92) });
            var cmbMoneda = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F) };
            cmbMoneda.Items.AddRange(new object[] { "ARS", "USD" });
            cmbMoneda.SelectedIndex = 0;
            cmbMoneda.SetBounds(410, 116, 140, 30);

            var btnGuardar = new Button { Text = "Guardar cambios" };
            btnGuardar.SetBounds(30, 168, 180, 40);
            AppEstilos.EstilizarBotonSecundario(btnGuardar);
            btnGuardar.Height = 40;
            btnGuardar.Click += (s, e) => MessageBox.Show(
                "Los cambios de configuración general estarán disponibles en una próxima versión.",
                "Configuración", MessageBoxButtons.OK, MessageBoxIcon.Information);

            tarjeta.Controls.AddRange(new Control[] { txtNombre, cmbMoneda, btnGuardar });
            return tarjeta;
        }

        private static string Inicial(string texto) =>
            string.IsNullOrWhiteSpace(texto) ? "" : texto.Trim()[0].ToString().ToUpper();

        // Los eventos del diseñador siguen existiendo pero sus controles ya no se muestran
        private void btnGuardarGeneral_Click(object sender, EventArgs e) { }
        private void btnCambiarPassword_Click(object sender, EventArgs e) { }
    }
}
