
using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using System;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmPasajeroABM : Form
    {
        private readonly PasajeroClient _pasajeroClient;
        private readonly PasajeroResponse? _pasajeroEdicion;

        public frmPasajeroABM(
            PasajeroClient pasajeroClient,
            PasajeroResponse? pasajeroExistente)
        {
            InitializeComponent();

            _pasajeroClient = pasajeroClient;
            _pasajeroEdicion = pasajeroExistente;
        }

        private void frmPasajeroABM_Load(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            Text = _pasajeroEdicion == null
                ? "Nuevo pasajero"
                : "Editar pasajero";

            DisenarFormulario();

            if (_pasajeroEdicion != null)
            {
                txtNombre.Text = _pasajeroEdicion.Nombre;
                txtApellido.Text = _pasajeroEdicion.Apellido;
                txtDocumento.Text = _pasajeroEdicion.Documento;
                txtNacionalidad.Text = _pasajeroEdicion.Nacionalidad;
                txtEmail.Text = _pasajeroEdicion.Email;
                txtTelefono.Text = _pasajeroEdicion.Telefono;
            }
        }

        // ---------------------------------------------------------------------
        // Diseño: encabezado oscuro y campos en dos columnas (mismo estilo que Nueva reserva)
        // ---------------------------------------------------------------------
        private void DisenarFormulario()
        {
            Controls.Clear();
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(640, 470);
            BackColor = Color.White;
            Font = AppEstilos.FuenteNormal;

            var encabezado = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = AppEstilos.ColorLateral, Padding = new Padding(28, 0, 28, 0) };
            encabezado.Controls.Add(new Label
            {
                Text = _pasajeroEdicion == null ? "Nuevo pasajero" : "Editar pasajero",
                Font = AppEstilos.FuenteTitulo,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(28, 12)
            });
            encabezado.Controls.Add(new Label
            {
                Text = _pasajeroEdicion == null
                    ? "Cargá los datos tal como figuran en el documento de viaje."
                    : $"{_pasajeroEdicion.Apellido}, {_pasajeroEdicion.Nombre} · DNI {_pasajeroEdicion.Documento}",
                Font = AppEstilos.FuenteNormal,
                ForeColor = AppEstilos.ColorLateralTexto,
                AutoSize = true,
                Location = new Point(30, 44)
            });
            Controls.Add(encabezado);

            const int izquierda = 32, derecha = 336, ancho = 272;
            int y = 100;
            void Campo(string titulo, TextBox caja, int x, int fila, string ayuda = "")
            {
                int top = y + fila * 78;
                Controls.Add(new Label
                {
                    Text = titulo,
                    Font = AppEstilos.FuenteBoton,
                    ForeColor = AppEstilos.ColorTextoOscuro,
                    AutoSize = true,
                    Location = new Point(x, top)
                });
                caja.Font = new Font("Segoe UI", 11F);
                caja.BorderStyle = BorderStyle.FixedSingle;
                caja.PlaceholderText = ayuda;
                caja.SetBounds(x, top + 24, ancho, 30);
                Controls.Add(caja);
            }

            Campo("Nombre", txtNombre, izquierda, 0);
            Campo("Apellido", txtApellido, derecha, 0);
            Campo("Documento (DNI o pasaporte)", txtDocumento, izquierda, 1, "Sin puntos");
            Campo("Nacionalidad", txtNacionalidad, derecha, 1, "Ej.: Argentina");
            Campo("Email", txtEmail, izquierda, 2, "nombre@mail.com");
            Campo("Teléfono", txtTelefono, derecha, 2, "Ej.: 379 4123456");

            lblMensaje.AutoSize = true;
            lblMensaje.MaximumSize = new Size(576, 0);
            lblMensaje.Font = AppEstilos.FuenteBoton;
            lblMensaje.ForeColor = AppEstilos.ColorError;
            lblMensaje.Location = new Point(izquierda, y + 3 * 78 + 2);
            Controls.Add(lblMensaje);

            var pie = new Panel { Dock = DockStyle.Bottom, Height = 76, BackColor = AppEstilos.ColorFilaAlterna };
            pie.Paint += (s, e) =>
            {
                using var lapiz = new Pen(AppEstilos.ColorBorde);
                e.Graphics.DrawLine(lapiz, 0, 0, pie.Width, 0);
            };
            btnGuardar.Text = _pasajeroEdicion == null ? "Guardar pasajero" : "Guardar cambios";
            btnCancelar.Text = "Cancelar";
            AppEstilos.EstilizarBotonPrimario(btnGuardar);
            AppEstilos.EstilizarBotonSecundario(btnCancelar);
            btnGuardar.SetBounds(640 - 32 - 190, 18, 190, 40);
            btnCancelar.SetBounds(btnGuardar.Left - 12 - 120, 18, 120, 40);
            pie.Controls.Add(btnGuardar);
            pie.Controls.Add(btnCancelar);
            Controls.Add(pie);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
            ActiveControl = txtNombre;
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            // Validaciones básicas de interfaz
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MostrarError("El nombre es obligatorio.");
                txtNombre.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MostrarError("El apellido es obligatorio.");
                txtApellido.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                MostrarError("El documento es obligatorio.");
                txtDocumento.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNacionalidad.Text))
            {
                MostrarError("La nacionalidad es obligatoria.");
                txtNacionalidad.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MostrarError("El email es obligatorio.");
                txtEmail.Focus();
                return;
            }

            // El teléfono es opcional.
            // Las validaciones de formato se realizan en la API.

            btnGuardar.Enabled = false;

            try
            {
                if (_pasajeroEdicion == null)
                {
                    // CREAR PASAJERO
                    await _pasajeroClient.CrearAsync(
                        txtNombre.Text.Trim(),
                        txtApellido.Text.Trim(),
                        txtDocumento.Text.Trim(),
                        txtNacionalidad.Text.Trim(),
                        txtEmail.Text.Trim(),
                        txtTelefono.Text.Trim());
                }
                else
                {
                    // ACTUALIZAR PASAJERO
                    await _pasajeroClient.ActualizarAsync(
                        _pasajeroEdicion.IdPasajero,
                        txtNombre.Text.Trim(),
                        txtApellido.Text.Trim(),
                        txtDocumento.Text.Trim(),
                        txtNacionalidad.Text.Trim(),
                        txtEmail.Text.Trim(),
                        txtTelefono.Text.Trim());
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    MostrarError(
                        "Tu sesión expiró o no es válida. Iniciá sesión nuevamente.");
                }
                else if (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    MostrarError(
                        "No tenés permisos para realizar esta operación.");
                }
                else if (ex.StatusCode.HasValue)
                {
                    // Mostrar el mensaje específico enviado por la API
                    MostrarError(ex.Message);
                }
                else
                {
                    MostrarError(
                        "No se pudo conectar con la API: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MostrarError("No se pudo guardar: " + ex.Message);
            }
            finally
            {
                btnGuardar.Enabled = true;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void MostrarError(string mensaje)
        {
            // El mensaje va debajo de los campos, arriba de la barra de botones
            lblMensaje.Text = "⚠  " + mensaje;
            lblMensaje.ForeColor = AppEstilos.ColorError;
            lblMensaje.Font = AppEstilos.FuenteBoton;
            lblMensaje.AutoSize = true;
            lblMensaje.MaximumSize = new Size(ClientSize.Width - 64, 0);
            lblMensaje.Location = new Point(32, 344);

            lblMensaje.Visible = true;
            lblMensaje.BringToFront();
        }
    }
}
