using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class AsientoService : IAsientoService
    {
        private readonly SkyPlusDbContext _context;

        private static readonly string[] Letras =
        {
            "A",
            "B",
            "C",
            "D",
            "E",
            "F"
        };

        private static readonly string[] EstadosValidos =
        {
            "Libre",
            "Bloqueado temporalmente",
            "Ocupado"
        };

        public AsientoService(SkyPlusDbContext context)
        {
            _context = context;
        }

        public async Task<List<AsientoResponse>> ObtenerPorVueloAsync(
            int idVuelo)
        {
            return await _context.Asientos
                .Where(a => a.IdVuelo == idVuelo)
                .OrderBy(a => a.Fila)
                .ThenBy(a => a.Letra)
                .Select(a => new AsientoResponse
                {
                    IdAsiento = a.IdAsiento,
                    IdVuelo = a.IdVuelo,
                    Fila = a.Fila,
                    Letra = a.Letra,
                    Estado = a.Estado
                })
                .ToListAsync();
        }

        public async Task<AsientoResponse?> ObtenerPorIdAsync(
            int id)
        {
            return await _context.Asientos
                .Where(a => a.IdAsiento == id)
                .Select(a => new AsientoResponse
                {
                    IdAsiento = a.IdAsiento,
                    IdVuelo = a.IdVuelo,
                    Fila = a.Fila,
                    Letra = a.Letra,
                    Estado = a.Estado
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> GenerarAsientosAsync(
            int idVuelo)
        {
            var vueloExiste =
                await _context.Vuelos
                    .AnyAsync(v => v.IdVuelo == idVuelo);

            if (!vueloExiste)
                return false;

            var yaTieneAsientos =
                await _context.Asientos
                    .AnyAsync(a => a.IdVuelo == idVuelo);

            if (yaTieneAsientos)
                return false;

            var asientos =
                new List<Domain.Entities.Asiento>();

            for (int fila = 1; fila <= 30; fila++)
            {
                foreach (var letra in Letras)
                {
                    asientos.Add(
                        new Domain.Entities.Asiento
                        {
                            IdVuelo = idVuelo,
                            Fila = fila,
                            Letra = letra,
                            Estado = "Libre"
                        });
                }
            }

            _context.Asientos.AddRange(asientos);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<AsientoResponse?> ActualizarEstadoAsync(
            int id,
            ActualizarEstadoAsientoRequest request)
        {
            if (!EstadosValidos.Contains(request.Estado))
                return null;

            var asiento =
                await _context.Asientos
                    .FirstOrDefaultAsync(a =>
                        a.IdAsiento == id);

            if (asiento == null)
                return null;

            asiento.Estado = request.Estado;

            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(id);
        }
    }
}