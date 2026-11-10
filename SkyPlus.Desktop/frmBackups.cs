using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using System.Drawing;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SkyPlus.Desktop
{
    public partial class frmBackups : Form
    {
        private readonly BackupClient _backupClient = new();
        private readonly bool _esAdministrador;

        private List<BackupResponse> _backups = new();
        private bool _ocupado;

        public frmBackups(bool esAdministrador)
        {
            InitializeComponent();

            dgvBackups.AutoGenerateColumns = false;
            dgvBackups.ShowCellToolTips = true;

            dgvBackups.CellToolTipTextNeeded += (sender, e) =>
            {
                if (e.RowIndex >= 0 &&
                    e.ColumnIndex == colArchivo.Index)
                {
                    e.ToolTipText = dgvBackups
                        .Rows[e.RowIndex]
                        .Cells[e.ColumnIndex]
                        .Value?.ToString();
                }
            };

            _esAdministrador = esAdministrador;

            ConfigurarFormulario();
            ConfigurarEventos();

            dgvBackups.Resize += (sender, e) => AjustarColumnas();

            AjustarColumnas();
        }

        private void ConfigurarFormulario()
        {
            // Los controles y las columnas ya están creados por el Designer.
            Text = "SkyPlus - Gestión de Backups";
            BackColor = Color.FromArgb(245, 247, 250);

            dgvBackups.AutoGenerateColumns = false;
            dgvBackups.MultiSelect = false;
            dgvBackups.AllowUserToDeleteRows = false;
            dgvBackups.ClearSelection();

            lblEstado.Text = "Preparado.";
            btnGenerar.Visible = _esAdministrador;
            btnEliminar.Visible = _esAdministrador;
            btnRestaurar.Visible = _esAdministrador;
            btnRestaurarProduccion.Visible = _esAdministrador;
        }

        private void ConfigurarEventos()
        {
            Shown += async (_, _) =>
                await CargarHistorialAsync();

            btnActualizar.Click += async (_, _) =>
                await CargarHistorialAsync();

            btnGenerar.Click += async (_, _) =>
                await GenerarBackupAsync();

            btnEliminar.Click += async (_, _) =>
                await EliminarBackupAsync();

            btnRestaurar.Click += async (_, _) =>
                await RestaurarBackupAsync();

            btnRestaurarProduccion.Click += async (_, _) =>
                await IniciarRestauracionProduccionAsync();
        }

        private void CambiarOcupado(bool ocupado)
        {
            _ocupado = ocupado;

            btnActualizar.Enabled = !ocupado;
            btnGenerar.Enabled = !ocupado;
            btnEliminar.Enabled = !ocupado;
            btnRestaurar.Enabled = !ocupado;
            btnRestaurarProduccion.Enabled = !ocupado;

            UseWaitCursor = ocupado;
        }

        private async Task CargarHistorialAsync()
        {
            if (_ocupado) return;

            CambiarOcupado(true);
            lblEstado.Text = "Consultando backups...";

            try
            {
                await ActualizarGrillaAsync();
                lblEstado.Text =
                    $"Backups registrados: {_backups.Count}";
            }
            catch (Exception ex)
            {
                MostrarError(ex);
                lblEstado.Text = "Error al consultar backups.";
            }
            finally
            {
                CambiarOcupado(false);
            }
        }

        private async Task ActualizarGrillaAsync()
        {
            _backups =
                await _backupClient.ObtenerHistorialAsync()
                ?? new List<BackupResponse>();

            var datos = _backups.Select(b => new
            {
                b.Id,
                b.Archivo,
                FechaLocal = b.FechaUtc.ToLocalTime()
                    .ToString("dd/MM/yyyy HH:mm"),
                b.Tipo,
                b.Estado,
                TamanoMb = (b.TamanoBytes / 1024.0 / 1024.0)
                    .ToString("N2") + " MB",
                IdAdministrador = b.IdAdministrador == 0
                    ? "Sistema"
                    : b.IdAdministrador.ToString()
            }).ToList();

            dgvBackups.DataSource = null;
            dgvBackups.DataSource = datos;
            dgvBackups.ClearSelection();
        }


        private BackupResponse? ObtenerSeleccionado()
        {
            if (dgvBackups.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Seleccioná un backup de la tabla.",
                    "SkyPlus",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return null;
            }

            var fila = dgvBackups.SelectedRows[0];
            var propiedad = fila.DataBoundItem?
                .GetType().GetProperty("Id");

            if (propiedad?.GetValue(fila.DataBoundItem)
                is not Guid id)
                return null;

            return _backups.FirstOrDefault(b => b.Id == id);
        }

        private async Task GenerarBackupAsync()
        {
            if (_ocupado || !_esAdministrador) return;

            var confirmar = MessageBox.Show(
                "¿Querés generar un nuevo backup de SkyPlus?",
                "Confirmar backup",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmar != DialogResult.Yes) return;

            CambiarOcupado(true);
            lblEstado.Text = "Generando backup...";

            try
            {
                var resultado =
                    await _backupClient.GenerarBackupAsync();

                await ActualizarGrillaAsync();

                MessageBox.Show(
                    resultado?.Mensaje ??
                    "Backup generado correctamente.",
                    "SkyPlus",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                lblEstado.Text = "Backup generado.";
            }
            catch (Exception ex)
            {
                MostrarError(ex);
                lblEstado.Text = "Error al generar backup.";
            }
            finally
            {
                CambiarOcupado(false);
            }
        }

        private async Task EliminarBackupAsync()
        {
            if (_ocupado || !_esAdministrador) return;

            var backup = ObtenerSeleccionado();
            if (backup == null) return;

            if (backup.Estado != "Disponible")
            {
                MessageBox.Show(
                    "Solo se pueden eliminar backups disponibles.");
                return;
            }

            var confirmar = MessageBox.Show(
                $"¿Eliminar definitivamente este backup?\n\n" +
                $"{backup.Archivo}\n\n" +
                "Esta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmar != DialogResult.Yes) return;

            CambiarOcupado(true);

            try
            {
                await _backupClient.EliminarBackupAsync(backup.Id);

                await ActualizarGrillaAsync();

                MessageBox.Show(
                    "Backup eliminado correctamente.",
                    "SkyPlus",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
            finally
            {
                CambiarOcupado(false);
            }
        }

        private async Task RestaurarBackupAsync()
        {
            if (_ocupado || !_esAdministrador) return;

            var backup = ObtenerSeleccionado();
            if (backup == null) return;

            if (backup.Estado != "Disponible")
            {
                MessageBox.Show(
                    "El backup seleccionado no está disponible.");
                return;
            }

            using var ventana = new Form
            {
                Text = "Autorización de restauración",
                Size = new Size(465, 330),
                MinimumSize = new Size(465, 330),
                MaximumSize = new Size(465, 330),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblInfo = new Label
            {
                Text = "Restauración exclusiva en la base de prueba.\n" +
                       "Requiere otro Administrador activo.",
                Location = new Point(25, 20),
                Size = new Size(400, 45)
            };

            var lblEmail = new Label
            {
                Text = "Correo del segundo administrador:",
                Location = new Point(25, 80),
                AutoSize = true
            };

            var txtEmail = new TextBox
            {
                Location = new Point(25, 105),
                Width = 395
            };

            var lblPassword = new Label
            {
                Text = "Contraseña:",
                Location = new Point(25, 145),
                AutoSize = true
            };

            var txtPassword = new TextBox
            {
                Location = new Point(25, 170),
                Width = 395,
                UseSystemPasswordChar = true
            };

            var btnConfirmar = new Button
            {
                Text = "Autorizar restauración",
                Location = new Point(210, 225),
                Size = new Size(210, 36),
                DialogResult = DialogResult.OK
            };

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(90, 225),
                Size = new Size(110, 36),
                DialogResult = DialogResult.Cancel
            };

            ventana.Controls.AddRange(new Control[]
            {
                lblInfo, lblEmail, txtEmail,
                lblPassword, txtPassword,
                btnConfirmar, btnCancelar
            });

            ventana.AcceptButton = btnConfirmar;
            ventana.CancelButton = btnCancelar;

            if (ventana.ShowDialog(this) != DialogResult.OK)
                return;

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            txtPassword.Clear();

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Ingresá el correo y la contraseña.");
                return;
            }

            var confirmar = MessageBox.Show(
                "¿Confirmás la restauración de prueba?\n\n" +
                $"Backup: {backup.Archivo}\n" +
                "Destino: skyplus_restauracion_test\n\n" +
                "Se ejecutarán instrucciones SQL sobre la base temporal.",
                "Confirmar restauración",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmar != DialogResult.Yes)
                return;

            CambiarOcupado(true);
            lblEstado.Text = "Restaurando backup de prueba...";

            try
            {
                var resultado =
                    await _backupClient.RestaurarPruebaAsync(
                        backup.Id, email, password);

                if (resultado == null)
                    throw new InvalidOperationException("La API no devolvió el resultado de la restauración.");

                MessageBox.Show(
                    resultado.Mensaje ??
                    "Restauración de prueba finalizada.",
                    "SkyPlus",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                lblEstado.Text = "Restauración de prueba finalizada.";
            }
            catch (Exception ex)
            {
                MostrarError(ex);
                lblEstado.Text = "La restauración no se completó.";
            }
            finally
            {
                password = string.Empty;
                CambiarOcupado(false);
            }
        }

        // Transferencia diagnóstica de una autorización firmada.
        // La autorización NO habilita la importación real en esta etapa.
        private async Task IniciarRestauracionProduccionAsync()
        {
            if (_ocupado || !_esAdministrador) return;

            var backup = ObtenerSeleccionado();
            if (backup == null) return;
            if (!string.Equals(backup.Estado, "Disponible", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("El backup seleccionado no está disponible.");
                return;
            }

            var confirmar = MessageBox.Show(
                "INICIAR RESTAURACIÓN DE PRODUCCIÓN\n\n" +
                $"Backup: {backup.Archivo}\n" +
                "Destino: skyplus (producción)\n\n" +
                "Se solicitará autorización de un segundo Administrador.\n" +
                "La API generará una autorización firmada de 10 minutos.\n" +
                "RestoreLauncher cerrará Desktop y API automáticamente.\n" +
                "Generará un backup preventivo y restaurará los datos seleccionados.\n" +
                "Si todo se verifica correctamente, reiniciará API y Desktop.\n" +
                "Si falla, mantendrá el estado de recuperación para revisión.\n\n" +
                "¿Solicitar autorizaciones para iniciar el proceso?",
                "SkyPlus - Restauración de producción",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmar != DialogResult.Yes) return;

            using var ventana = new Form
            {
                Text = "Segunda autorización administrativa (producción)",
                Size = new Size(465, 310),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var lbl = new Label
            {
                Text = "Ingresá las credenciales de OTRO Administrador activo:",
                Location = new Point(22, 18),
                Size = new Size(410, 35)
            };
            var emailLabel = new Label { Text = "Correo:", Location = new Point(22, 64), AutoSize = true };
            var emailInput = new TextBox { Location = new Point(22, 87), Width = 400 };
            var passLabel = new Label { Text = "Contraseña:", Location = new Point(22, 127), AutoSize = true };
            var passInput = new TextBox
            {
                Location = new Point(22, 150),
                Width = 400,
                UseSystemPasswordChar = true
            };
            var aceptar = new Button
            {
                Text = "Autorizar inicio",
                DialogResult = DialogResult.OK,
                Location = new Point(224, 205),
                Size = new Size(198, 35)
            };
            var cancelar = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Location = new Point(105, 205),
                Size = new Size(110, 35)
            };
            ventana.Controls.AddRange(new Control[]
            {
                lbl, emailLabel, emailInput, passLabel, passInput, aceptar, cancelar
            });
            ventana.AcceptButton = aceptar;
            ventana.CancelButton = cancelar;
            if (ventana.ShowDialog(this) != DialogResult.OK) return;

            string correo = emailInput.Text.Trim();
            string password = passInput.Text;
            passInput.Clear();
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Ingresá el correo y la contraseña.");
                return;
            }

            string? rutaTemporal = null;
            string? rutaContinuacion = null;
            CambiarOcupado(true);
            lblEstado.Text = "Solicitando autorización firmada de producción...";
            try
            {
                var respuesta = await _backupClient.AutorizarRestauracionProduccionAsync(
                    backup.Id, correo, password);

                if (respuesta is null || respuesta.OperationId == Guid.Empty ||
                    respuesta.BackupId != backup.Id ||
                    string.IsNullOrWhiteSpace(respuesta.AutorizacionFirmada) ||
                    respuesta.ExpiresAtUtc <= DateTime.UtcNow)
                    throw new InvalidOperationException("Respuesta de autorización inválida o vencida.");

                lblEstado.Text = "Solicitando autorización anticipada de continuación...";
                // Se solicita una segunda aprobación independiente para la misma operación.
                var respuestaContinuacion = await _backupClient.AutorizarContinuacionProduccionAsync(
                    respuesta.OperationId, respuesta.BackupId, correo, password);
                if (respuestaContinuacion is null ||
                    respuestaContinuacion.OperationId != respuesta.OperationId ||
                    respuestaContinuacion.BackupId != respuesta.BackupId ||
                    respuestaContinuacion.ExpiresAtUtc <= DateTime.UtcNow ||
                    string.IsNullOrWhiteSpace(respuestaContinuacion.AutorizacionFirmada))
                    throw new InvalidOperationException("Autorización anticipada de continuación inválida.");

                // El Launcher descifra con DPAPI bajo el MISMO usuario de Windows.
                // No se pasan firmas ni contraseñas por la línea de comandos.
                byte[] contenido = JsonSerializer.SerializeToUtf8Bytes(new
                {
                    respuesta.OperationId,
                    respuesta.BackupId,
                    respuesta.ExpiresAtUtc,
                    respuesta.AutorizacionFirmada
                });
                byte[] cifrado = ProtectedData.Protect(
                    contenido, null, DataProtectionScope.CurrentUser);
                CryptographicOperations.ZeroMemory(contenido);

                string directorio = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SkyPlus", "RestoreHandoffs");
                Directory.CreateDirectory(directorio);
                rutaTemporal = Path.Combine(directorio, Guid.NewGuid().ToString("N") + ".bin");
                using (var archivo = new FileStream(rutaTemporal, FileMode.CreateNew,
                    FileAccess.Write, FileShare.None))
                    archivo.Write(cifrado);
                CryptographicOperations.ZeroMemory(cifrado);

                byte[] contenidoContinuacion = JsonSerializer.SerializeToUtf8Bytes(new
                {
                    respuestaContinuacion.OperationId,
                    respuestaContinuacion.BackupId,
                    respuestaContinuacion.ExpiresAtUtc,
                    respuestaContinuacion.AutorizacionFirmada
                });
                byte[] cifradoContinuacion = ProtectedData.Protect(
                    contenidoContinuacion, null, DataProtectionScope.CurrentUser);
                CryptographicOperations.ZeroMemory(contenidoContinuacion);
                rutaContinuacion = Path.Combine(directorio, Guid.NewGuid().ToString("N") + ".bin");
                using (var archivo = new FileStream(rutaContinuacion, FileMode.CreateNew,
                    FileAccess.Write, FileShare.None))
                    archivo.Write(cifradoContinuacion);
                CryptographicOperations.ZeroMemory(cifradoContinuacion);
                password = string.Empty;

                // Estructura de Visual Studio: <solución>/Desktop/bin/Debug/net10.0-windows/
                string raiz = Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory, "..", "..", "..", ".."));
                string dirLauncher = Path.Combine(raiz, "SkyPlus.RestoreLauncher", "bin");
                if (!Directory.Exists(dirLauncher))
                    throw new FileNotFoundException("No se encontró RestoreLauncher. Compilá la solución.");

                string exe = Directory.GetFiles(dirLauncher, "SkyPlus.RestoreLauncher.exe",
                    SearchOption.AllDirectories)
                    .OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault()
                    ?? throw new FileNotFoundException("No se encontró RestoreLauncher.exe.");

                var inicio = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = exe,
                    WorkingDirectory = Path.GetDirectoryName(exe)!,
                    UseShellExecute = true
                };
                inicio.ArgumentList.Add("--flujo-completo");
                inicio.ArgumentList.Add(rutaTemporal);
                inicio.ArgumentList.Add(rutaContinuacion);

                using var proceso = System.Diagnostics.Process.Start(inicio)
                    ?? throw new InvalidOperationException("No se inició RestoreLauncher.");
                rutaTemporal = null; // El Launcher borra ambas transferencias.
                rutaContinuacion = null;
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo transferir la autorización.\n\n" + ex.Message,
                    "SkyPlus - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "No se inició la restauración de producción.";
            }
            finally
            {
                password = string.Empty;
                if (rutaTemporal is not null)
                {
                    try { File.Delete(rutaTemporal); } catch { /* Permisos del sistema */ }
                }
                if (rutaContinuacion is not null)
                {
                    try { File.Delete(rutaContinuacion); } catch { /* Permisos del sistema */ }
                }
                CambiarOcupado(false);
            }
        }

        private static void MostrarError(Exception ex)
        {
            string mensaje = ex is HttpRequestException
                ? ex.Message
                : "No se pudo completar la operación. " +
                  "Verificá la conexión con la API.";

            MessageBox.Show(
                mensaje,
                "SkyPlus - Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void lblTitulo_Click(object sender, EventArgs e)
        {

        }
        private void AjustarColumnas()
        {
            int anchoMinimo = 1050;

            if (dgvBackups.ClientSize.Width >= anchoMinimo)
            {
                dgvBackups.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill;

                colArchivo.FillWeight = 35;
                colFecha.FillWeight = 18;
                colTipo.FillWeight = 10;
                colEstado.FillWeight = 13;
                colTamano.FillWeight = 12;
                colAdministrador.FillWeight = 12;
            }
            else
            {
                dgvBackups.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.None;

                colArchivo.Width = 370;
                colFecha.Width = 145;
                colTipo.Width = 100;
                colEstado.Width = 120;
                colTamano.Width = 110;
                colAdministrador.Width = 120;
            }
        }
    }
}
