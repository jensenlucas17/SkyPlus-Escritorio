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
                    Modelo = a.Modelo
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
                    Modelo = a.Modelo
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AeronaveResponse?> CrearAsync(
            CrearAeronaveRequest request)
        {
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

            _context.Aeronaves.Remove(aeronave);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}