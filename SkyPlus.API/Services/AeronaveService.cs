using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class AeronaveService : IAeronaveService
    {
        private readonly SkyPlusDbContext _context;

        public AeronaveService(SkyPlusDbContext context)
        {
            _context = context;
        }

        public async Task<List<AeronaveResponse>> ObtenerTodosAsync()
        {
            return await _context.Aeronaves
                .Select(a => new AeronaveResponse
                {
                    IdAeronave = a.IdAeronave,
                    Matricula = a.Matricula,
                    Modelo = a.Modelo,
                    Estado = a.Estado
                })
                .ToListAsync();
        }

        public async Task<AeronaveResponse?> ObtenerPorIdAsync(int id)
        {
            return await _context.Aeronaves
                .Where(a => a.IdAeronave == id)
                .Select(a => new AeronaveResponse
                {
                    IdAeronave = a.IdAeronave,
                    Matricula = a.Matricula,
                    Modelo = a.Modelo,
                    Estado = a.Estado
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AeronaveResponse?> CrearAsync(
            CrearAeronaveRequest request)
        {
            request.Matricula = request.Matricula?.Trim().ToUpperInvariant()
                ?? string.Empty;

            request.Modelo = request.Modelo?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(request.Matricula) ||
                string.IsNullOrWhiteSpace(request.Modelo))
            {
                return null;
            }
            var matriculaExiste = await _context.Aeronaves
                .AnyAsync(a => a.Matricula == request.Matricula);

            if (matriculaExiste)
                return null;

            var aeronave = new Domain.Entities.Aeronave
            {
                Matricula = request.Matricula,
                Modelo = request.Modelo
            };

            _context.Aeronaves.Add(aeronave);

            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(aeronave.IdAeronave);
        }

        public async Task<AeronaveResponse?> ActualizarAsync(
            int id,
            ActualizarAeronaveRequest request)
        {
            request.Matricula = request.Matricula?.Trim().ToUpperInvariant()
            ?? string.Empty;

            request.Modelo = request.Modelo?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(request.Matricula) ||
                string.IsNullOrWhiteSpace(request.Modelo))
            {
                return null;
            }
            var aeronave = await _context.Aeronaves
                .FirstOrDefaultAsync(a => a.IdAeronave == id);

            if (aeronave == null)
                return null;

            var matriculaExiste = await _context.Aeronaves
                .AnyAsync(a =>
                    a.Matricula == request.Matricula &&
                    a.IdAeronave != id);

            if (matriculaExiste)
                return null;

            aeronave.Matricula = request.Matricula;
            aeronave.Modelo = request.Modelo;

            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(id);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var aeronave = await _context.Aeronaves
                .FirstOrDefaultAsync(a => a.IdAeronave == id);

            if (aeronave == null)
                return false;

            if (aeronave.Estado == "Inactivo")
                return false;

            aeronave.Estado = "Inactivo";

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ReactivarAsync(int id)
        {
            var aeronave = await _context.Aeronaves
                .FirstOrDefaultAsync(a => a.IdAeronave == id);

            if (aeronave == null || aeronave.Estado == "Activo")
                return false;

            aeronave.Estado = "Activo";

            await _context.SaveChangesAsync();
            return true;
        }
    }
}