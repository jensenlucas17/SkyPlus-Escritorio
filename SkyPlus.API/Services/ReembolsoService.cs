
using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.API.Exceptions;
using SkyPlus.Domain.Entities;
using SkyPlus.Infrastructure.Data;
using System.Data;

namespace SkyPlus.API.Services
{
    public class ReembolsoService : IReembolsoService
    {
        private readonly SkyPlusDbContext _context;

        public ReembolsoService(SkyPlusDbContext context)
        {
            _context = context;
        }

        private IQueryable<ReembolsoResponse> Consultar()
        {
            return _context.Reembolsos
                .AsNoTracking()
                .Select(r => new ReembolsoResponse
                {
                    IdReembolso = r.IdReembolso,
                    IdPago = r.IdPago,
                    IdReserva = r.Pago != null
                        ? r.Pago.IdReserva : 0,
                    CodigoReserva =
                        r.Pago != null && r.Pago.Reserva != null
                            ? r.Pago.Reserva.CodigoReserva : "",
                    Pasajero =
                        r.Pago != null &&
                        r.Pago.Reserva != null &&
                        r.Pago.Reserva.Pasajero != null
                            ? r.Pago.Reserva.Pasajero.Apellido + ", " +
                              r.Pago.Reserva.Pasajero.Nombre
                            : "",
                    Vuelo =
                        r.Pago != null &&
                        r.Pago.Reserva != null &&
                        r.Pago.Reserva.Vuelo != null
                            ? r.Pago.Reserva.Vuelo.NumeroVuelo : "",
                    IdUsuarioAgente = r.IdUsuarioAgente,
                    MontoReembolsado = r.MontoReembolsado,
                    Motivo = r.Motivo,
                    FechaReembolso = r.FechaReembolso,
                    EstadoReembolso = r.EstadoReembolso
                });
        }

        public Task<List<ReembolsoResponse>> ObtenerTodosAsync()
        {
            return Consultar()
                .OrderByDescending(r => r.FechaReembolso)
                .ToListAsync();
        }

        public Task<ReembolsoResponse?> ObtenerPorIdAsync(int id)
        {
            return Consultar()
                .FirstOrDefaultAsync(r => r.IdReembolso == id);
        }

        public async Task<ReembolsoResponse> CrearAsync(
            CrearReembolsoRequest request)
        {
            if (request.IdPago <= 0)
                throw new ValidacionNegocioException(
                    "Seleccioná un pago válido.");

            if (request.IdUsuarioAgente <= 0)
                throw new ValidacionNegocioException(
                    "El agente es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Motivo))
                throw new ValidacionNegocioException(
                    "El motivo del reembolso es obligatorio.");

            var motivo = request.Motivo.Trim();

            if (motivo.Length > 500)
                throw new ValidacionNegocioException(
                    "El motivo no puede superar los 500 caracteres.");

            await using var tx =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var pago = await _context.Pagos
                .FirstOrDefaultAsync(p =>
                    p.IdPago == request.IdPago);

            if (pago == null)
                throw new ValidacionNegocioException(
                    "El pago indicado no existe.");

            if (pago.EstadoPago != "Registrado")
                throw new ValidacionNegocioException(
                    "Solo se pueden reembolsar pagos registrados.");

            if (pago.Monto <= 0)
                throw new ValidacionNegocioException(
                    "El pago no tiene un monto válido.");

            if (await _context.Reembolsos.AnyAsync(
                r => r.IdPago == pago.IdPago))
            {
                throw new ValidacionNegocioException(
                    "El pago ya tiene un reembolso asociado.");
            }

            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r =>
                    r.IdReserva == pago.IdReserva);

            if (reserva == null)
                throw new ValidacionNegocioException(
                    "La reserva asociada no existe.");

            if (reserva.EstadoReserva != "Confirmada")
                throw new ValidacionNegocioException(
                    "Solo se pueden reembolsar reservas confirmadas.");

            var agente = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == request.IdUsuarioAgente);

            if (agente == null)
                throw new ValidacionNegocioException(
                    "El agente no existe.");

            if (agente.EstadoCuenta != "Activo")
                throw new ValidacionNegocioException(
                    "El agente está inactivo.");

            if (agente.Rol?.NombreRol != "Agente de Reservas")
                throw new ValidacionNegocioException(
                    "Solo un Agente de Reservas puede registrar reembolsos.");

            // No cancelar reservas que ya tengan check-in.
            if (await _context.CheckIns.AnyAsync(
                c => c.IdReserva == reserva.IdReserva))
            {
                throw new ValidacionNegocioException(
                    "La reserva tiene un check-in asociado. " +
                    "Debe resolverse antes de realizar el reembolso.");
            }

            var asiento = await _context.Asientos
                .FirstOrDefaultAsync(a =>
                    a.IdAsiento == reserva.IdAsiento &&
                    a.IdVuelo == reserva.IdVuelo);

            if (asiento == null)
                throw new ValidacionNegocioException(
                    "No se encontró el asiento de la reserva.");

            var ahora = DateTime.UtcNow;

            var reembolso = new Reembolso
            {
                IdPago = pago.IdPago,
                IdUsuarioAgente = agente.IdUsuario,
                MontoReembolsado = pago.Monto,
                Motivo = motivo,
                FechaReembolso = ahora,
                EstadoReembolso = "Registrado"
            };

            _context.Reembolsos.Add(reembolso);

            reserva.EstadoReserva = "Cancelada";
            reserva.FechaCancelacion = ahora;

            asiento.Estado = "Libre";

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return await ObtenerPorIdAsync(reembolso.IdReembolso)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar el reembolso registrado.");
        }
    }
}
