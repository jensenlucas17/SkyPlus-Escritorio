using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop
{
    public class ucMapaAsientos : UserControl
    {
        private const int Filas = 30;
        private static readonly string[] Letras = { "A", "B", "C", "D", "E", "F" };

        private static readonly Color ColorLibre = Color.FromArgb(76, 175, 80);
        private static readonly Color ColorOcupado = Color.FromArgb(211, 47, 47);
        private static readonly Color ColorSeleccionado = Color.FromArgb(25, 118, 210);
        private static readonly Color ColorSinDatos = Color.Gainsboro;

        private readonly Dictionary<string, Button> _botones = new();
        private readonly Dictionary<string, AsientoResponse> _asientos = new();
        private Button? _botonSeleccionado;

        public event EventHandler<AsientoResponse>? AsientoSeleccionado;

        public AsientoResponse? AsientoActual =>
            _botonSeleccionado?.Tag is string clave && _asientos.TryGetValue(clave, out var asiento)
                ? asiento
                : null;

        public ucMapaAsientos()
        {
            BackColor = Color.White;
            ConstruirMapa();
        }

        private void ConstruirMapa()
        {
            var leyenda = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(10, 8, 0, 0)
            };
            leyenda.Controls.Add(CrearItemLeyenda("Libre", ColorLibre));
            leyenda.Controls.Add(CrearItemLeyenda("Ocupado", ColorOcupado));
            leyenda.Controls.Add(CrearItemLeyenda("Seleccionado", ColorSeleccionado));

            var contenedor = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            var tabla = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 8,
                RowCount = Filas + 1,
                Location = new Point(10, 10)
            };

            tabla.SuspendLayout();

            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
            for (int i = 0; i < 3; i++) tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
            for (int i = 0; i < 3; i++) tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));

            for (int i = 0; i <= Filas; i++)
                tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            for (int i = 0; i < Letras.Length; i++)
                tabla.Controls.Add(CrearEtiqueta(Letras[i]), ColumnaDeLetra(i), 0);

            for (int fila = 1; fila <= Filas; fila++)
            {
                tabla.Controls.Add(CrearEtiqueta(fila.ToString()), 0, fila);

                for (int i = 0; i < Letras.Length; i++)
                {
                    var clave = $"{fila}{Letras[i]}";
                    var boton = CrearBoton(Letras[i], clave);
                    _botones[clave] = boton;
                    tabla.Controls.Add(boton, ColumnaDeLetra(i), fila);
                }
            }

            tabla.ResumeLayout(true);
            contenedor.Controls.Add(tabla);

            Controls.Add(contenedor);
            Controls.Add(leyenda);
        }

        // A, B, C usan las columnas 1 a 3; la 4 es el pasillo; D, E, F van de la 5 a la 7
        private static int ColumnaDeLetra(int indice) => indice < 3 ? indice + 1 : indice + 2;

        private static Label CrearEtiqueta(string texto) => new Label
        {
            Text = texto,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 8, FontStyle.Bold)
        };

        private Button CrearBoton(string letra, string clave)
        {
            var boton = new Button
            {
                Text = letra,
                Tag = clave,
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorSinDatos,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8)
            };
            boton.FlatAppearance.BorderSize = 0;
            boton.Click += Boton_Click;
            return boton;
        }

        private static Control CrearItemLeyenda(string texto, Color color)
        {
            var panel = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 0, 14, 0) };
            panel.Controls.Add(new Panel { Width = 14, Height = 14, BackColor = color, Margin = new Padding(0, 3, 4, 0) });
            panel.Controls.Add(new Label { Text = texto, AutoSize = true, Margin = new Padding(0, 2, 0, 0) });
            return panel;
        }

        // Carga o recarga los estados de los asientos de un vuelo
        public void Cargar(IEnumerable<AsientoResponse> asientos)
        {
            _asientos.Clear();
            _botonSeleccionado = null;

            foreach (var boton in _botones.Values)
            {
                boton.BackColor = ColorSinDatos;
                boton.Cursor = Cursors.Default;
            }

            foreach (var asiento in asientos)
            {
                var clave = $"{asiento.Fila}{asiento.Letra}";
                if (!_botones.TryGetValue(clave, out var boton)) continue;

                _asientos[clave] = asiento;
                PintarSegunEstado(boton, asiento);
            }
        }

        private static void PintarSegunEstado(Button boton, AsientoResponse asiento)
        {
            bool libre = asiento.Estado == "Libre";
            boton.BackColor = libre ? ColorLibre : ColorOcupado;
            boton.Cursor = libre ? Cursors.Hand : Cursors.Default;
        }

        private void Boton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button boton || boton.Tag is not string clave) return;
            if (!_asientos.TryGetValue(clave, out var asiento) || asiento.Estado != "Libre") return;

            if (_botonSeleccionado?.Tag is string claveAnterior &&
                _asientos.TryGetValue(claveAnterior, out var anterior))
            {
                PintarSegunEstado(_botonSeleccionado, anterior);
            }

            _botonSeleccionado = boton;
            boton.BackColor = ColorSeleccionado;
            AsientoSeleccionado?.Invoke(this, asiento);
        }

        // Feedback inmediato al confirmar una venta; el estado real lo confirma la API al recargar
        public void MarcarOcupado(int idAsiento)
        {
            var asiento = _asientos.Values.FirstOrDefault(a => a.IdAsiento == idAsiento);
            if (asiento == null) return;

            asiento.Estado = "Ocupado";
            var clave = $"{asiento.Fila}{asiento.Letra}";

            if (_botonSeleccionado?.Tag as string == clave)
                _botonSeleccionado = null;

            PintarSegunEstado(_botones[clave], asiento);
        }
    }
}