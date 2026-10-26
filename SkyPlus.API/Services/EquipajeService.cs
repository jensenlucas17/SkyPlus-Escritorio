
using System.Data;
using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
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
    }
}
