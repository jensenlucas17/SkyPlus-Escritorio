
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
    public class frmRegistrarReembolso : Form
    {
        private readonly PagoClient _pagos = new();
        private readonly ReservaClient _reservas = new();
        private readonly ReembolsoClient _reembolsos = new();

        private readonly ComboBox cmbPago = new()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 390
        };

        private readonly Label lblImporte = new()
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold)
        };

        private readonly TextBox txtMotivo = new()
        {
            Width = 390,
            Multiline = true,
            Height = 75,
            MaxLength = 500,
            ScrollBars = ScrollBars.Vertical
        };

        private readonly Button btnConfirmar = new()
        {
            Text = "Registrar reembolso",
            Width = 170
        };

        private readonly Button btnCancelar = new()
        {
            Text = "Cancelar",
            Width = 110
        };

        private bool _registrando = false;

        private sealed class OpcionPago
        {
            public PagoResponse Pago { get; init; } = null!;

            public ReservaResponse Reserva { get; init; } = null!;

            public override string ToString()
            {
                return $"Pago #{Pago.IdPago} - " +
                       $"Reserva {Reserva.CodigoReserva} - " +
                       $"{Reserva.Pasajero}";
            }
        }

        public frmRegistrarReembolso()
        {
            Text = "Registrar reembolso";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(465, 365);
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
                Text = "Pago a reembolsar",
                AutoSize = true
            });

            panel.Controls.Add(cmbPago);

            panel.Controls.Add(new Label
            {
                Text = "Importe del reembolso",
                AutoSize = true,
                Margin = new Padding(3, 15, 3, 3)
            });

            panel.Controls.Add(lblImporte);

            panel.Controls.Add(new Label
            {
                Text = "Motivo del reembolso",
                AutoSize = true,
                Margin = new Padding(3, 15, 3, 3)
            });

            panel.Controls.Add(txtMotivo);

            var botones = new FlowLayoutPanel
            {
                AutoSize = true,
                Margin = new Padding(0, 18, 0, 0)
            };

            botones.Controls.Add(btnConfirmar);
            botones.Controls.Add(btnCancelar);

            panel.Controls.Add(botones);
            Controls.Add(panel);

            cmbPago.SelectedIndexChanged += (_, _) =>
                ActualizarDetalle();

            btnConfirmar.Click += Registrar_Click;

            btnCancelar.Click += (_, _) => Close();

            Load += async (_, _) =>
                await CargarPagosAsync();
        }

        private async Task CargarPagosAsync()
        {
            btnConfirmar.Enabled = false;

            try
            {
                var pagos = await _pagos.ObtenerTodosAsync();

                var reservas =
                    await _reservas.ObtenerTodosAsync()
                    ?? new List<ReservaResponse>();

                var reembolsos =
                    await _reembolsos.ObtenerTodosAsync();

                var idsReembolsados = reembolsos
                    .Select(r => r.IdPago)
                    .ToHashSet();

                var reservasConfirmadas = reservas
                    .Where(r => r.EstadoReserva == "Confirmada")
                    .ToDictionary(r => r.IdReserva);

                var opciones = pagos
                    .Where(p =>
                        p.EstadoPago == "Registrado" &&
                        p.Monto > 0 &&
                        !idsReembolsados.Contains(p.IdPago) &&
                        reservasConfirmadas.ContainsKey(p.IdReserva))
                    .Select(p => new OpcionPago
                    {
                        Pago = p,
                        Reserva = reservasConfirmadas[p.IdReserva]
                    })
                    .OrderBy(p => p.Pago.IdPago)
                    .ToList();

                cmbPago.DataSource = null;
                cmbPago.DataSource = opciones;

                if (opciones.Count == 0)
                {
                    MessageBox.Show(
                        "No hay pagos disponibles para reembolsar.",
                        "Reembolsos",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                ActualizarDetalle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al cargar pagos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ActualizarDetalle()
        {
            var opcion = cmbPago.SelectedItem as OpcionPago;

            lblImporte.Text = opcion == null
                ? "—"
                : opcion.Pago.Monto.ToString("C2");

            btnConfirmar.Enabled =
                opcion != null && !_registrando;
        }

        private async void Registrar_Click(
            object? sender,
            EventArgs e)
        {
            if (_registrando)
                return;

            var opcion = cmbPago.SelectedItem as OpcionPago;

            if (opcion == null)
            {
                MessageBox.Show(
                    "Seleccioná un pago.",
                    "Reembolsos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (SesionUsuario.IdUsuario <= 0)
            {
                MessageBox.Show(
                    "No se pudo identificar al agente.",
                    "Reembolsos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string motivo = txtMotivo.Text.Trim();

            if (string.IsNullOrWhiteSpace(motivo))
            {
                MessageBox.Show(
                    "Ingresá el motivo del reembolso.",
                    "Reembolsos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Confirmás el reembolso?\n\n" +
                $"Reserva: {opcion.Reserva.CodigoReserva}\n" +
                $"Pasajero: {opcion.Reserva.Pasajero}\n" +
                $"Monto: {opcion.Pago.Monto:C2}\n\n" +
                "Esta operación cancelará la reserva y liberará el asiento.",
                "Confirmar reembolso",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes)
                return;

            _registrando = true;
            btnConfirmar.Enabled = false;

            try
            {
                var reembolso = await _reembolsos.CrearAsync(
                    opcion.Pago.IdPago,
                    SesionUsuario.IdUsuario,
                    motivo);

                if (reembolso == null)
                    throw new InvalidOperationException(
                        "La API no devolvió el reembolso registrado.");

                MessageBox.Show(
                    $"Reembolso registrado correctamente.\n\n" +
                    $"Número: {reembolso.IdReembolso}\n" +
                    $"Monto: {reembolso.MontoReembolsado:C2}\n\n" +
                    "La reserva fue cancelada y el asiento liberado.",
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
                    "No se pudo registrar el reembolso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                await CargarPagosAsync();
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
                _registrando = false;

                if (!IsDisposed && !Disposing)
                    ActualizarDetalle();
            }
        }
    }
}
