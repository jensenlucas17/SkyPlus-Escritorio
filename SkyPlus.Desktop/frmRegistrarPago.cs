
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public class frmRegistrarPago : Form
    {
        private readonly ReservaClient _reservas = new();
        private readonly PagoClient _pagos = new();

        private readonly ComboBox cmbReserva = new()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 350
        };

        private readonly ComboBox cmbMetodo = new()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 350
        };

        private readonly Label lblImporte = new()
        {
            AutoSize = true
        };

        private readonly Label lblVencimiento = new()
        {
            AutoSize = true
        };

        private readonly Button btnConfirmar = new()
        {
            Text = "Registrar pago",
            Width = 140
        };

        private readonly Button btnCancelar = new()
        {
            Text = "Cancelar",
            Width = 110
        };

        private readonly System.Windows.Forms.Timer _timer = new()
        {
            Interval = 1000
        };

        private bool _registrandoPago = false;

        private sealed class OpcionReserva
        {
            public ReservaResponse Reserva { get; init; } = null!;

            public override string ToString()
            {
                return $"{Reserva.CodigoReserva} - " +
                       $"{Reserva.Pasajero} - {Reserva.Vuelo}";
            }
        }

        public frmRegistrarPago()
        {
            Text = "Registrar pago";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(460, 285);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(18),
                AutoScroll = true
            };

            panel.Controls.Add(new Label
            {
                Text = "Reserva pendiente",
                AutoSize = true
            });

            panel.Controls.Add(cmbReserva);

            panel.Controls.Add(new Label
            {
                Text = "Importe",
                AutoSize = true,
                Margin = new Padding(3, 12, 3, 3)
            });

            panel.Controls.Add(lblImporte);
            panel.Controls.Add(lblVencimiento);

            panel.Controls.Add(new Label
            {
                Text = "Método de pago",
                AutoSize = true,
                Margin = new Padding(3, 12, 3, 3)
            });

            panel.Controls.Add(cmbMetodo);

            var botones = new FlowLayoutPanel
            {
                AutoSize = true,
                Margin = new Padding(0, 18, 0, 0)
            };

            botones.Controls.Add(btnConfirmar);
            botones.Controls.Add(btnCancelar);

            panel.Controls.Add(botones);
            Controls.Add(panel);

            cmbMetodo.Items.AddRange(new object[]
            {
                "Efectivo",
                "Transferencia",
                "Tarjeta"
            });

            cmbMetodo.SelectedIndex = 0;

            cmbReserva.SelectedIndexChanged +=
                (_, _) => ActualizarDetalle();

            btnConfirmar.Click += Registrar_Click;

            btnCancelar.Click += (_, _) => Close();

            _timer.Tick += (_, _) => ActualizarDetalle();

            Load += async (_, _) =>
                await CargarPendientesAsync();

            FormClosed += (_, _) => _timer.Dispose();
        }

        private async Task CargarPendientesAsync()
        {
            btnConfirmar.Enabled = false;
            _timer.Stop();

            try
            {
                var reservas =
                    await _reservas.ObtenerTodosAsync()
                    ?? new List<ReservaResponse>();

                var pagos = await _pagos.ObtenerTodosAsync();

                var idsPagados = pagos
                    .Select(p => p.IdReserva)
                    .ToHashSet();

                var opciones = reservas
                    .Where(r =>
                        string.Equals(
                            r.EstadoReserva,
                            "PendientePago",
                            StringComparison.OrdinalIgnoreCase)
                        &&
                        r.FechaExpiracion.HasValue
                        &&
                        r.FechaExpiracion.Value > DateTime.Now
                        &&
                        !idsPagados.Contains(r.IdReserva))
                    .OrderBy(r => r.FechaExpiracion)
                    .Select(r => new OpcionReserva
                    {
                        Reserva = r
                    })
                    .ToList();

                cmbReserva.DataSource = null;
                cmbReserva.DataSource = opciones;

                if (opciones.Count == 0)
                {
                    MessageBox.Show(
                        "No hay reservas pendientes de pago vigentes.",
                        "Pagos",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                _timer.Start();
                ActualizarDetalle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al cargar reservas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ActualizarDetalle()
        {
            var reserva =
                (cmbReserva.SelectedItem as OpcionReserva)?.Reserva;

            lblImporte.Text = reserva == null
                ? "—"
                : reserva.TarifaBase.ToString("C2");

            if (reserva?.FechaExpiracion is DateTime fecha)
            {
                var restante = fecha - DateTime.Now;

                if (restante > TimeSpan.Zero)
                {
                    lblVencimiento.Text =
                        $"Tiempo restante: {restante:mm\\:ss}";

                    btnConfirmar.Enabled = !_registrandoPago;
                }
                else
                {
                    lblVencimiento.Text =
                        "Reserva vencida: actualizá la lista";

                    btnConfirmar.Enabled = false;
                }
            }
            else
            {
                lblVencimiento.Text =
                    "Sin reserva seleccionada";

                btnConfirmar.Enabled = false;
            }
        }

        private async void Registrar_Click(
            object? sender,
            EventArgs e)
        {
            if (_registrandoPago)
                return;

            var reserva =
                (cmbReserva.SelectedItem as OpcionReserva)?.Reserva;

            if (reserva == null)
            {
                MessageBox.Show(
                    "Seleccioná una reserva.",
                    "Pagos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (SesionUsuario.IdUsuario <= 0)
            {
                MessageBox.Show(
                    "No se pudo identificar al agente de la sesión.",
                    "Pagos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!reserva.FechaExpiracion.HasValue ||
                reserva.FechaExpiracion.Value <= DateTime.Now)
            {
                MessageBox.Show(
                    "La reserva ya venció. No se puede registrar el pago.",
                    "Pagos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                await CargarPendientesAsync();
                return;
            }

            _registrandoPago = true;
            btnConfirmar.Enabled = false;

            try
            {
                var pago = await _pagos.CrearAsync(
                    reserva.IdReserva,
                    SesionUsuario.IdUsuario,
                    cmbMetodo.SelectedItem?.ToString() ?? "");

                if (pago == null)
                {
                    throw new InvalidOperationException(
                        "La API no devolvió el pago registrado.");
                }

                MessageBox.Show(
                    $"Pago registrado correctamente.\n" +
                    $"Número de pago: {pago.IdPago}",
                    "SkyPlus",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "No se pudo registrar el pago",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                await CargarPendientesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _registrandoPago = false;

                if (!IsDisposed && !Disposing)
                    ActualizarDetalle();
            }
        }
    }
}
