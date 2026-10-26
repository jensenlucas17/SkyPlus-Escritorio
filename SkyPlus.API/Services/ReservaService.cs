using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.API.Exceptions;
using SkyPlus.Domain.Entities;
using SkyPlus.Infrastructure.Data;
using System.Data;
using System.Security.Cryptography;

namespace SkyPlus.API.Services
{
    public class ReservaService : IReservaService
    {
        private readonly SkyPlusDbContext _context;
        public ReservaService(SkyPlusDbContext context) => _context = context;

        public async Task<List<ReservaResponse>> ObtenerTodosAsync()
        {
            var reservas = await ConsultaReservas().ToListAsync();
            return reservas.Select(MapearReserva).ToList();
        }

        public async Task<ReservaResponse?> ObtenerPorIdAsync(int id)
        {
            var reserva = await ConsultaReservas().FirstOrDefaultAsync(r => r.IdReserva == id);
            return reserva == null ? null : MapearReserva(reserva);
        }

        public async Task<List<ReservaResponse>> ObtenerPorPasajeroAsync(int idPasajero)
        {
            var reservas = await ConsultaReservas().Where(r => r.IdPasajero == idPasajero).ToListAsync();
            return reservas.Select(MapearReserva).ToList();
        }

        private IQueryable<Reserva> ConsultaReservas() => _context.Reservas
            .AsNoTracking()
            .Include(r => r.Pasajero)
            .Include(r => r.Vuelo).ThenInclude(v => v!.LugarOrigen)
            .Include(r => r.Vuelo).ThenInclude(v => v!.LugarDestino)
            .Include(r => r.Asiento);

        public async Task<ReservaResponse?> CrearAsync(CrearReservaRequest request)
        {
            // Bloqueo serializable para evitar que dos solicitudes ocupen el mismo asiento.
            await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var pasajero = await _context.Pasajeros.FindAsync(request.IdPasajero);
            if (pasajero == null) throw new ValidacionNegocioException("El pasajero seleccionado no existe.");
            if (pasajero.Estado != "Activo") throw new ValidacionNegocioException($"El pasajero {pasajero.Nombre} {pasajero.Apellido} está inactivo.");

            var vuelo = await _context.Vuelos.FindAsync(request.IdVuelo);
            if (vuelo == null) throw new ValidacionNegocioException("El vuelo seleccionado no existe.");
            if (vuelo.EstadoVuelo != "Programado" && vuelo.EstadoVuelo != "Confirmado")
                throw new ValidacionNegocioException($"No se puede reservar un vuelo en estado {vuelo.EstadoVuelo}.");

            var asiento = await _context.Asientos.FindAsync(request.IdAsiento);
            if (asiento == null) throw new ValidacionNegocioException("El asiento seleccionado no existe.");
            if (asiento.IdVuelo != request.IdVuelo) throw new ValidacionNegocioException("El asiento pertenece a otro vuelo.");

            // El vencimiento se comprueba incluso si el servicio automático aún no corrió.
            var ahora = DateTime.UtcNow;
            var vencidas = await _context.Reservas
                .Where(r => r.IdAsiento == request.IdAsiento && r.EstadoReserva == "PendientePago" &&
                            r.FechaExpiracion != null && r.FechaExpiracion <= ahora)
                .ToListAsync();
            foreach (var vencida in vencidas) vencida.EstadoReserva = "Expirada";

            var ocupada = await _context.Reservas.AnyAsync(r =>
                r.IdAsiento == request.IdAsiento && r.IdVuelo == request.IdVuelo &&
                (r.EstadoReserva == "Confirmada" ||
                 (r.EstadoReserva == "PendientePago" && r.FechaExpiracion > ahora)));
            if (ocupada) throw new ValidacionNegocioException("El asiento ya tiene una reserva activa.");

            var agente = await _context.Usuarios.Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == request.IdUsuarioAgente);
            if (agente == null) throw new ValidacionNegocioException("El agente no existe.");
            if (agente.EstadoCuenta != "Activo") throw new ValidacionNegocioException("El agente está inactivo.");
            if (agente.Rol?.NombreRol != "Agente de Reservas")
                throw new ValidacionNegocioException("Solo un Agente de Reservas puede crear reservas.");

            var reserva = new Reserva
            {
                IdPasajero = request.IdPasajero,
                IdVuelo = request.IdVuelo,
                IdAsiento = request.IdAsiento,
                IdUsuarioAgente = request.IdUsuarioAgente,
                CodigoReserva = await GenerarCodigoReservaAsync(),
                TarifaBase = vuelo.Tarifa,
                EstadoReserva = "PendientePago",
                FechaReserva = ahora,
                FechaExpiracion = ahora.AddMinutes(5)
            };
            asiento.Estado = "Ocupado";
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return await ObtenerPorIdAsync(reserva.IdReserva);
        }

        public async Task<bool> CancelarAsync(int id)
        {
            await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var reserva = await _context.Reservas.FirstOrDefaultAsync(r => r.IdReserva == id);
            if (reserva == null) return false;
            if (reserva.EstadoReserva == "Cancelada") throw new ValidacionNegocioException("La reserva ya está cancelada.");
            if (reserva.EstadoReserva == "Expirada") throw new ValidacionNegocioException("La reserva ya expiró.");
            if (await _context.Pagos.AnyAsync(p => p.IdReserva == id && p.EstadoPago == "Registrado"))
                throw new ValidacionNegocioException("La reserva tiene un pago registrado. Gestioná primero el reembolso.");
            reserva.EstadoReserva = "Cancelada";
            reserva.FechaCancelacion = DateTime.UtcNow;
            var asiento = await _context.Asientos.FirstOrDefaultAsync(a => a.IdAsiento == reserva.IdAsiento);
            if (asiento != null) asiento.Estado = "Libre";
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
        }

        private static ReservaResponse MapearReserva(Reserva r) => new ReservaResponse
        {
            IdReserva = r.IdReserva,
            IdPasajero = r.IdPasajero,
            IdVuelo = r.IdVuelo,
            IdAsiento = r.IdAsiento,
            IdUsuarioAgente = r.IdUsuarioAgente,
            CodigoReserva = r.CodigoReserva,
            TarifaBase = r.TarifaBase,
            EstadoReserva = r.EstadoReserva,
            FechaReserva = r.FechaReserva,
            FechaExpiracion = r.FechaExpiracion,
            FechaCancelacion = r.FechaCancelacion,
            Pasajero = r.Pasajero == null ? "" : $"{r.Pasajero.Apellido}, {r.Pasajero.Nombre}",
            Vuelo = r.Vuelo?.NumeroVuelo ?? "",
            Asiento = r.Asiento == null ? "" : $"{r.Asiento.Fila}{r.Asiento.Letra}"
        };

        private async Task<string> GenerarCodigoReservaAsync()
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            while (true)
            {
                var codigo = new string(Enumerable.Range(0, 6)
                    .Select(_ => caracteres[RandomNumberGenerator.GetInt32(caracteres.Length)]).ToArray());
                if (!await _context.Reservas.AnyAsync(r => r.CodigoReserva == codigo)) return codigo;
            }
        }
    }
}