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
                    Pais = l.Pais,
                    Estado = l.Estado
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
                    Pais = l.Pais,
                    Estado = l.Estado
                })
                .FirstOrDefaultAsync();
        }

        public async Task<LugarResponse?> CrearAsync(
            CrearLugarRequest request)
        {
            var codigoExiste = await _context.Lugares
                .AnyAsync(l => l.CodigoIata == request.CodigoIata);

            request.CodigoIata = request.CodigoIata?.Trim().ToUpperInvariant()
                ?? string.Empty;

            request.Nombre = request.Nombre?.Trim() ?? string.Empty;
            request.Ciudad = request.Ciudad?.Trim() ?? string.Empty;
            request.Pais = request.Pais?.Trim() ?? string.Empty;

            if (!CodigoIataValido(request.CodigoIata) ||
                !NombreLugarValido(request.Nombre) ||
                !CiudadPaisValido(request.Ciudad) ||
                !CiudadPaisValido(request.Pais))
            {
                return null;
            }

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

            request.CodigoIata = request.CodigoIata?.Trim().ToUpperInvariant()
                ?? string.Empty;

            request.Nombre = request.Nombre?.Trim() ?? string.Empty;
            request.Ciudad = request.Ciudad?.Trim() ?? string.Empty;
            request.Pais = request.Pais?.Trim() ?? string.Empty;

            if (!CodigoIataValido(request.CodigoIata) ||
                !NombreLugarValido(request.Nombre) ||
                !CiudadPaisValido(request.Ciudad) ||
                !CiudadPaisValido(request.Pais))
            {
                return null;
            }

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

            // Evitar repetir la baja.
            if (lugar.Estado == "Inactivo")
                return false;

            // Baja lógica: conservar el registro.
            lugar.Estado = "Inactivo";

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ReactivarAsync(int id)
        {
            var lugar = await _context.Lugares
                .FirstOrDefaultAsync(l => l.IdLugar == id);

            if (lugar == null || lugar.Estado == "Activo")
                return false;

            lugar.Estado = "Activo";

            await _context.SaveChangesAsync();
            return true;
        }

        private static bool CodigoIataValido(string? codigo)
        {
            return !string.IsNullOrWhiteSpace(codigo) &&
                System.Text.RegularExpressions.Regex.IsMatch(
                    codigo, @"^[A-Z]{3}$");
        }

        private static bool CiudadPaisValido(string? valor)
        {
            return !string.IsNullOrWhiteSpace(valor) &&
                System.Text.RegularExpressions.Regex.IsMatch(
                    valor, @"^\p{L}+(?:[ '\-]\p{L}+)*$");
        }

        private static bool NombreLugarValido(string? nombre)
        {
            return !string.IsNullOrWhiteSpace(nombre) &&
                System.Text.RegularExpressions.Regex.IsMatch(
                    nombre, @"^[\p{L}\p{N}][\p{L}\p{N}\s.'()\-]*$");
        }

    }
}