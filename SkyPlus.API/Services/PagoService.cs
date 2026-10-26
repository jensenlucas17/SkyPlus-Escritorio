
using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.API.Exceptions;
using SkyPlus.Domain.Entities;
using SkyPlus.Infrastructure.Data;
using System.Data;

namespace SkyPlus.API.Services
{
    public class PagoService : IPagoService
    {
        private readonly SkyPlusDbContext _context;

        public PagoService(SkyPlusDbContext context)
        {
            _context = context;
        }

        private IQueryable<PagoResponse> Consultar() =>
            _context.Pagos.AsNoTracking()
                .Select(p => new PagoResponse
                {
                    IdPago = p.IdPago,
                    IdReserva = p.IdReserva,
                    CodigoReserva = p.Reserva != null
                        ? p.Reserva.CodigoReserva : "",
                    IdUsuarioAgente = p.IdUsuarioAgente,
                    Monto = p.Monto,
                    MetodoPago = p.MetodoPago,
                    FechaPago = p.FechaPago,
                    EstadoPago = p.EstadoPago,
                    TipoPago = p.TipoPago
                });

        public Task<List<PagoResponse>> ObtenerTodosAsync() =>
            Consultar()
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

        public Task<PagoResponse?> ObtenerPorIdAsync(int id) =>
            Consultar().FirstOrDefaultAsync(p => p.IdPago == id);

        private static void ValidarRequest(CrearPagoRequest request)
        {
            if (request.IdReserva <= 0)
                throw new ValidacionNegocioException(
                    "Seleccioná una reserva válida.");

            if (request.IdUsuarioAgente <= 0)
                throw new ValidacionNegocioException(
                    "El agente es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.MetodoPago))
                throw new ValidacionNegocioException(
                    "El método de pago es obligatorio.");
        }

        private async Task<Usuario> ValidarAgenteAsync(int idUsuario)
        {
            var agente = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (agente == null)
                throw new ValidacionNegocioException(
                    "El agente no existe.");

            if (agente.EstadoCuenta != "Activo")
                throw new ValidacionNegocioException(
                    "El agente está inactivo.");

            if (agente.Rol?.NombreRol != "Agente de Reservas")
                throw new ValidacionNegocioException(
                    "Solo los Agentes de Reservas pueden registrar pagos.");

            return agente;
        }

        // PAGO INICIAL: PASAJE + EQUIPAJE INICIAL
        public async Task<PagoResponse?> CrearAsync(
            CrearPagoRequest request)
        {
            ValidarRequest(request);

            int idPago;

            await using (var tx = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable))
            {
                var reserva = await _context.Reservas
                    .FirstOrDefaultAsync(r =>
                        r.IdReserva == request.IdReserva);

                if (reserva == null)
                    throw new ValidacionNegocioException(
                        "La reserva indicada no existe.");

                if (reserva.EstadoReserva != "PendientePago")
                    throw new ValidacionNegocioException(
                        "Solo se pueden pagar reservas pendientes de pago.");

                if (reserva.FechaExpiracion == null ||
                    reserva.FechaExpiracion <= DateTime.UtcNow)
                    throw new ValidacionNegocioException(
                        "La reserva venció. Debés crear una nueva reserva.");

                if (reserva.TarifaBase <= 0)
                    throw new ValidacionNegocioException(
                        "La reserva no tiene una tarifa válida.");

                if (await _context.Pagos.AnyAsync(p =>
                    p.IdReserva == reserva.IdReserva &&
                    p.TipoPago == "Inicial"))
                {
                    throw new ValidacionNegocioException(
                        "La reserva ya tiene un pago inicial registrado.");
                }

                var agente = await ValidarAgenteAsync(
                    request.IdUsuarioAgente);

                var cargos = await _context.CargosAdicionales
                    .Where(c =>
                        c.IdReserva == reserva.IdReserva &&
                        c.Estado == "Pendiente")
                    .ToListAsync();

                if (cargos.Any(c => c.Monto < 0))
                    throw new ValidacionNegocioException(
                        "Existen cargos con importes inválidos.");

                var total = reserva.TarifaBase +
                    cargos.Sum(c => c.Monto);

                var pago = new Pago
                {
                    IdReserva = reserva.IdReserva,
                    IdUsuarioAgente = agente.IdUsuario,
                    Monto = total,
                    MetodoPago = request.MetodoPago.Trim(),
                    FechaPago = DateTime.UtcNow,
                    EstadoPago = "Registrado",
                    TipoPago = "Inicial"
                };

                pago.Detalles.Add(new PagoDetalle
                {
                    Concepto = "Pasaje",
                    Monto = reserva.TarifaBase
                });

                foreach (var cargo in cargos)
                {
                    pago.Detalles.Add(new PagoDetalle
                    {
                        IdCargoAdicional = cargo.IdCargoAdicional,
                        Concepto = "CargoAdicional",
                        Monto = cargo.Monto
                    });

                    cargo.Estado = "Pagado";
                }

                await ActivarEquipajesAsync(cargos);

                reserva.EstadoReserva = "Confirmada";

                _context.Pagos.Add(pago);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                idPago = pago.IdPago;
            }

            return await ObtenerPorIdAsync(idPago);
        }

        // PAGO ADICIONAL: EQUIPAJE Y OTROS CARGOS
        public async Task<PagoResponse?> CrearAdicionalAsync(
            CrearPagoRequest request)
        {
            ValidarRequest(request);

            int idPago;

            await using (var tx = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable))
            {
                var reserva = await _context.Reservas
                    .FirstOrDefaultAsync(r =>
                        r.IdReserva == request.IdReserva);

                if (reserva == null)
                    throw new ValidacionNegocioException(
                        "La reserva indicada no existe.");

                if (reserva.EstadoReserva != "Confirmada")
                    throw new ValidacionNegocioException(
                        "Solo se pueden registrar pagos adicionales " +
                        "en reservas confirmadas.");

                var agente = await ValidarAgenteAsync(
                    request.IdUsuarioAgente);

                var cargos = await _context.CargosAdicionales
                    .Where(c =>
                        c.IdReserva == reserva.IdReserva &&
                        c.Estado == "Pendiente")
                    .ToListAsync();

                if (cargos.Count == 0)
                    throw new ValidacionNegocioException(
                        "La reserva no tiene cargos pendientes.");

                if (cargos.Any(c => c.Monto < 0))
                    throw new ValidacionNegocioException(
                        "Existen cargos con importes inválidos.");

                var total = cargos.Sum(c => c.Monto);

                var pago = new Pago
                {
                    IdReserva = reserva.IdReserva,
                    IdUsuarioAgente = agente.IdUsuario,
                    Monto = total,
                    MetodoPago = request.MetodoPago.Trim(),
                    FechaPago = DateTime.UtcNow,
                    EstadoPago = "Registrado",
                    TipoPago = "Adicional"
                };

                foreach (var cargo in cargos)
                {
                    pago.Detalles.Add(new PagoDetalle
                    {
                        IdCargoAdicional = cargo.IdCargoAdicional,
                        Concepto = "CargoAdicional",
                        Monto = cargo.Monto
                    });

                    cargo.Estado = "Pagado";
                }

                await ActivarEquipajesAsync(cargos);

                _context.Pagos.Add(pago);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                idPago = pago.IdPago;
            }

            return await ObtenerPorIdAsync(idPago);
        }

        private async Task ActivarEquipajesAsync(
            List<CargoAdicional> cargos)
        {
            var ids = cargos
                .Where(c =>
                    c.TipoCargo == "Equipaje" &&
                    c.IdEquipajeReserva.HasValue)
                .Select(c => c.IdEquipajeReserva!.Value)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return;

            var equipajes = await _context.EquipajesReserva
                .Where(e => ids.Contains(e.IdEquipajeReserva))
                .ToListAsync();

            foreach (var equipaje in equipajes)
            {
                equipaje.Estado = "Activo";
            }
        }

        public async Task<bool> AnularAsync(int id)
        {
            await using var tx = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var pago = await _context.Pagos
                .FirstOrDefaultAsync(p => p.IdPago == id);

            if (pago == null)
                throw new ValidacionNegocioException(
                    "El pago no existe.");

            if (pago.EstadoPago == "Anulado")
                throw new ValidacionNegocioException(
                    "El pago ya está anulado.");

            if (pago.EstadoPago != "Registrado")
                throw new ValidacionNegocioException(
                    "Solo se pueden anular pagos registrados.");

            if (await _context.Reembolsos
                .AnyAsync(r => r.IdPago == id))
            {
                throw new ValidacionNegocioException(
                    "El pago tiene un reembolso asociado.");
            }

            if (await _context.Reservas.AnyAsync(r =>
                r.IdReserva == pago.IdReserva &&
                r.EstadoReserva == "Confirmada"))
            {
                throw new ValidacionNegocioException(
                    "No se puede anular el pago de una reserva " +
                    "confirmada. Gestioná la cancelación y el reembolso.");
            }

            pago.EstadoPago = "Anulado";

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return true;
        }
    }
}
