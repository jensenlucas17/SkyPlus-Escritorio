using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly SkyPlusDbContext _context;

        public UsuarioService(SkyPlusDbContext context)
        {
            _context = context;
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
                    EmailCorporativo = u.EmailCorporativo,
                    EstadoCuenta = u.EstadoCuenta,
                    UltimaSesion = u.UltimaSesion,
                    Rol = u.Rol.NombreRol
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UsuarioResponse?> CrearAsync(CrearUsuarioRequest request)
        {
            var rolExiste = await _context.Roles
                .AnyAsync(r => r.IdRol == request.IdRol);

            if (!rolExiste)
                return null;

            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.EmailCorporativo == request.EmailCorporativo);

            if (emailExiste)
                return null;

            var usuario = new Domain.Entities.Usuario
            {
                IdRol = request.IdRol,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                EmailCorporativo = request.EmailCorporativo,
                ContrasenaHash = GenerarHash(request.Password),
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
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
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

        private static string GenerarHash(string password)
        {
            using SHA256 sha256 = SHA256.Create();

            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(bytes);

            return Convert.ToHexString(hash).ToLower();
        }
    }
}