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
            CrearReembolsoRequest request,
            int idUsuarioAutenticado)
        {
            if (request.IdPago <= 0)
                throw new ValidacionNegocioException(
                    "Seleccioná un pago válido.");

            if (idUsuarioAutenticado <= 0)
                throw new ValidacionNegocioException(
                    "El usuario autenticado no es válido.");

            if (string.IsNullOrWhiteSpace(request.Motivo))
                throw new ValidacionNegocioException(
                    "El motivo del reembolso es obligatorio.");

            var motivo = request.Motivo.Trim();

            if (motivo.Length > 500)
                throw new ValidacionNegocioException(
                    "El motivo no puede superar los 500 caracteres.");

            int idReembolso;

            await using (var tx = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable))
            {
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

                if (pago.TipoPago != "Inicial" &&
                    pago.TipoPago != "Adicional")
                    throw new ValidacionNegocioException(
                        "El tipo de pago no es válido.");

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
                        u.IdUsuario == idUsuarioAutenticado);

                if (agente == null)
                    throw new ValidacionNegocioException(
                        "El agente no existe.");

                if (agente.EstadoCuenta != "Activo")
                    throw new ValidacionNegocioException(
                        "El agente está inactivo.");

                if (agente.Rol?.NombreRol != "Agente de Reservas")
                    throw new ValidacionNegocioException(
                        "Solo un Agente de Reservas puede " +
                        "registrar reembolsos.");

                if (await _context.CheckIns.AnyAsync(
                    c => c.IdReserva == reserva.IdReserva))
                {
                    throw new ValidacionNegocioException(
                        "La reserva tiene un check-in asociado. " +
                        "Debe resolverse antes del reembolso.");
                }

                var ahora = DateTime.UtcNow;

                if (pago.TipoPago == "Inicial")
                {
                    // No cancelar el pasaje dejando pagos
                    // adicionales registrados sin resolver.
                    bool adicionalesVigentes =
                        await _context.Pagos.AnyAsync(p =>
                            p.IdReserva == reserva.IdReserva &&
                            p.TipoPago == "Adicional" &&
                            p.EstadoPago == "Registrado" &&
                            !_context.Reembolsos.Any(r =>
                                r.IdPago == p.IdPago));

                    if (adicionalesVigentes)
                        throw new ValidacionNegocioException(
                            "La reserva tiene pagos adicionales " +
                            "sin reembolsar. Resolvelos primero.");

                    var asiento = await _context.Asientos
                        .FirstOrDefaultAsync(a =>
                            a.IdAsiento == reserva.IdAsiento &&
                            a.IdVuelo == reserva.IdVuelo);

                    if (asiento == null)
                        throw new ValidacionNegocioException(
                            "No se encontró el asiento de la reserva.");

                    reserva.EstadoReserva = "Cancelada";
                    reserva.FechaCancelacion = ahora;
                    asiento.Estado = "Libre";

                    // Desactivar equipajes asociados a la reserva.
                    var equipajes = await _context.EquipajesReserva
                        .Where(e => e.IdReserva == reserva.IdReserva)
                        .ToListAsync();

                    foreach (var equipaje in equipajes)
                        equipaje.Estado = "Cancelado";

                    // Cancelar cargos que aún no se hayan pagado.
                    var cargosPendientes =
                        await _context.CargosAdicionales
                            .Where(c =>
                                c.IdReserva == reserva.IdReserva &&
                                c.Estado == "Pendiente")
                            .ToListAsync();

                    foreach (var cargo in cargosPendientes)
                        cargo.Estado = "Cancelado";
                }
                else
                {
                    // Reembolso adicional:
                    // solo se revierten los servicios cobrados
                    // en este pago. El pasaje permanece confirmado.
                    var detalles = await _context.PagosDetalle
                        .Where(d => d.IdPago == pago.IdPago)
                        .ToListAsync();

                    if (detalles.Count == 0)
                        throw new ValidacionNegocioException(
                            "El pago adicional no tiene detalles.");

                    var idsCargos = detalles
                        .Where(d => d.IdCargoAdicional.HasValue)
                        .Select(d => d.IdCargoAdicional!.Value)
                        .Distinct()
                        .ToList();

                    var cargos = await _context.CargosAdicionales
                        .Where(c => idsCargos.Contains(
                            c.IdCargoAdicional))
                        .ToListAsync();

                    if (cargos.Count != idsCargos.Count ||
                        cargos.Any(c =>
                            c.IdReserva != reserva.IdReserva ||
                            c.Estado != "Pagado"))
                    {
                        throw new ValidacionNegocioException(
                            "Los cargos del pago no son válidos " +
                            "para el reembolso.");
                    }

                    foreach (var cargo in cargos)
                    {
                        cargo.Estado = "Reembolsado";
                    }

                    var idsEquipajes = cargos
                        .Where(c =>
                            c.TipoCargo == "Equipaje" &&
                            c.IdEquipajeReserva.HasValue)
                        .Select(c => c.IdEquipajeReserva!.Value)
                        .Distinct()
                        .ToList();

                    var equipajes = await _context.EquipajesReserva
                        .Where(e => idsEquipajes.Contains(
                            e.IdEquipajeReserva))
                        .ToListAsync();

                    foreach (var equipaje in equipajes)
                    {
                        equipaje.Estado = "Cancelado";
                    }
                }

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

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                idReembolso = reembolso.IdReembolso;
            }

            return await ObtenerPorIdAsync(idReembolso)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar el reembolso registrado.");
        }
    }
}