// SkyPlus.Desktop/Estilos/AppEstilos.cs
// Paleta, tipografías y métodos de estilo de toda la aplicación.
// Cambiar un valor acá cambia el aspecto de TODOS los formularios.

using System.Drawing;
using System.Windows.Forms;

namespace SkyPlus.Desktop.Estilos
{
    public static class AppEstilos
    {
        // ---------- Paleta ----------
        public static readonly Color ColorPrimario = ColorTranslator.FromHtml("#2563EB");      // azul principal (botones, acentos)
        public static readonly Color ColorPrimarioHover = ColorTranslator.FromHtml("#1D4ED8");
        public static readonly Color ColorPrimarioSuave = ColorTranslator.FromHtml("#DBEAFE"); // selección de filas, chips
        public static readonly Color ColorLateral = ColorTranslator.FromHtml("#0F1B33");       // menú lateral
        public static readonly Color ColorLateralItem = ColorTranslator.FromHtml("#1C2C4D");   // ítem activo / hover del menú
        public static readonly Color ColorLateralTexto = ColorTranslator.FromHtml("#C9D3E6");
        public static readonly Color ColorLateralTitulo = ColorTranslator.FromHtml("#8B9AB8");
        public static readonly Color ColorAcentoClaro = ColorTranslator.FromHtml("#60A5FA");   // marca del ítem activo

        public static readonly Color ColorSecundario = ColorTranslator.FromHtml("#F3F5F9");    // fondo general
        public static readonly Color ColorTarjeta = Color.White;
        public static readonly Color ColorBorde = ColorTranslator.FromHtml("#E2E7F0");
        public static readonly Color ColorFilaAlterna = ColorTranslator.FromHtml("#F8FAFC");

        public static readonly Color ColorTextoClaro = Color.White;
        public static readonly Color ColorTextoOscuro = ColorTranslator.FromHtml("#172033");
        public static readonly Color ColorTextoSuave = ColorTranslator.FromHtml("#5B6780");

        public static readonly Color ColorAcento = ColorTranslator.FromHtml("#D97706");        // naranja: pendientes / alertas
        public static readonly Color ColorExito = ColorTranslator.FromHtml("#15803D");
        public static readonly Color ColorError = ColorTranslator.FromHtml("#B91C1C");

        // ---------- Tipografía ----------
        public static readonly Font FuenteTitulo = new Font("Segoe UI Semibold", 16F);
        public static readonly Font FuenteSubtitulo = new Font("Segoe UI Semibold", 11F);
        public static readonly Font FuenteNormal = new Font("Segoe UI", 10F);
        public static readonly Font FuentePequena = new Font("Segoe UI", 9F);
        public static readonly Font FuenteBoton = new Font("Segoe UI Semibold", 10F);
        public static readonly Font FuenteNumero = new Font("Segoe UI Semibold", 22F);

        // ---------- Botones ----------
        public static void EstilizarBotonPrimario(Button boton)
        {
            boton.BackColor = ColorPrimario;
            boton.ForeColor = ColorTextoClaro;
            boton.Font = FuenteBoton;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = ColorPrimarioHover;
            boton.FlatAppearance.MouseDownBackColor = ColorPrimarioHover;
            boton.Cursor = Cursors.Hand;
            boton.Height = 36;
            boton.UseVisualStyleBackColor = false;
        }

        public static void EstilizarBotonSecundario(Button boton)
        {
            boton.BackColor = Color.White;
            boton.ForeColor = ColorTextoOscuro;
            boton.Font = FuenteNormal;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderColor = ColorBorde;
            boton.FlatAppearance.BorderSize = 1;
            boton.FlatAppearance.MouseOverBackColor = ColorSecundario;
            boton.FlatAppearance.MouseDownBackColor = ColorBorde;
            boton.Cursor = Cursors.Hand;
            boton.Height = 36;
            boton.UseVisualStyleBackColor = false;
        }

        // ---------- Grillas ----------
        public static void EstilizarGrid(DataGridView grid)
        {
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Color.White;
            grid.GridColor = ColorBorde;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;

            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = ColorFilaAlterna;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = ColorTextoSuave;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = ColorFilaAlterna;
            grid.ColumnHeadersDefaultCellStyle.Font = FuenteBoton;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 38;

            grid.DefaultCellStyle.Font = FuenteNormal;
            grid.DefaultCellStyle.ForeColor = ColorTextoOscuro;
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.SelectionBackColor = ColorPrimarioSuave;
            grid.DefaultCellStyle.SelectionForeColor = ColorTextoOscuro;
            grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            grid.AlternatingRowsDefaultCellStyle.BackColor = ColorFilaAlterna;
            grid.RowTemplate.Height = 34;

            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AllowUserToResizeRows = false;
        }

        // ---------- Formularios completos ----------
        // Se llama una sola vez, desde frmDashboard, para cada módulo que se abre.
        // Ajusta fondo, fuentes y cajas de texto sin tocar lo que cada formulario ya personalizó.
        public static void EstilizarFormulario(Control contenedor)
        {
            if (contenedor is Form || contenedor is Panel || contenedor is GroupBox ||
                contenedor is TableLayoutPanel || contenedor is FlowLayoutPanel || contenedor is TabPage)
            {
                if (contenedor.BackColor == SystemColors.Control || contenedor.BackColor == Color.Transparent)
                    contenedor.BackColor = ColorSecundario;
            }

            foreach (Control c in contenedor.Controls)
            {
                switch (c)
                {
                    case Label lbl:
                        // Solo las etiquetas con la fuente por defecto; los títulos grandes se respetan
                        if (lbl.Font.Size <= 9.5F && !lbl.Font.Bold)
                            lbl.Font = FuenteNormal;
                        if (lbl.ForeColor == SystemColors.ControlText)
                            lbl.ForeColor = ColorTextoOscuro;
                        break;

                    case TextBox txt:
                        txt.BorderStyle = BorderStyle.FixedSingle;
                        txt.Font = FuenteNormal;
                        break;

                    case ComboBox cmb:
                        cmb.FlatStyle = FlatStyle.Flat;
                        cmb.Font = FuenteNormal;
                        break;

                    case DateTimePicker dtp:
                        dtp.Font = FuenteNormal;
                        break;

                    case NumericUpDown num:
                        num.BorderStyle = BorderStyle.FixedSingle;
                        num.Font = FuenteNormal;
                        break;

                    case CheckBox chk:
                        chk.Font = FuenteNormal;
                        break;

                    case Button btn:
                        // Botones que ningún formulario estilizó: quedan como secundarios
                        if (btn.FlatStyle == FlatStyle.Standard)
                            EstilizarBotonSecundario(btn);
                        break;

                    case DataGridView dgv:
                        if (dgv.EnableHeadersVisualStyles)
                            EstilizarGrid(dgv);
                        break;
                }

                if (c.HasChildren && c is not DataGridView && c is not UserControl)
                    EstilizarFormulario(c);
            }
        }

        // ---------- Piezas para pantallas nuevas ----------
        public static Label CrearTitulo(string texto) => new Label
        {
            Text = texto,
            Font = FuenteTitulo,
            ForeColor = ColorTextoOscuro,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4)
        };

        public static Label CrearTextoSuave(string texto) => new Label
        {
            Text = texto,
            Font = FuenteNormal,
            ForeColor = ColorTextoSuave,
            AutoSize = true
        };

        // Tarjeta blanca con borde fino (para indicadores, listas, gráficos)
        public static Panel CrearTarjeta()
        {
            var tarjeta = new Panel
            {
                BackColor = ColorTarjeta,
                Padding = new Padding(16, 14, 16, 14),
                Margin = new Padding(0, 0, 14, 14)
            };
            tarjeta.Paint += (s, e) =>
            {
                using var lapiz = new Pen(ColorBorde);
                e.Graphics.DrawRectangle(lapiz, 0, 0, tarjeta.Width - 1, tarjeta.Height - 1);
            };
            return tarjeta;
        }

        // Tarjeta de indicador: título chico, número grande, detalle abajo
        public static Panel CrearIndicador(string titulo, string valor, string detalle, Color? colorValor = null)
        {
            var tarjeta = CrearTarjeta();

            var lblDetalle = new Label
            {
                Text = detalle,
                Font = FuentePequena,
                ForeColor = ColorTextoSuave,
                Dock = DockStyle.Top,
                Height = 20
            };
            var lblValor = new Label
            {
                Text = valor,
                Font = FuenteNumero,
                ForeColor = colorValor ?? ColorTextoOscuro,
                Dock = DockStyle.Top,
                Height = 44
            };
            var lblTitulo = new Label
            {
                Text = titulo,
                Font = FuenteNormal,
                ForeColor = ColorTextoSuave,
                Dock = DockStyle.Top,
                Height = 22
            };

            // Con Dock=Top el último agregado queda arriba
            tarjeta.Controls.Add(lblDetalle);
            tarjeta.Controls.Add(lblValor);
            tarjeta.Controls.Add(lblTitulo);
            return tarjeta;
        }
    }
}
