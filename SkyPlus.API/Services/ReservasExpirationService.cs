
using Microsoft.EntityFrameworkCore;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class ReservasExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservasExpirationService> _logger;
        private readonly IConfiguration _configuration;

        public ReservasExpirationService(
            IServiceScopeFactory scopeFactory,
            ILogger<ReservasExpirationService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        private bool EstaEnMantenimiento()
        {
            string ruta = _configuration[
                "Mantenimiento:ArchivoEstado"]
                ?? throw new InvalidOperationException(
                    "Falta configurar Mantenimiento:ArchivoEstado.");

            return File.Exists(ruta);
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(
                TimeSpan.FromSeconds(10));

            try
            {
                await ExpirarAsync(stoppingToken);

                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await ExpirarAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Detención normal del servicio.
            }
        }

        private async Task ExpirarAsync(CancellationToken ct)
        {
            if (EstaEnMantenimiento())
                return;

            try
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider
                    .GetRequiredService<SkyPlusDbContext>();

                // Si se activó mantenimiento mientras
                // se preparaba la operación, no continuar.
                if (EstaEnMantenimiento())
                    return;

                await using var tx =
                    await db.Database.BeginTransactionAsync(
                        System.Data.IsolationLevel.Serializable,
                        ct);

                var vencidas = await db.Reservas
                    .Where(r =>
                        r.EstadoReserva == "PendientePago" &&
                        r.FechaExpiracion != null &&
                        r.FechaExpiracion <= DateTime.UtcNow)
                    .ToListAsync(ct);

                foreach (var reserva in vencidas)
                {
                    reserva.EstadoReserva = "Expirada";

                    var asiento = await db.Asientos
                        .FirstOrDefaultAsync(
                            a => a.IdAsiento == reserva.IdAsiento,
                            ct);

                    if (asiento != null)
                        asiento.Estado = "Libre";
                }

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(
                    ex,
                    "Error al expirar reservas pendientes.");
            }
        }
    }
}
