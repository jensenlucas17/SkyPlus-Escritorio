// SkyPlus.Desktop/Estilos/FormularioDialogo.cs
// Diseño común para los formularios de alta/edición (ABM): encabezado oscuro,
// campos en dos columnas con su título arriba, mensaje de error y barra de botones abajo.
// Reubica los controles que ya creó el diseñador: no cambia su lógica ni sus eventos.

using System.Drawing;
using System.Windows.Forms;

namespace SkyPlus.Desktop.Estilos
{
    public static class FormularioDialogo
    {
        private const int Ancho = 640;
        private const int MargenX = 32;
        private const int AnchoCampo = 272;
        private const int AltoFila = 78;
        private const int Inicio = 100;

        public static void Disenar(
            Form form,
            string titulo,
            string subtitulo,
            Label lblMensaje,
            Button btnGuardar,
            string textoGuardar,
            Button btnCancelar,
            params (Label Etiqueta, string Titulo, Control Control, int Columna, int Fila, string Ayuda)[] campos)
        {
            form.Controls.Clear();
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.StartPosition = FormStartPosition.CenterParent;
            form.BackColor = Color.White;
            form.Font = AppEstilos.FuenteNormal;

            int filas = campos.Length == 0 ? 0 : campos.Max(c => c.Fila) + 1;
            int yMensaje = Inicio + filas * AltoFila + 2;
            form.ClientSize = new Size(Ancho, yMensaje + 56 + 76);

            // ----- Encabezado -----
            var encabezado = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = AppEstilos.ColorLateral };
            encabezado.Controls.Add(new Label
            {
                Text = titulo,
                Font = AppEstilos.FuenteTitulo,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(28, 12)
            });
            encabezado.Controls.Add(new Label
            {
                Text = subtitulo,
                Font = AppEstilos.FuenteNormal,
                ForeColor = AppEstilos.ColorLateralTexto,
                AutoSize = true,
                Location = new Point(30, 44)
            });
            form.Controls.Add(encabezado);

            // ----- Campos -----
            foreach (var (etiqueta, textoTitulo, control, columna, fila, ayuda) in campos)
            {
                int x = columna == 0 ? MargenX : MargenX + AnchoCampo + 32;
                int y = Inicio + fila * AltoFila;

                etiqueta.Text = textoTitulo;
                etiqueta.AutoSize = true;
                etiqueta.Font = AppEstilos.FuenteBoton;
                etiqueta.ForeColor = AppEstilos.ColorTextoOscuro;
                etiqueta.Location = new Point(x, y);
                form.Controls.Add(etiqueta);

                switch (control)
                {
                    case TextBox txt:
                        txt.Font = new Font("Segoe UI", 11F);
                        txt.BorderStyle = BorderStyle.FixedSingle;
                        if (!string.IsNullOrEmpty(ayuda)) txt.PlaceholderText = ayuda;
                        break;
                    case ComboBox cmb:
                        cmb.Font = new Font("Segoe UI", 11F);
                        cmb.FlatStyle = FlatStyle.Flat;
                        break;
                    case DateTimePicker dtp:
                        dtp.Font = new Font("Segoe UI", 11F);
                        break;
                }
                control.SetBounds(x, y + 24, AnchoCampo, 30);
                form.Controls.Add(control);
            }

            // ----- Mensaje de error (debajo de los campos) -----
            lblMensaje.AutoSize = true;
            lblMensaje.MaximumSize = new Size(Ancho - MargenX * 2, 0);
            lblMensaje.Font = AppEstilos.FuenteBoton;
            lblMensaje.ForeColor = AppEstilos.ColorError;
            lblMensaje.TextAlign = ContentAlignment.TopLeft;
            lblMensaje.Location = new Point(MargenX, yMensaje);
            form.Controls.Add(lblMensaje);

            // ----- Botones -----
            var pie = new Panel { Dock = DockStyle.Bottom, Height = 76, BackColor = AppEstilos.ColorFilaAlterna };
            pie.Paint += (s, e) =>
            {
                using var lapiz = new Pen(AppEstilos.ColorBorde);
                e.Graphics.DrawLine(lapiz, 0, 0, pie.Width, 0);
            };
            btnGuardar.Text = textoGuardar;
            btnCancelar.Text = "Cancelar";
            AppEstilos.EstilizarBotonPrimario(btnGuardar);
            AppEstilos.EstilizarBotonSecundario(btnCancelar);
            btnGuardar.SetBounds(Ancho - MargenX - 190, 18, 190, 40);
            btnCancelar.SetBounds(btnGuardar.Left - 12 - 120, 18, 120, 40);
            pie.Controls.Add(btnGuardar);
            pie.Controls.Add(btnCancelar);
            form.Controls.Add(pie);

            form.CancelButton = btnCancelar;
        }

        // Muestra un error debajo de los campos (sin moverlo de lugar)
        public static void MostrarError(Label lblMensaje, string mensaje)
        {
            lblMensaje.Text = "⚠  " + mensaje;
            lblMensaje.ForeColor = AppEstilos.ColorError;
            lblMensaje.Visible = true;
            lblMensaje.BringToFront();
        }
    }
}
