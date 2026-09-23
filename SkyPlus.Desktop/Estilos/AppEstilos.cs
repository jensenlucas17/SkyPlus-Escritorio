// SkyPlus.Desktop/Estilos/AppEstilos.cs

using System.Drawing;
using System.Windows.Forms;

namespace SkyPlus.Desktop.Estilos
{
    public static class AppEstilos
    {
        public static readonly Color ColorPrimario = ColorTranslator.FromHtml("#0B5394");   // azul aerolínea
        public static readonly Color ColorSecundario = ColorTranslator.FromHtml("#F4F6F8"); // gris muy claro, fondo
        public static readonly Color ColorAcento = ColorTranslator.FromHtml("#E8710A");     // naranja, para alertas/acciones importantes
        public static readonly Color ColorTextoClaro = Color.White;
        public static readonly Color ColorTextoOscuro = ColorTranslator.FromHtml("#1F2937");
        public static readonly Color ColorError = ColorTranslator.FromHtml("#C0392B");

        public static readonly Font FuenteTitulo = new Font("Segoe UI", 16, FontStyle.Bold);
        public static readonly Font FuenteNormal = new Font("Segoe UI", 10);
        public static readonly Font FuenteBoton = new Font("Segoe UI", 10, FontStyle.Bold);

        public static void EstilizarBotonPrimario(Button boton)
        {
            boton.BackColor = ColorPrimario;
            boton.ForeColor = ColorTextoClaro;
            boton.Font = FuenteBoton;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Cursor = Cursors.Hand;
            boton.Height = 36;
        }

        public static void EstilizarBotonSecundario(Button boton)
        {
            boton.BackColor = ColorSecundario;
            boton.ForeColor = ColorTextoOscuro;
            boton.Font = FuenteNormal;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderColor = ColorPrimario;
            boton.FlatAppearance.BorderSize = 1;
            boton.Cursor = Cursors.Hand;
            boton.Height = 36;
        }

        public static void EstilizarGrid(DataGridView grid)
        {
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.BackColor = ColorPrimario;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = ColorTextoClaro;
            grid.ColumnHeadersDefaultCellStyle.Font = FuenteBoton;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.AlternatingRowsDefaultCellStyle.BackColor = ColorSecundario;
            grid.DefaultCellStyle.Font = FuenteNormal;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        }
    }
}