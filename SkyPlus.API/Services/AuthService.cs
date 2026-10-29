using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly SkyPlusDbContext _context;
        private readonly JwtTokenService _jwtTokenService;
        private readonly PasswordService _passwordService;

        public AuthService(
            SkyPlusDbContext context,
            JwtTokenService jwtTokenService,
            PasswordService passwordService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _passwordService = passwordService;
        }

        public async Task<LoginResponse?> LoginAsync(
            LoginRequest request)
        {
            var email = request.Email.Trim();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.EmailCorporativo == email);

            if (usuario == null ||
                usuario.EstadoCuenta != "Activo")
            {
                return null;
            }

            bool passwordValida =
                _passwordService.VerificarPassword(
                    request.Password,
                    usuario.ContrasenaHash);

            if (!passwordValida)
            {
                return null;
            }

            var rol = await _context.Roles
                .FirstOrDefaultAsync(r =>
                    r.IdRol == usuario.IdRol);

            if (rol == null)
            {
                return null;
            }

            var (token, expiracion) =
                _jwtTokenService.GenerarToken(
                    usuario,
                    rol.NombreRol);

            usuario.UltimaSesion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.EmailCorporativo,
                IdRol = usuario.IdRol,
                Rol = rol.NombreRol,
                Token = token,
                ExpiracionToken = expiracion
            };
        }
    }
}