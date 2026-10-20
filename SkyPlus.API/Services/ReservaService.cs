using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.API.Exceptions;
using SkyPlus.Infrastructure.Data;
using System.Security.Cryptography;

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
            var reservas = await ConsultaReservas().ToListAsync();
            return reservas.Select(MapearReserva).ToList();
        }

        public async Task<ReservaResponse?> ObtenerPorIdAsync(int id)
        {
            var reserva = await ConsultaReservas()
                .FirstOrDefaultAsync(r => r.IdReserva == id);
            return reserva == null ? null : MapearReserva(reserva);
        }

        public async Task<List<ReservaResponse>> ObtenerPorPasajeroAsync(int idPasajero)
        {
            var reservas = await ConsultaReservas()
                .Where(r => r.IdPasajero == idPasajero)
                .ToListAsync();
            return reservas.Select(MapearReserva).ToList();
        }

        private IQueryable<Domain.Entities.Reserva> ConsultaReservas()
        {
            return _context.Reservas
                .Include(r => r.Pasajero)
                .Include(r => r.Vuelo).ThenInclude(v => v!.LugarOrigen)
                .Include(r => r.Vuelo).ThenInclude(v => v!.LugarDestino)
                .Include(r => r.Asiento);
        }

        public async Task<ReservaResponse?> CrearAsync(CrearReservaRequest request)
        {
            var pasajero = await _context.Pasajeros.FindAsync(request.IdPasajero);
            if (pasajero == null)
                throw new ValidacionNegocioException("El pasajero seleccionado no existe.");
            if (pasajero.Estado != "Activo")
                throw new ValidacionNegocioException($"El pasajero {pasajero.Nombre} {pasajero.Apellido} está inactivo. Debe reactivarlo antes de reservar.");

            var vuelo = await _context.Vuelos.FindAsync(request.IdVuelo);
            if (vuelo == null)
                throw new ValidacionNegocioException("El vuelo seleccionado no existe.");
            if (vuelo.EstadoVuelo == "Cancelado")
                throw new ValidacionNegocioException("No se puede reservar: el vuelo está cancelado.");
            if (vuelo.EstadoVuelo == "Finalizado")
                throw new ValidacionNegocioException("No se puede reservar: el vuelo ya finalizó.");
            if (vuelo.EstadoVuelo != "Programado" && vuelo.EstadoVuelo != "Confirmado")
                throw new ValidacionNegocioException($"No se puede reservar un vuelo en estado {vuelo.EstadoVuelo}.");

            var asiento = await _context.Asientos.FindAsync(request.IdAsiento);
            if (asiento == null)
                throw new ValidacionNegocioException("El asiento seleccionado no existe.");
            if (asiento.IdVuelo != request.IdVuelo)
                throw new ValidacionNegocioException("El asiento seleccionado pertenece a otro vuelo.");
            if (asiento.Estado != "Libre")
                throw new ValidacionNegocioException($"El asiento {asiento.Fila}{asiento.Letra} no está libre (estado: {asiento.Estado}).");

            var asientoReservado = await _context.Reservas.AnyAsync(r =>
                r.IdAsiento == request.IdAsiento &&
                r.IdVuelo == request.IdVuelo &&
                r.EstadoReserva != "Cancelada");
            if (asientoReservado)
                throw new ValidacionNegocioException("El asiento ya tiene una reserva activa.");

            var agente = await _context.Usuarios.FindAsync(request.IdUsuarioAgente);
            if (agente == null)
                throw new ValidacionNegocioException("El agente de reservas seleccionado no existe.");
            if (agente.EstadoCuenta != "Activo")
                throw new ValidacionNegocioException("El agente de reservas está inactivo.");

            var codigo = await GenerarCodigoReservaAsync();
            var reserva = new Domain.Entities.Reserva
            {
                IdPasajero = request.IdPasajero,
                IdVuelo = request.IdVuelo,
                IdAsiento = request.IdAsiento,
                IdUsuarioAgente = request.IdUsuarioAgente,
                CodigoReserva = codigo,
                TarifaBase = vuelo.Tarifa,
                EstadoReserva = "Confirmada",
                FechaReserva = DateTime.Now
            };

            asiento.Estado = "Ocupado";
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();
            return await ObtenerPorIdAsync(reserva.IdReserva);
        }

        public async Task<bool> CancelarAsync(int id)
        {
            var reserva = await _context.Reservas.FirstOrDefaultAsync(r => r.IdReserva == id);
            if (reserva == null)
                return false;
            if (reserva.EstadoReserva == "Cancelada")
                throw new ValidacionNegocioException("La reserva ya está cancelada.");

            reserva.EstadoReserva = "Cancelada";
            reserva.FechaCancelacion = DateTime.Now;
            var asiento = await _context.Asientos.FirstOrDefaultAsync(a => a.IdAsiento == reserva.IdAsiento);
            if (asiento != null)
                asiento.Estado = "Libre";
            await _context.SaveChangesAsync();
            return true;
        }

        private static ReservaResponse MapearReserva(Domain.Entities.Reserva reserva)
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
                Pasajero = reserva.Pasajero == null ? string.Empty :
                    $"{reserva.Pasajero.Apellido}, {reserva.Pasajero.Nombre}",
                Vuelo = reserva.Vuelo == null ? string.Empty : reserva.Vuelo.NumeroVuelo,
                Asiento = reserva.Asiento == null ? string.Empty :
                    $"{reserva.Asiento.Fila}{reserva.Asiento.Letra}"
            };
        }

        private async Task<string> GenerarCodigoReservaAsync()
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            while (true)
            {
                var codigo = new string(Enumerable.Range(0, 6)
                    .Select(_ => caracteres[RandomNumberGenerator.GetInt32(caracteres.Length)])
                    .ToArray());
                if (!await _context.Reservas.AnyAsync(r => r.CodigoReserva == codigo))
                    return codigo;
            }
        }
    }
}
