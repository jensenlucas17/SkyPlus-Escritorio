using Microsoft.EntityFrameworkCore;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class ReservasExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservasExpirationService> _logger;
        public ReservasExpirationService(IServiceScopeFactory scopeFactory, ILogger<ReservasExpirationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
            try
            {
                await ExpirarAsync(stoppingToken);
                while (await timer.WaitForNextTickAsync(stoppingToken))
                    await ExpirarAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        }
        private async Task ExpirarAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SkyPlusDbContext>();
                // Bloquea escrituras concurrentes durante la expiración.
                await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
                var vencidas = await db.Reservas
                    .Where(r => r.EstadoReserva == "PendientePago" && r.FechaExpiracion != null && r.FechaExpiracion <= DateTime.UtcNow)
                    .ToListAsync(ct);
                foreach (var reserva in vencidas)
                {
                    reserva.EstadoReserva = "Expirada";
                    var asiento = await db.Asientos.FirstOrDefaultAsync(a => a.IdAsiento == reserva.IdAsiento, ct);
                    if (asiento != null) asiento.Estado = "Libre";
                }
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error al expirar reservas pendientes.");
            }
        }
    }
}