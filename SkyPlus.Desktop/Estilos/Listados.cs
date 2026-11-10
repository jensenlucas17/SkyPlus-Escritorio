// SkyPlus.Desktop/Estilos/Listados.cs
// Criterio ÚNICO para todas las pantallas con lista (igual que Reservas):
//   - Arriba, una tarjeta de filtros: "Buscar" (filtra mientras se escribe) + filtros con su título
//     + botón Refrescar (limpia la búsqueda y los filtros, y vuelve a traer los datos de la API).
//   - No hay botón "Buscar".
//   - Abajo de la lista, un resumen con totales de lo que se está viendo.

using System.Drawing;
using System.Windows.Forms;

namespace SkyPlus.Desktop.Estilos
{
    public static class Listados
    {
        public const string Todos = "Todos";

        // Reemplaza el contenido del panel de filtros del diseñador por la barra estándar.
        public static void ArmarFiltros(
            Panel panel,
            TextBox txtBuscar,
            Button btnRefrescar,
            string placeholder,
            params (string Titulo, Control Control)[] filtros)
            => ArmarFiltros(panel, txtBuscar, btnRefrescar, "Buscar", placeholder, filtros);

        public static void ArmarFiltros(
            Panel panel,
            TextBox txtBuscar,
            Button btnRefrescar,
            string tituloBuscar,
            string placeholder,
            params (string Titulo, Control Control)[] filtros)
        {
            panel.Controls.Clear();
            panel.Dock = DockStyle.Top;
            panel.Height = 92;
            panel.BackColor = Color.White;
            panel.Padding = new Padding(20, 12, 20, 12);
            panel.Paint -= DibujarLineaInferior;
            panel.Paint += DibujarLineaInferior;

            txtBuscar.Width = 300;
            txtBuscar.Font = AppEstilos.FuenteNormal;
            txtBuscar.BorderStyle = BorderStyle.FixedSingle;
            txtBuscar.PlaceholderText = placeholder;
            txtBuscar.Margin = Padding.Empty;
            txtBuscar.Visible = true;

            var fila = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.White
            };
            fila.Controls.Add(Campo(tituloBuscar, txtBuscar));
            foreach (var (titulo, control) in filtros)
                fila.Controls.Add(Campo(titulo, control));

            btnRefrescar.Text = "Refrescar";
            btnRefrescar.Visible = true;
            btnRefrescar.Dock = DockStyle.Fill;
            AppEstilos.EstilizarBotonSecundario(btnRefrescar);

            var derecha = new Panel { Dock = DockStyle.Right, Width = 120, BackColor = Color.White, Padding = new Padding(10, 24, 0, 4) };
            derecha.Controls.Add(btnRefrescar);

            panel.Controls.Add(fila);
            panel.Controls.Add(derecha);
            fila.BringToFront();
        }

        // Búsqueda en vivo + Refrescar.
        //   filtrar:  vuelve a aplicar búsqueda y filtros sobre los datos en memoria.
        //   recargar: vuelve a pedir los datos a la API.
        //   limpiar:  (opcional) deja los filtros en "Todos".
        public static void ConectarBusqueda(
            TextBox txtBuscar,
            Button btnRefrescar,
            Action filtrar,
            Func<Task> recargar,
            Action? limpiar = null)
        {
            txtBuscar.TextChanged += (s, e) => filtrar();
            btnRefrescar.Click += async (s, e) =>
            {
                btnRefrescar.Enabled = false;
                txtBuscar.Clear();
                limpiar?.Invoke();
                await recargar();
                btnRefrescar.Enabled = true;
            };
        }

        public static Control Campo(string titulo, Control control)
        {
            var campo = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 14, 0)
            };
            campo.Controls.Add(new Label
            {
                Text = titulo,
                AutoSize = true,
                Font = AppEstilos.FuenteBoton,
                ForeColor = AppEstilos.ColorTextoSuave,
                Margin = new Padding(0, 0, 0, 4)
            });
            control.Margin = Padding.Empty;
            campo.Controls.Add(control);
            return campo;
        }

        // Combo de filtro con "Todos" como primera opción
        public static ComboBox Combo(int ancho, IEnumerable<string> opciones, Action alCambiar)
        {
            var combo = new ComboBox
            {
                Width = ancho,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Font = AppEstilos.FuenteNormal
            };
            CargarOpciones(combo, opciones);
            combo.SelectedIndexChanged += (s, e) => alCambiar();
            return combo;
        }

        // Recarga las opciones de un combo manteniendo lo elegido si sigue existiendo
        public static void CargarOpciones(ComboBox combo, IEnumerable<string> opciones)
        {
            string? anterior = combo.SelectedItem as string;
            combo.BeginUpdate();
            combo.Items.Clear();
            combo.Items.Add(Todos);
            foreach (var o in opciones.Where(o => !string.IsNullOrWhiteSpace(o)).Distinct())
                combo.Items.Add(o);
            combo.EndUpdate();
            int i = anterior == null ? 0 : combo.Items.IndexOf(anterior);
            combo.SelectedIndex = i < 0 ? 0 : i;
        }

        public static string Valor(ComboBox combo) => combo.SelectedItem as string ?? Todos;

        public static bool Pasa(ComboBox combo, string? valor) =>
            Valor(combo) == Todos || string.Equals(Valor(combo), valor, StringComparison.OrdinalIgnoreCase);

        // Fecha opcional: con la casilla destildada no filtra
        public static DateTimePicker Fecha(Action alCambiar)
        {
            var fecha = new DateTimePicker
            {
                Width = 140,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                ShowCheckBox = true,
                Checked = false,
                Font = AppEstilos.FuenteNormal
            };
            fecha.ValueChanged += (s, e) => alCambiar();
            return fecha;
        }

        public static bool PasaDesde(DateTimePicker desde, DateTime fecha) =>
            !desde.Checked || fecha.Date >= desde.Value.Date;

        public static bool PasaHasta(DateTimePicker hasta, DateTime fecha) =>
            !hasta.Checked || fecha.Date <= hasta.Value.Date;

        // Barra de resumen debajo de la lista (arriba de la barra de botones, si hay)
        public static Label CrearResumen(Form form, Control grilla)
        {
            var resumen = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Color.White,
                ForeColor = AppEstilos.ColorTextoOscuro,
                Font = AppEstilos.FuenteBoton,
                Padding = new Padding(20, 0, 20, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            resumen.Paint += (s, e) =>
            {
                using var lapiz = new Pen(AppEstilos.ColorBorde);
                e.Graphics.DrawLine(lapiz, 0, 0, resumen.Width, 0);
            };

            form.Controls.Add(resumen);
            // Orden de acoplamiento: la lista (Fill) primero en el índice 0 y el resumen justo después,
            // así queda pegado debajo de la lista y arriba de los botones.
            grilla.BringToFront();
            form.Controls.SetChildIndex(resumen, 1);
            return resumen;
        }

        private static void DibujarLineaInferior(object? sender, PaintEventArgs e)
        {
            if (sender is not Control c) return;
            using var lapiz = new Pen(AppEstilos.ColorBorde);
            e.Graphics.DrawLine(lapiz, 0, c.Height - 1, c.Width, c.Height - 1);
        }
    }
}
