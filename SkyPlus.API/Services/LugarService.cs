using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class LugarService : ILugarService
    {
        private readonly SkyPlusDbContext _context;

        public LugarService(SkyPlusDbContext context)
        {
            _context = context;
        }

        public async Task<List<LugarResponse>> ObtenerTodosAsync()
        {
            return await _context.Lugares
                .Select(l => new LugarResponse
                {
                    IdLugar = l.IdLugar,
                    CodigoIata = l.CodigoIata,
                    Nombre = l.Nombre,
                    Ciudad = l.Ciudad,
                    Pais = l.Pais
                })
                .ToListAsync();
        }

        public async Task<LugarResponse?> ObtenerPorIdAsync(int id)
        {
            return await _context.Lugares
                .Where(l => l.IdLugar == id)
                .Select(l => new LugarResponse
                {
                    IdLugar = l.IdLugar,
                    CodigoIata = l.CodigoIata,
                    Nombre = l.Nombre,
                    Ciudad = l.Ciudad,
                    Pais = l.Pais
                })
                .FirstOrDefaultAsync();
        }

        public async Task<LugarResponse?> CrearAsync(
            CrearLugarRequest request)
        {
            var codigoExiste = await _context.Lugares
                .AnyAsync(l => l.CodigoIata == request.CodigoIata);

            if (codigoExiste)
                return null;

            var lugar = new Domain.Entities.Lugar
            {
                CodigoIata = request.CodigoIata,
                Nombre = request.Nombre,
                Ciudad = request.Ciudad,
                Pais = request.Pais
            };

            _context.Lugares.Add(lugar);

            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(lugar.IdLugar);
        }

        public async Task<LugarResponse?> ActualizarAsync(
            int id,
            ActualizarLugarRequest request)
        {
            var lugar = await _context.Lugares
                .FirstOrDefaultAsync(l => l.IdLugar == id);

            if (lugar == null)
                return null;

            var codigoExiste = await _context.Lugares
                .AnyAsync(l =>
                    l.CodigoIata == request.CodigoIata &&
                    l.IdLugar != id);

            if (codigoExiste)
                return null;

            lugar.CodigoIata = request.CodigoIata;
            lugar.Nombre = request.Nombre;
            lugar.Ciudad = request.Ciudad;
            lugar.Pais = request.Pais;

            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(id);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var lugar = await _context.Lugares
                .FirstOrDefaultAsync(l => l.IdLugar == id);

            if (lugar == null)
                return false;

            _context.Lugares.Remove(lugar);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}