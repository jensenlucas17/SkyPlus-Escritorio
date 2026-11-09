using Microsoft.EntityFrameworkCore;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    // Pasa a "Finalizado" los vuelos que ya llegaron a destino.
    // Así cada vuelo cierra su ciclo solo: deja de aparecer como vuelo en venta,
    // sus reservas pasan al historial (vuelos finalizados) y la aeronave
    // queda libre para el próximo vuelo (después del tiempo de escala).
    public class VuelosFinalizacionService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<VuelosFinalizacionService> _logger;

        public VuelosFinalizacionService(IServiceScopeFactory scopeFactory, ILogger<VuelosFinalizacionService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            try
            {
                await FinalizarAsync(stoppingToken);
                while (await timer.WaitForNextTickAsync(stoppingToken))
                    await FinalizarAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        }

        private async Task FinalizarAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SkyPlusDbContext>();

                // Las fechas de los vuelos se guardan en hora local (igual que se cargan)
                var ahora = DateTime.Now;

                var terminados = await db.Vuelos
                    .Where(v => (v.EstadoVuelo == "Programado" || v.EstadoVuelo == "Confirmado") &&
                                v.Llegada <= ahora)
                    .ToListAsync(ct);

                if (terminados.Count == 0) return;

                foreach (var vuelo in terminados)
                    vuelo.EstadoVuelo = "Finalizado";

                await db.SaveChangesAsync(ct);
                _logger.LogInformation("Se finalizaron {Cantidad} vuelos.", terminados.Count);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error al finalizar vuelos.");
            }
        }
    }
}
