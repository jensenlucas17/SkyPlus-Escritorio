
using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

            ValidarPasajero(
                request.Nombre,
                request.Apellido,
                request.Documento,
                request.Nacionalidad,
                request.Email,
                request.Telefono);

            var documentoExiste = await _context.Pasajeros
                .AnyAsync(p => p.Documento == request.Documento);

            if (documentoExiste)
            {
                throw new ValidacionPasajeroException(
                    "Ya existe un pasajero registrado con ese documento.");
            }

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

            return await ObtenerPorIdAsync(pasajero.IdPasajero);
        }

        public async Task<PasajeroResponse?> ActualizarAsync(
            int id,
            ActualizarPasajeroRequest request)
        {
            var pasajero = await _context.Pasajeros
                .FirstOrDefaultAsync(p => p.IdPasajero == id);

            if (pasajero == null)
                return null;

            request.Nombre = request.Nombre?.Trim() ?? string.Empty;
            request.Apellido = request.Apellido?.Trim() ?? string.Empty;
            request.Documento = request.Documento?.Trim() ?? string.Empty;
            request.Nacionalidad = request.Nacionalidad?.Trim() ?? string.Empty;
            request.Email = request.Email?.Trim() ?? string.Empty;
            request.Telefono = request.Telefono?.Trim() ?? string.Empty;

            ValidarPasajero(
                request.Nombre,
                request.Apellido,
                request.Documento,
                request.Nacionalidad,
                request.Email,
                request.Telefono);

            var documentoExiste = await _context.Pasajeros
                .AnyAsync(p =>
                    p.Documento == request.Documento &&
                    p.IdPasajero != id);

            if (documentoExiste)
            {
                throw new ValidacionPasajeroException(
                    "Ya existe otro pasajero registrado con ese documento.");
            }

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

        // VALIDACIONES CENTRALIZADAS

        private static void ValidarPasajero(
            string? nombre,
            string? apellido,
            string? documento,
            string? nacionalidad,
            string? email,
            string? telefono)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ValidacionPasajeroException(
                    "El nombre es obligatorio.");
            }

            if (!NombreValido(nombre))
            {
                throw new ValidacionPasajeroException(
                    "El nombre solo puede contener letras, espacios, apóstrofes y guiones.");
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                throw new ValidacionPasajeroException(
                    "El apellido es obligatorio.");
            }

            if (!NombreValido(apellido))
            {
                throw new ValidacionPasajeroException(
                    "El apellido solo puede contener letras, espacios, apóstrofes y guiones.");
            }

            if (string.IsNullOrWhiteSpace(documento))
            {
                throw new ValidacionPasajeroException(
                    "El documento es obligatorio.");
            }

            if (!DocumentoValido(documento))
            {
                throw new ValidacionPasajeroException(
                    "El documento debe tener entre 3 y 30 caracteres. Se permiten letras, números, espacios y guiones internos.");
            }

            if (string.IsNullOrWhiteSpace(nacionalidad))
            {
                throw new ValidacionPasajeroException(
                    "La nacionalidad es obligatoria.");
            }

            if (!NombreValido(nacionalidad))
            {
                throw new ValidacionPasajeroException(
                    "La nacionalidad solo puede contener letras, espacios, apóstrofes y guiones.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidacionPasajeroException(
                    "El email es obligatorio.");
            }

            if (!EmailValido(email))
            {
                throw new ValidacionPasajeroException(
                    "Ingresá un email válido. Ejemplo: pasajero@correo.com.");
            }

            if (!TelefonoValido(telefono))
            {
                throw new ValidacionPasajeroException(
                    "El teléfono debe contener entre 7 y 15 dígitos, con + opcional.");
            }
        }

        private static bool NombreValido(string? valor)
        {
            return !string.IsNullOrWhiteSpace(valor) &&
                Regex.IsMatch(
                    valor.Trim(),
                    @"^\p{L}+(?:[ '\-]\p{L}+)*$");
        }

        private static bool DocumentoValido(string? documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
                return false;

            documento = documento.Trim();

            return documento.Length >= 3 &&
                   documento.Length <= 30 &&
                   Regex.IsMatch(
                       documento,
                       @"^[A-Za-z0-9]+(?:[ -][A-Za-z0-9]+)*$");
        }

        private static bool EmailValido(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.Trim();

            if (!MailAddress.TryCreate(email, out var direccion))
                return false;

            var partes = email.Split('@');

            return partes.Length == 2 &&
                   direccion.Address == email &&
                   partes[1].Contains('.') &&
                   !partes[1].StartsWith('.') &&
                   !partes[1].EndsWith('.');
        }

        private static bool TelefonoValido(string? telefono)
        {
            // El teléfono es opcional
            if (string.IsNullOrWhiteSpace(telefono))
                return true;

            return Regex.IsMatch(
                telefono.Trim(),
                @"^\+?[0-9]{7,15}$");
        }
    }
}
