using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmDashboard : Form
    {
        // Cada módulo: sección del menú, texto del botón y roles que lo pueden ver.
        // En el menú aparecen SOLO los módulos del rol que inició sesión.
        private readonly (string Seccion, string Texto, string[] Roles)[] _modulos =
        {
            ("",               "Inicio",        new[] { "Administrador", "Gerente", "Agente de Reservas", "Agente de Check-in" }),
            ("ADMINISTRACIÓN", "Usuarios",      new[] { "Administrador" }),
            ("OPERACIONES",    "Vuelos",        new[] { "Administrador", "Gerente" }),
            ("OPERACIONES",    "Aeronaves",     new[] { "Administrador", "Gerente" }),
            ("OPERACIONES",    "Lugares",       new[] { "Administrador", "Gerente" }),
            ("VENTAS",         "Pasajeros",     new[] { "Agente de Reservas" }),
            ("VENTAS",         "Reservas",      new[] { "Agente de Reservas" }),
            ("VENTAS",         "Ventas",        new[] { "Agente de Reservas" }),
            ("VENTAS",         "Cancelaciones", new[] { "Agente de Reservas" }),
            ("AEROPUERTO",     "Check-in",      new[] { "Agente de Check-in" }),
            ("AEROPUERTO",     "Boarding Pass", new[] { "Agente de Check-in" }),
            ("ANÁLISIS",       "Reportes",      new[] { "Administrador", "Gerente" }),
            ("SISTEMA",        "Backups",       new[] { "Administrador", "Gerente" }),
            ("SISTEMA",        "Configuración", new[] { "Administrador", "Gerente", "Agente de Reservas", "Agente de Check-in" }),
        };

        private static readonly Font FuenteMarca = new Font("Segoe UI", 15F, FontStyle.Bold);
        private static readonly Font FuenteSeccion = new Font("Segoe UI Semibold", 8F);

        private Button? _botonActivo;
        private Label _lblTituloModulo = new Label();
        private bool _cerrandoSesion;

        public frmDashboard()
        {
            InitializeComponent();
        }

        private void frmDashboard_Load(object sender, EventArgs e)
        {
            ConstruirLayout();
            CrearMenu();
            CargarModulo("Inicio");
        }

        // ---------------------------------------------------------------------
        // Estructura: menú lateral oscuro (izquierda), barra superior blanca
        // con el nombre del módulo, y el contenido debajo.
        // Se arma por código para no depender del diseñador.
        // ---------------------------------------------------------------------
        private void ConstruirLayout()
        {
            Text = "SkyPlus";
            BackColor = AppEstilos.ColorSecundario;
            Font = AppEstilos.FuenteNormal;
            MinimumSize = new Size(1100, 680);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            FormClosed += frmDashboard_FormClosed;

            // ----- Lateral -----
            var pnlLateral = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = AppEstilos.ColorLateral,
                Padding = new Padding(14, 0, 14, 14)
            };

            var pnlMarca = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = AppEstilos.ColorLateral };
            pnlMarca.Paint += DibujarMarca;

            var pnlUsuario = new Panel { Dock = DockStyle.Bottom, Height = 112, BackColor = AppEstilos.ColorLateral };
            pnlUsuario.Paint += DibujarUsuario;

            Controls.Remove(pnlMenu);
            pnlMenu.Dock = DockStyle.Fill;
            pnlMenu.BackColor = AppEstilos.ColorLateral;
            pnlMenu.Padding = new Padding(0, 6, 0, 6);

            // Datos del usuario (los labels del diseñador se reubican acá)
            pnlSuperior.Controls.Remove(lblUsuarioActual);
            pnlSuperior.Controls.Remove(lblRolActual);
            pnlSuperior.Controls.Remove(btnCerrarSesion);

            lblUsuarioActual.Text = $"{SesionUsuario.Nombre} {SesionUsuario.Apellido}";
            lblUsuarioActual.Font = AppEstilos.FuenteBoton;
            lblUsuarioActual.ForeColor = AppEstilos.ColorTextoClaro;
            lblUsuarioActual.AutoSize = false;
            lblUsuarioActual.AutoEllipsis = true;
            lblUsuarioActual.SetBounds(50, 14, 160, 20);

            lblRolActual.Text = SesionUsuario.Rol;
            lblRolActual.Font = AppEstilos.FuentePequena;
            lblRolActual.ForeColor = AppEstilos.ColorLateralTitulo;
            lblRolActual.AutoSize = false;
            lblRolActual.SetBounds(50, 34, 160, 18);

            btnCerrarSesion.Text = "Cerrar sesión";
            btnCerrarSesion.FlatStyle = FlatStyle.Flat;
            btnCerrarSesion.UseVisualStyleBackColor = false;
            btnCerrarSesion.BackColor = AppEstilos.ColorLateral;
            btnCerrarSesion.ForeColor = AppEstilos.ColorLateralTexto;
            btnCerrarSesion.Font = AppEstilos.FuenteNormal;
            btnCerrarSesion.FlatAppearance.BorderColor = AppEstilos.ColorLateralItem;
            btnCerrarSesion.FlatAppearance.MouseOverBackColor = AppEstilos.ColorLateralItem;
            btnCerrarSesion.TextAlign = ContentAlignment.MiddleLeft;
            btnCerrarSesion.Padding = new Padding(10, 0, 0, 0);
            btnCerrarSesion.Cursor = Cursors.Hand;
            btnCerrarSesion.SetBounds(0, 66, 212, 38);

            pnlUsuario.Controls.Add(lblUsuarioActual);
            pnlUsuario.Controls.Add(lblRolActual);
            pnlUsuario.Controls.Add(btnCerrarSesion);

            pnlLateral.Controls.Add(pnlMenu);
            pnlLateral.Controls.Add(pnlMarca);
            pnlLateral.Controls.Add(pnlUsuario);
            pnlMenu.BringToFront(); // el Fill va adelante para respetar marca (arriba) y usuario (abajo)

            // ----- Barra superior -----
            pnlSuperior.Height = 64;
            pnlSuperior.BackColor = Color.White;
            pnlSuperior.Padding = new Padding(28, 0, 28, 0);
            pnlSuperior.Paint += (s, e) =>
            {
                using var lapiz = new Pen(AppEstilos.ColorBorde);
                e.Graphics.DrawLine(lapiz, 0, pnlSuperior.Height - 1, pnlSuperior.Width, pnlSuperior.Height - 1);
            };

            _lblTituloModulo = new Label
            {
                Dock = DockStyle.Fill,
                Font = AppEstilos.FuenteTitulo,
                ForeColor = AppEstilos.ColorTextoOscuro,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var cultura = new CultureInfo("es-AR");
            string fecha = DateTime.Now.ToString("dddd d 'de' MMMM 'de' yyyy", cultura);
            var lblFecha = new Label
            {
                Dock = DockStyle.Right,
                Width = 320,
                Font = AppEstilos.FuenteNormal,
                ForeColor = AppEstilos.ColorTextoSuave,
                TextAlign = ContentAlignment.MiddleRight,
                Text = char.ToUpper(fecha[0], cultura) + fecha.Substring(1)
            };
            pnlSuperior.Controls.Add(_lblTituloModulo);
            pnlSuperior.Controls.Add(lblFecha);
            _lblTituloModulo.BringToFront();

            pnlContenido.BackColor = AppEstilos.ColorSecundario;

            // Orden de acoplamiento: primero el lateral (alto completo),
            // después la barra superior y por último el contenido.
            Controls.Add(pnlLateral);
            pnlSuperior.SendToBack();
            pnlLateral.SendToBack();
            pnlContenido.BringToFront();
        }

        // ---------------------------------------------------------------------
        // Menú lateral: solo los módulos del rol, agrupados por sección
        // ---------------------------------------------------------------------
        private void CrearMenu()
        {
            pnlMenu.Controls.Clear();
            string seccionActual = "";
            int ancho = 212;

            foreach (var modulo in _modulos)
            {
                if (!Array.Exists(modulo.Roles, r => r == SesionUsuario.Rol))
                    continue;

                if (modulo.Seccion != "" && modulo.Seccion != seccionActual)
                {
                    seccionActual = modulo.Seccion;
                    pnlMenu.Controls.Add(new Label
                    {
                        Text = seccionActual,
                        Font = FuenteSeccion,
                        ForeColor = AppEstilos.ColorLateralTitulo,
                        AutoSize = false,
                        Width = ancho,
                        Height = 30,
                        TextAlign = ContentAlignment.BottomLeft,
                        Padding = new Padding(12, 0, 0, 4),
                        Margin = new Padding(0, 8, 0, 0)
                    });
                }

                var boton = new Button
                {
                    Text = modulo.Texto,
                    Tag = modulo.Texto,
                    Width = ancho,
                    Height = 40,
                    Margin = new Padding(0, 1, 0, 1),
                    FlatStyle = FlatStyle.Flat,
                    UseVisualStyleBackColor = false,
                    BackColor = AppEstilos.ColorLateral,
                    ForeColor = AppEstilos.ColorLateralTexto,
                    Font = AppEstilos.FuenteNormal,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(12, 0, 0, 0),
                    Cursor = Cursors.Hand
                };
                boton.FlatAppearance.BorderSize = 0;
                boton.FlatAppearance.MouseOverBackColor = AppEstilos.ColorLateralItem;
                boton.FlatAppearance.MouseDownBackColor = AppEstilos.ColorLateralItem;
                boton.Paint += DibujarMarcaActiva;
                boton.Click += (s, e) => CargarModulo(modulo.Texto);

                pnlMenu.Controls.Add(boton);
            }
        }

        private void MarcarActivo(string nombreModulo)
        {
            foreach (Control c in pnlMenu.Controls)
            {
                if (c is not Button b) continue;
                bool activo = (string?)b.Tag == nombreModulo;
                b.BackColor = activo ? AppEstilos.ColorLateralItem : AppEstilos.ColorLateral;
                b.ForeColor = activo ? AppEstilos.ColorTextoClaro : AppEstilos.ColorLateralTexto;
                b.Font = activo ? AppEstilos.FuenteBoton : AppEstilos.FuenteNormal;
                if (activo) _botonActivo = b;
                b.Invalidate();
            }
        }

        private void CargarModulo(string nombreModulo)
        {
            // Liberar el módulo anterior (antes solo se quitaba y quedaba en memoria)
            foreach (Control c in pnlContenido.Controls)
                c.Dispose();
            pnlContenido.Controls.Clear();

            MarcarActivo(nombreModulo);
            _lblTituloModulo.Text = nombreModulo;

            if (nombreModulo == "Inicio")
            {
                pnlContenido.Controls.Add(new ucInicio());
                return;
            }

            Form? formulario = nombreModulo switch
            {
                "Usuarios" => new frmUsuarios(),
                "Lugares" => new frmLugares(),
                "Aeronaves" => new frmAeronaves(),
                "Vuelos" => new frmVuelos(),
                "Pasajeros" => new frmPasajeros(),
                "Reservas" => new frmReservas(),
                "Ventas" => new frmVentas(),
                "Reembolsos" => new frmReembolsos(),
                "Check-in" => new frmCheckin(),
                "Boarding Pass" => new frmBoardingPass(),
                "Reportes" => new frmReportes(),
                "Backups" => new frmBackups(
                    SesionUsuario.Rol == "Administrador"),
                "Configuración" => new frmConfiguracion(),
                "Cancelaciones" => new frmCancelaciones(),
                _ => null
            };

            if (formulario != null)
            {
                formulario.TopLevel = false;
                formulario.FormBorderStyle = FormBorderStyle.None;
                formulario.Dock = DockStyle.Fill;

                pnlContenido.Controls.Add(formulario);

                formulario.Show();
                AppEstilos.EstilizarFormulario(formulario);

                return;
            }

            var lbl = new Label
            {
                Text = $"Módulo: {nombreModulo}\n(pantalla en construcción)",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = AppEstilos.FuenteSubtitulo,
                ForeColor = AppEstilos.ColorTextoSuave
            };

            pnlContenido.Controls.Add(lbl);
        }

        // ---------------------------------------------------------------------
        // Dibujos del lateral
        // ---------------------------------------------------------------------
        private void DibujarMarca(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Logo: cuadrado azul redondeado con una "S"
            var logo = new Rectangle(4, 22, 34, 34);
            using (var camino = new GraphicsPath())
            {
                int d = 10;
                camino.AddArc(logo.Left, logo.Top, d, d, 180, 90);
                camino.AddArc(logo.Right - d, logo.Top, d, d, 270, 90);
                camino.AddArc(logo.Right - d, logo.Bottom - d, d, d, 0, 90);
                camino.AddArc(logo.Left, logo.Bottom - d, d, d, 90, 90);
                camino.CloseFigure();
                using var pincel = new SolidBrush(AppEstilos.ColorPrimario);
                g.FillPath(pincel, camino);
            }
            TextRenderer.DrawText(g, "S", FuenteMarca, logo, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            TextRenderer.DrawText(g, "SkyPlus", FuenteMarca,
                new Point(46, 14), Color.White);
            TextRenderer.DrawText(g, "Venta de pasajes", AppEstilos.FuentePequena,
                new Point(48, 46), AppEstilos.ColorLateralTitulo);
        }

        private void DibujarUsuario(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var lapiz = new Pen(AppEstilos.ColorLateralItem))
                g.DrawLine(lapiz, 0, 2, ((Control)sender!).Width, 2);

            // Avatar con iniciales
            var avatar = new Rectangle(4, 14, 36, 36);
            using (var pincel = new SolidBrush(AppEstilos.ColorPrimario))
                g.FillEllipse(pincel, avatar);

            string iniciales = $"{Inicial(SesionUsuario.Nombre)}{Inicial(SesionUsuario.Apellido)}";
            TextRenderer.DrawText(g, iniciales, AppEstilos.FuenteBoton, avatar, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        // Barrita celeste a la izquierda del módulo activo
        private void DibujarMarcaActiva(object? sender, PaintEventArgs e)
        {
            if (sender is Button b && b == _botonActivo)
            {
                using var pincel = new SolidBrush(AppEstilos.ColorAcentoClaro);
                e.Graphics.FillRectangle(pincel, 0, 8, 3, b.Height - 16);
            }
        }

        private static string Inicial(string texto) =>
            string.IsNullOrWhiteSpace(texto) ? "" : texto.Trim()[0].ToString().ToUpper();

        // ---------------------------------------------------------------------
        // Cierre
        // ---------------------------------------------------------------------
        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            var confirmacion = MessageBox.Show(
                "¿Estás seguro de que querés cerrar sesión?",
                "Cerrar sesión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
                return;

            SesionUsuario.CerrarSesion();
            _cerrandoSesion = true;

            // Se vuelve a mostrar el login que quedó oculto al ingresar
            var login = Application.OpenForms.OfType<frmLogin>().FirstOrDefault() ?? new frmLogin();
            login.Reiniciar();
            login.Show();

            this.Close();
        }

        // Si se cierra la ventana con la X, se termina la aplicación
        // (el login queda oculto y si no, el programa seguiría corriendo)
        private void frmDashboard_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (!_cerrandoSesion)
                Application.Exit();
        }
    }
}
