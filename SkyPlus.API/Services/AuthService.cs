using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly SkyPlusDbContext _context;

        public AuthService(SkyPlusDbContext context)
        {
            _context = context;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.EmailCorporativo == request.Email);

            if (usuario == null)
            {
                return null;
            }

            if (usuario.EstadoCuenta != "Activo")
            {
                return null;
            }

            string passwordHash = GenerarHash(request.Password);

            if (usuario.ContrasenaHash != passwordHash)
            {
                return null;
            }

            var rol = await _context.Roles
                .FirstOrDefaultAsync(r => r.IdRol == usuario.IdRol);

            if (rol == null)
            {
                return null;
            }

            usuario.UltimaSesion = DateTime.Now;

            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.EmailCorporativo,
                IdRol = usuario.IdRol,
                Rol = rol.NombreRol
            };
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