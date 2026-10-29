
using System.Data;
using Microsoft.EntityFrameworkCore;
using SkyPlus.API.Constants;
using SkyPlus.API.DTOs;
using SkyPlus.API.Exceptions;
using SkyPlus.Domain.Entities;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class EquipajeService : IEquipajeService
    {
        private readonly SkyPlusDbContext _context;

        public EquipajeService(SkyPlusDbContext context)
        {
            _context = context;
        }

        public async Task<List<TipoEquipajeResponse>> ObtenerTiposAsync()
        {
            return await _context.TiposEquipaje
                .AsNoTracking()
                .Where(t => t.Estado == "Activo")
                .Select(t => new TipoEquipajeResponse
                {
                    IdTipoEquipaje = t.IdTipoEquipaje,
                    Nombre = t.Nombre,
                    CostoAdicional = t.CostoAdicional,
                    LimiteKg = t.LimiteKg,
                    Descripcion = t.Descripcion
                })
                .ToListAsync();
        }

        public async Task<List<EquipajeReservaResponse>>
            ObtenerPorReservaAsync(int idReserva)
        {
            return await _context.EquipajesReserva
                .AsNoTracking()
                .Where(e => e.IdReserva == idReserva)
                .Select(e => new EquipajeReservaResponse
                {
                    IdEquipajeReserva = e.IdEquipajeReserva,
                    IdReserva = e.IdReserva,
                    IdTipoEquipaje = e.IdTipoEquipaje,
                    NombreTipoEquipaje = e.TipoEquipaje != null
                        ? e.TipoEquipaje.Nombre : "",
                    PesoKg = e.PesoKg,
                    LimiteKgContratado = e.LimiteKgContratado,
                    Costo = e.Costo,
                    Estado = e.Estado,
                    FechaContratacion = e.FechaContratacion
                })
                .ToListAsync();
        }

        public async Task<EquipajeReservaResponse> ContratarAsync(
            ContratarEquipajeRequest request)
        {
            await using var tx =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r =>
                    r.IdReserva == request.IdReserva);

            if (reserva == null)
                throw new InvalidOperationException(
                    "La reserva no existe.");

            if (reserva.EstadoReserva != "PendientePago" &&
                reserva.EstadoReserva != "Confirmada")
                throw new InvalidOperationException(
                    "La reserva no permite contratar equipaje.");

            if (reserva.EstadoReserva == "PendientePago" &&
                (!reserva.FechaExpiracion.HasValue ||
                 reserva.FechaExpiracion.Value <= DateTime.UtcNow))
                throw new InvalidOperationException(
                    "La reserva ha expirado.");

            var tipo = await _context.TiposEquipaje
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.IdTipoEquipaje == request.IdTipoEquipaje &&
                    t.Estado == "Activo");

            if (tipo == null)
                throw new InvalidOperationException(
                    "El tipo de equipaje no existe o está inactivo.");

            if (tipo.CostoAdicional < 0 || tipo.LimiteKg <= 0)
                throw new InvalidOperationException(
                    "La configuración del tipo de equipaje no es válida.");

            var equipaje = new EquipajeReserva
            {
                IdReserva = reserva.IdReserva,
                IdTipoEquipaje = tipo.IdTipoEquipaje,
                PesoKg = null,
                LimiteKgContratado = tipo.LimiteKg,
                Costo = tipo.CostoAdicional,
                Estado = "PendientePago",
                FechaContratacion = DateTime.UtcNow
            };

            _context.EquipajesReserva.Add(equipaje);
            await _context.SaveChangesAsync();

            var cargo = new CargoAdicional
            {
                IdReserva = reserva.IdReserva,
                IdEquipajeReserva = equipaje.IdEquipajeReserva,
                TipoCargo = "Equipaje",
                Descripcion = $"Equipaje: {tipo.Nombre}",
                Monto = equipaje.Costo,
                Estado = "Pendiente",
                FechaCreacion = DateTime.UtcNow
            };

            _context.CargosAdicionales.Add(cargo);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new EquipajeReservaResponse
            {
                IdEquipajeReserva = equipaje.IdEquipajeReserva,
                IdReserva = equipaje.IdReserva,
                IdTipoEquipaje = equipaje.IdTipoEquipaje,
                NombreTipoEquipaje = tipo.Nombre,
                PesoKg = equipaje.PesoKg,
                LimiteKgContratado = equipaje.LimiteKgContratado,
                Costo = equipaje.Costo,
                Estado = equipaje.Estado,
                FechaContratacion = equipaje.FechaContratacion
            };
        }

        public async Task<ResultadoPesoEquipajeResponse>
            RegistrarPesoAsync(RegistrarPesoEquipajeRequest request)
        {
            if (request.IdEquipajeReserva <= 0)
                throw new ValidacionNegocioException(
                    "Seleccioná un equipaje válido.");

            if (request.PesoKg <= 0)
                throw new ValidacionNegocioException(
                    "El peso debe ser mayor que cero.");

            if (decimal.Round(request.PesoKg, 2) != request.PesoKg)
            {
                throw new ValidacionNegocioException(
                    "El peso solo puede tener hasta dos decimales.");
            }

            if (request.IdUsuarioAgente <= 0)
            {
                throw new ValidacionNegocioException(
                    "El agente de Check-in es obligatorio.");
            }

            await using var tx =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);


            var agente = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == request.IdUsuarioAgente);
            if (agente == null)
            {
                throw new ValidacionNegocioException(
                    "El agente indicado no existe.");
            }

            if (agente.EstadoCuenta != "Activo")
            {
                throw new ValidacionNegocioException(
                    "El agente se encuentra inactivo.");
            }

            if (agente.Rol?.NombreRol != "Agente de Check-in")
            {
                throw new ValidacionNegocioException(
                    "Solo un Agente de Check-in puede registrar el peso del equipaje.");
            }

            var equipaje = await _context.EquipajesReserva
                .FirstOrDefaultAsync(e =>
                    e.IdEquipajeReserva == request.IdEquipajeReserva);

            if (equipaje == null)
                throw new ValidacionNegocioException(
                    "El equipaje no existe.");

            if (equipaje.Estado != "Activo")
                throw new ValidacionNegocioException(
                    "Solo se puede registrar el peso de equipaje activo y pagado.");

            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r =>
                    r.IdReserva == equipaje.IdReserva);

            if (reserva == null ||
                reserva.EstadoReserva != "Confirmada")
                throw new ValidacionNegocioException(
                    "La reserva debe estar confirmada.");

            if (await _context.CheckIns.AnyAsync(c =>
                c.IdReserva == reserva.IdReserva))
                throw new ValidacionNegocioException(
                    "No se puede modificar el peso después del check-in.");


            var cargosExceso = await _context.CargosAdicionales
                .Where(c =>
                    c.IdEquipajeReserva == equipaje.IdEquipajeReserva &&
                    c.TipoCargo == "ExcesoPeso")
                .ToListAsync();

            if (cargosExceso.Any(c =>
                c.Estado == "Pagado" ||
                c.Estado == "Reembolsado"))
            {
                throw new ValidacionNegocioException(
                    "El equipaje ya tiene un cargo por exceso procesado. " +
                    "No se permite modificar su peso.");
            }

            var cargosPendientes = cargosExceso
                .Where(c => c.Estado == "Pendiente")
                .ToList();

            if (cargosPendientes.Count > 1)
                throw new ValidacionNegocioException(
                    "Se detectaron varios cargos pendientes por exceso de peso.");

            decimal excesoKg = Math.Max(
                0m,
                request.PesoKg - equipaje.LimiteKgContratado);

            decimal montoExceso =
                excesoKg * TarifasEquipaje.PrecioPorKgExcedido;

            var cargoExistente = cargosPendientes.FirstOrDefault();

            if (montoExceso > 0)
            {
                if (cargoExistente == null)
                {
                    _context.CargosAdicionales.Add(new CargoAdicional
                    {
                        IdReserva = reserva.IdReserva,
                        IdEquipajeReserva = equipaje.IdEquipajeReserva,
                        TipoCargo = "ExcesoPeso",
                        Descripcion = $"Exceso de peso: {excesoKg:0.##} kg",
                        Monto = montoExceso,
                        Estado = "Pendiente",
                        FechaCreacion = DateTime.UtcNow
                    });
                }
                else
                {
                    cargoExistente.Monto = montoExceso;
                    cargoExistente.Descripcion =
                        $"Exceso de peso: {excesoKg:0.##} kg";
                }
            }
            else if (cargoExistente != null)
            {
                cargoExistente.Estado = "Cancelado";
            }


            equipaje.PesoKg = request.PesoKg;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new ResultadoPesoEquipajeResponse
            {
                IdEquipajeReserva = equipaje.IdEquipajeReserva,
                PesoKg = request.PesoKg,
                LimiteKgContratado = equipaje.LimiteKgContratado,
                ExcesoKg = excesoKg,
                MontoExceso = montoExceso,
                RequierePago = montoExceso > 0,
                Mensaje = montoExceso > 0
                    ? "Peso registrado. Debe abonarse el exceso antes del check-in."
                    : "Peso registrado correctamente. Sin exceso de peso."
            };
        }
    }
}
