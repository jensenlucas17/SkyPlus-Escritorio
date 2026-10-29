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
                    Telefono = p.Telefono,
                    Estado = p.Estado
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
                    Telefono = p.Telefono,
                    Estado = p.Estado
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PasajeroResponse?> CrearAsync(
            CrearPasajeroRequest request)
        {
            request.Nombre = request.Nombre?.Trim() ?? string.Empty;
            request.Apellido = request.Apellido?.Trim() ?? string.Empty;
            request.Documento = request.Documento?.Trim() ?? string.Empty;
            request.Nacionalidad = request.Nacionalidad?.Trim() ?? string.Empty;
            request.Email = request.Email?.Trim() ?? string.Empty;
            request.Telefono = request.Telefono?.Trim() ?? string.Empty;

            if (!NombreValido(request.Nombre) ||
                !NombreValido(request.Apellido) ||
                !DocumentoValido(request.Documento) ||
                !NombreValido(request.Nacionalidad) ||
                !EmailValido(request.Email) ||
                !TelefonoValido(request.Telefono))
            {
                return null;
            }

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

            request.Nombre = request.Nombre?.Trim() ?? string.Empty;
            request.Apellido = request.Apellido?.Trim() ?? string.Empty;
            request.Documento = request.Documento?.Trim() ?? string.Empty;
            request.Nacionalidad = request.Nacionalidad?.Trim() ?? string.Empty;
            request.Email = request.Email?.Trim() ?? string.Empty;
            request.Telefono = request.Telefono?.Trim() ?? string.Empty;

            if (!NombreValido(request.Nombre) ||
                !NombreValido(request.Apellido) ||
                !DocumentoValido(request.Documento) ||
                !NombreValido(request.Nacionalidad) ||
                !EmailValido(request.Email) ||
                !TelefonoValido(request.Telefono))
            {
                return null;
            }

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
            var pasajero = await _context.Pasajeros
                .FirstOrDefaultAsync(p => p.IdPasajero == id);

            if (pasajero == null)
                return false;

            if (pasajero.Estado == "Inactivo")
                return false;

            pasajero.Estado = "Inactivo";

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ReactivarAsync(int id)
        {
            var pasajero = await _context.Pasajeros
                .FirstOrDefaultAsync(p => p.IdPasajero == id);

            if (pasajero == null || pasajero.Estado == "Activo")
                return false;

            pasajero.Estado = "Activo";

            await _context.SaveChangesAsync();
            return true;
        }

        private static bool NombreValido(string? valor)
        {
            return !string.IsNullOrWhiteSpace(valor) &&
                System.Text.RegularExpressions.Regex.IsMatch(
                    valor, @"^\p{L}+(?:[ '\-]\p{L}+)*$");
        }

        private static bool DocumentoValido(string? documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
                return false;

            documento = documento.Trim();

            return documento.Length >= 3 &&
                   documento.Length <= 30 &&
                   System.Text.RegularExpressions.Regex.IsMatch(
                       documento,
                       @"^[A-Za-z0-9]+(?:[ -][A-Za-z0-9]+)*$");
        }

        private static bool EmailValido(string? email)
        {
            return !string.IsNullOrWhiteSpace(email) &&
                System.Net.Mail.MailAddress.TryCreate(
                    email, out var direccion) &&
                direccion.Address == email;
        }

        private static bool TelefonoValido(string? telefono)
        {
            return !string.IsNullOrWhiteSpace(telefono) &&
                System.Text.RegularExpressions.Regex.IsMatch(
                    telefono, @"^\+?[0-9]{7,15}$");
        }
    }
}