
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkyPlus.API.Exceptions;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class BoardingPassService
    {
        private readonly SkyPlusDbContext _context;

        public BoardingPassService(SkyPlusDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerarPdfAsync(int idCheckIn)
        {
            var checkIn = await _context.CheckIns
                .AsNoTracking()
                .Include(c => c.Reserva)
                    .ThenInclude(r => r!.Pasajero)
                .Include(c => c.Reserva)
                    .ThenInclude(r => r!.Vuelo)
                .Include(c => c.Reserva)
                    .ThenInclude(r => r!.Asiento)
                .FirstOrDefaultAsync(c =>
                    c.IdCheckin == idCheckIn);

            if (checkIn == null)
                throw new ValidacionNegocioException(
                    "El check-in no existe.");

            if (checkIn.EstadoCheckin != "Realizado")
                throw new ValidacionNegocioException(
                    "El check-in no está habilitado.");

            var reserva = checkIn.Reserva;

            if (reserva?.Pasajero == null ||
                reserva.Vuelo == null ||
                reserva.Asiento == null)
            {
                throw new ValidacionNegocioException(
                    "Faltan datos para generar la tarjeta de embarque.");
            }

            // Generar imagen QR a partir del token guardado.
            using var qrGenerator = new QRCodeGenerator();

            using var qrData = qrGenerator.CreateQrCode(
                checkIn.CodigoQr,
                QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrData);
            byte[] imagenQr = qrCode.GetGraphic(10);

            string pasajero =
                $"{reserva.Pasajero.Apellido}, " +
                $"{reserva.Pasajero.Nombre}";

            string vuelo = reserva.Vuelo.NumeroVuelo;
            string asiento =
                $"{reserva.Asiento.Fila}" +
                $"{reserva.Asiento.Letra}";

            string codigoReserva = reserva.CodigoReserva;

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Text("SKYPLUS")
                            .FontSize(25)
                            .Bold()
                            .FontColor(Colors.Blue.Darken3);

                        col.Item().Text("TARJETA DE EMBARQUE")
                            .FontSize(15)
                            .SemiBold();

                        col.Item().LineHorizontal(1);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(datos =>
                            {
                                datos.Spacing(10);

                                datos.Item().Text(
                                    $"PASAJERO: {pasajero}");

                                datos.Item().Text(
                                    $"VUELO: {vuelo}");

                                datos.Item().Text(
                                    $"RESERVA: {codigoReserva}");

                                datos.Item().Text(
                                    $"ASIENTO: {asiento}");

                                datos.Item().Text(
                                    $"CHECK-IN: " +
                                    $"{checkIn.FechaCheckin:dd/MM/yyyy HH:mm} UTC");
                            });

                            row.ConstantItem(140)
                                .Image(imagenQr)
                                .FitArea();
                        });

                        col.Item().LineHorizontal(1);

                        col.Item().Text(
                            "Presentá esta tarjeta de embarque " +
                            "junto con tu documentación.")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken2);
                    });
                });
            });

            return documento.GeneratePdf();
        }
    }
}
