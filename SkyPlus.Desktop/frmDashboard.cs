using System;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop
{
    public partial class frmDashboard : Form
    {
        // TODO: confirmar con Lucas los nombres exactos de rol que devuelve la API (RolResponse.NombreRol)
        private readonly (string Texto, string[] Roles)[] _modulos = new (string, string[])[]
        {
            ("Usuarios",       new[] { "Administrador", "Gerente" }),
            ("Vuelos",         new[] { "Administrador", "Gerente" }),
            ("Lugares",        new[] { "Administrador", "Gerente" }),
            ("Pasajeros",      new[] { "Agente de Reservas" }),
            ("Reservas",       new[] { "Agente de Reservas" }),
            ("Ventas",         new[] { "Agente de Reservas" }),
            ("Cancelaciones",  new[] { "Agente de Reservas" }),
            ("Reembolsos",     new[] { "Agente de Reservas" }),
            ("Check-in",       new[] { "Agente de Check-in" }),
            ("Boarding Pass",  new[] { "Agente de Check-in" }),
            ("Reportes",       new[] { "Administrador", "Gerente" }),
            ("Configuración",  new[] { "Administrador" }),
        };

        public frmDashboard()
        {
            InitializeComponent();
        }

        private void frmDashboard_Load(object sender, EventArgs e)
        {
            lblUsuarioActual.Text = $"{SesionUsuario.Nombre} {SesionUsuario.Apellido}";
            lblRolActual.Text = $"Rol: {SesionUsuario.Rol}";

            CrearMenu();
        }

        private void CrearMenu()
        {
            pnlMenu.Controls.Clear();

            foreach (var modulo in _modulos)
            {
                var boton = new Button
                {
                    Text = modulo.Texto,
                    Width = pnlMenu.Width - 25,
                    Height = 40,
                    Margin = new Padding(5),
                    Enabled = Array.Exists(modulo.Roles, r => r == SesionUsuario.Rol)
                };

                boton.Click += (s, e) => CargarModulo(modulo.Texto);
                pnlMenu.Controls.Add(boton);
            }
        }

        private void CargarModulo(string nombreModulo)
        {
            pnlContenido.Controls.Clear();

            if (nombreModulo == "Usuarios")
            {
                var frmUsuarios = new frmUsuarios { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                pnlContenido.Controls.Add(frmUsuarios);
                frmUsuarios.Show();
                return;
            }
            var lbl = new Label
            {
                Text = $"Módulo: {nombreModulo}\n(pantalla en construcción)",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 14)
            };

            pnlContenido.Controls.Add(lbl);
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            SesionUsuario.IdUsuario = 0;
            SesionUsuario.Nombre = string.Empty;
            SesionUsuario.Apellido = string.Empty;
            SesionUsuario.Email = string.Empty;
            SesionUsuario.IdRol = 0;
            SesionUsuario.Rol = string.Empty;

            var login = new frmLogin();
            login.Show();
            this.Close();
        }
    }
}