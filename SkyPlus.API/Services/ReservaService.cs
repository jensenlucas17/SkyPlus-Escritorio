using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class ReservaService : IReservaService
    {
        private readonly SkyPlusDbContext _context;

        public ReservaService(SkyPlusDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReservaResponse>> ObtenerTodosAsync()
        {
            return await _context.Reservas
                .Include(r => r.Pasajero)
                .Include(r => r.Vuelo)
                    .ThenInclude(v => v!.LugarOrigen)
                .Include(r => r.Vuelo)
                    .ThenInclude(v => v!.LugarDestino)
                .Include(r => r.Asiento)
                .Select(r => MapearReserva(r))
                .ToListAsync();
        }

        public async Task<ReservaResponse?> ObtenerPorIdAsync(
            int id)
        {
            var reserva =
                await _context.Reservas
                    .Include(r => r.Pasajero)
                    .Include(r => r.Vuelo)
                        .ThenInclude(v => v!.LugarOrigen)
                    .Include(r => r.Vuelo)
                        .ThenInclude(v => v!.LugarDestino)
                    .Include(r => r.Asiento)
                    .FirstOrDefaultAsync(r =>
                        r.IdReserva == id);

            return reserva == null
                ? null
                : MapearReserva(reserva);
        }

        public async Task<List<ReservaResponse>> ObtenerPorPasajeroAsync(
            int idPasajero)
        {
            return await _context.Reservas
                .Where(r => r.IdPasajero == idPasajero)
                .Include(r => r.Pasajero)
                .Include(r => r.Vuelo)
                    .ThenInclude(v => v!.LugarOrigen)
                .Include(r => r.Vuelo)
                    .ThenInclude(v => v!.LugarDestino)
                .Include(r => r.Asiento)
                .Select(r => MapearReserva(r))
                .ToListAsync();
        }

        public async Task<ReservaResponse?> CrearAsync(
            CrearReservaRequest request)
        {
            // 1. Verificar pasajero
            var pasajero =
                await _context.Pasajeros
                    .FirstOrDefaultAsync(p =>
                        p.IdPasajero == request.IdPasajero);

            if (pasajero == null)
                return null;

            // 2. Verificar vuelo
            var vuelo =
                await _context.Vuelos
                    .FirstOrDefaultAsync(v =>
                        v.IdVuelo == request.IdVuelo);

            if (vuelo == null)
                return null;

            // 3. Verificar asiento
            var asiento =
                await _context.Asientos
                    .FirstOrDefaultAsync(a =>
                        a.IdAsiento == request.IdAsiento);

            if (asiento == null)
                return null;

            // 4. El asiento debe pertenecer al vuelo
            if (asiento.IdVuelo != request.IdVuelo)
                return null;

            // 5. El asiento debe estar libre
            if (asiento.Estado != "Libre")
                return null;

            // 6. Verificar que no exista una reserva
            // activa para ese asiento
            var asientoReservado =
                await _context.Reservas
                    .AnyAsync(r =>
                        r.IdAsiento == request.IdAsiento &&
                        r.IdVuelo == request.IdVuelo &&
                        r.EstadoReserva != "Cancelada");

            if (asientoReservado)
                return null;

            // 7. Verificar usuario agente
            var usuario =
                await _context.Usuarios
                    .FirstOrDefaultAsync(u =>
                        u.IdUsuario == request.IdUsuarioAgente);

            if (usuario == null)
                return null;

            // 8. Generar código
            var codigoReserva =
                await GenerarCodigoReservaAsync();

            // 9. Crear reserva
            var reserva = new Domain.Entities.Reserva
            {
                IdPasajero = request.IdPasajero,
                IdVuelo = request.IdVuelo,
                IdAsiento = request.IdAsiento,
                IdUsuarioAgente = request.IdUsuarioAgente,
                CodigoReserva = codigoReserva,
                TarifaBase = vuelo.Tarifa,
                EstadoReserva = "Confirmada",
                FechaReserva = DateTime.Now
            };

            // 10. Ocupar asiento
            asiento.Estado = "Ocupado";

            _context.Reservas.Add(reserva);

            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(
                reserva.IdReserva);
        }

        public async Task<bool> CancelarAsync(
            int id)
        {
            var reserva =
                await _context.Reservas
                    .FirstOrDefaultAsync(r =>
                        r.IdReserva == id);

            if (reserva == null)
                return false;

            if (reserva.EstadoReserva == "Cancelada")
                return false;

            reserva.EstadoReserva = "Cancelada";
            reserva.FechaCancelacion = DateTime.Now;

            var asiento =
                await _context.Asientos
                    .FirstOrDefaultAsync(a =>
                        a.IdAsiento == reserva.IdAsiento);

            if (asiento != null)
            {
                asiento.Estado = "Libre";
            }

            await _context.SaveChangesAsync();

            return true;
        }

        private static ReservaResponse MapearReserva(
            Domain.Entities.Reserva reserva)
        {
            return new ReservaResponse
            {
                IdReserva = reserva.IdReserva,
                IdPasajero = reserva.IdPasajero,
                IdVuelo = reserva.IdVuelo,
                IdAsiento = reserva.IdAsiento,
                IdUsuarioAgente = reserva.IdUsuarioAgente,
                CodigoReserva = reserva.CodigoReserva,
                TarifaBase = reserva.TarifaBase,
                EstadoReserva = reserva.EstadoReserva,
                FechaReserva = reserva.FechaReserva,
                FechaCancelacion = reserva.FechaCancelacion,

                Pasajero =
                    reserva.Pasajero == null
                        ? string.Empty
                        : $"{reserva.Pasajero.Apellido}, " +
                          $"{reserva.Pasajero.Nombre}",

                Vuelo =
                    reserva.Vuelo == null
                        ? string.Empty
                        : reserva.Vuelo.NumeroVuelo,

                Asiento =
                    reserva.Asiento == null
                        ? string.Empty
                        : $"{reserva.Asiento.Fila}" +
                          $"{reserva.Asiento.Letra}"
            };
        }

        private async Task<string> GenerarCodigoReservaAsync()
        {
            const string caracteres =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random();

            while (true)
            {
                var codigo =
                    new string(
                        Enumerable.Range(0, 6)
                            .Select(_ =>
                                caracteres[
                                    random.Next(
                                        caracteres.Length)])
                            .ToArray());

                var existe =
                    await _context.Reservas
                        .AnyAsync(r =>
                            r.CodigoReserva == codigo);

                if (!existe)
                    return codigo;
            }
        }
    }
}