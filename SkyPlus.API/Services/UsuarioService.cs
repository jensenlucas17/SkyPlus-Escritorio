using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly SkyPlusDbContext _context;
        private readonly PasswordService _passwordService;

        public UsuarioService(
            SkyPlusDbContext context,
            PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        public async Task<List<UsuarioResponse>> ObtenerTodosAsync()
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .Select(u => new UsuarioResponse
                {
                    IdUsuario = u.IdUsuario,
                    IdRol = u.IdRol,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Dni = u.Dni,
                    EmailCorporativo = u.EmailCorporativo,
                    EstadoCuenta = u.EstadoCuenta,
                    UltimaSesion = u.UltimaSesion,
                    Rol = u.Rol.NombreRol
                })
                .ToListAsync();
        }

        public async Task<UsuarioResponse?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.IdUsuario == id)
                .Select(u => new UsuarioResponse
                {
                    IdUsuario = u.IdUsuario,
                    IdRol = u.IdRol,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Dni = u.Dni,
                    EmailCorporativo = u.EmailCorporativo,
                    EstadoCuenta = u.EstadoCuenta,
                    UltimaSesion = u.UltimaSesion,
                    Rol = u.Rol.NombreRol
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UsuarioResponse?> CrearAsync(CrearUsuarioRequest request)
        {
            if (!NombreValido(request.Nombre) ||
                !NombreValido(request.Apellido))
                {
                    return null;
                }

            var rolExiste = await _context.Roles
                .AnyAsync(r => r.IdRol == request.IdRol);

            if (!rolExiste)
                return null;

            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.EmailCorporativo == request.EmailCorporativo);

            if (emailExiste)
                return null;

            request.Dni = request.Dni?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(request.Dni) ||
                !System.Text.RegularExpressions.Regex.IsMatch(
                 request.Dni, @"^\d{7,8}$"))
                {
                    return null;
                }

                var dniExiste = await _context.Usuarios
                .AnyAsync(u => u.Dni == request.Dni);

             if (dniExiste)
                return null;

            if (string.IsNullOrWhiteSpace(request.Password) ||
                request.Password.Length < 12)
            {
                return null;
            }

            var usuario = new Domain.Entities.Usuario
            {
                IdRol = request.IdRol,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Dni = request.Dni,
                EmailCorporativo = request.EmailCorporativo,
                ContrasenaHash = _passwordService.GenerarHash(request.Password),
                EstadoCuenta = "Activo",
                UltimaSesion = null
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(usuario.IdUsuario);
        }

        public async Task<UsuarioResponse?> ActualizarAsync(
            int id,
            ActualizarUsuarioRequest request)
        {
            if (!NombreValido(request.Nombre) ||
                !NombreValido(request.Apellido))
                {
                    return null;
                }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
                return null;

            request.Dni = request.Dni?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(request.Dni) ||
                !System.Text.RegularExpressions.Regex.IsMatch(
                request.Dni, @"^\d{7,8}$"))
                {
                    return null;
                }

            var dniExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.Dni == request.Dni &&
                    u.IdUsuario != id);

            if (dniExiste)
                return null;

            var rolExiste = await _context.Roles
                .AnyAsync(r => r.IdRol == request.IdRol);

            if (!rolExiste)
                return null;

            var emailExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.EmailCorporativo == request.EmailCorporativo &&
                    u.IdUsuario != id);

            if (emailExiste)
                return null;

            usuario.IdRol = request.IdRol;
            usuario.Nombre = request.Nombre;
            usuario.Apellido = request.Apellido;
            usuario.Dni = request.Dni;
            usuario.EmailCorporativo = request.EmailCorporativo;
            usuario.EstadoCuenta = request.EstadoCuenta;

            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(id);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
                return false;

            usuario.EstadoCuenta = "Inactivo";

            await _context.SaveChangesAsync();

            return true;
        }

        private static bool NombreValido(string? valor)
        {
            return !string.IsNullOrWhiteSpace(valor) &&
                System.Text.RegularExpressions.Regex.IsMatch(
                    valor,
                    @"^\p{L}+(?:[ '\-]\p{L}+)*$");
        }
    }
}