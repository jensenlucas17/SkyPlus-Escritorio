using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class PasajeroService : IPasajeroService
    {
        private readonly SkyPlusDbContext _context;

        public PasajeroService(SkyPlusDbContext context)
        {
            _context = context;
        }

        public async Task<List<PasajeroResponse>> ObtenerTodosAsync()
        {
            return await _context.Pasajeros
                .Select(p => new PasajeroResponse
                {
                    IdPasajero = p.IdPasajero,
                    Nombre = p.Nombre,
                    Apellido = p.Apellido,
                    Documento = p.Documento,
                    Nacionalidad = p.Nacionalidad,
                    Email = p.Email,
                    Telefono = p.Telefono
                })
                .ToListAsync();
        }

        public async Task<PasajeroResponse?> ObtenerPorIdAsync(int id)
        {
            return await _context.Pasajeros
                .Where(p => p.IdPasajero == id)
                .Select(p => new PasajeroResponse
                {
                    IdPasajero = p.IdPasajero,
                    Nombre = p.Nombre,
                    Apellido = p.Apellido,
                    Documento = p.Documento,
                    Nacionalidad = p.Nacionalidad,
                    Email = p.Email,
                    Telefono = p.Telefono
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PasajeroResponse?> CrearAsync(
            CrearPasajeroRequest request)
        {
            var documentoExiste =
                await _context.Pasajeros
                    .AnyAsync(p =>
                        p.Documento == request.Documento);

            if (documentoExiste)
                return null;

            var pasajero = new Domain.Entities.Pasajero
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Documento = request.Documento,
                Nacionalidad = request.Nacionalidad,
                Email = request.Email,
                Telefono = request.Telefono
            };

            _context.Pasajeros.Add(pasajero);

            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(
                pasajero.IdPasajero);
        }

        public async Task<PasajeroResponse?> ActualizarAsync(
            int id,
            ActualizarPasajeroRequest request)
        {
            var pasajero =
                await _context.Pasajeros
                    .FirstOrDefaultAsync(p =>
                        p.IdPasajero == id);

            if (pasajero == null)
                return null;

            var documentoExiste =
                await _context.Pasajeros
                    .AnyAsync(p =>
                        p.Documento == request.Documento &&
                        p.IdPasajero != id);

            if (documentoExiste)
                return null;

            pasajero.Nombre = request.Nombre;
            pasajero.Apellido = request.Apellido;
            pasajero.Documento = request.Documento;
            pasajero.Nacionalidad = request.Nacionalidad;
            pasajero.Email = request.Email;
            pasajero.Telefono = request.Telefono;

            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(id);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var pasajero =
                await _context.Pasajeros
                    .FirstOrDefaultAsync(p =>
                        p.IdPasajero == id);

            if (pasajero == null)
                return false;

            _context.Pasajeros.Remove(pasajero);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}