
using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class AutorizacionRestauracionService
    {
        private readonly SkyPlusDbContext _context;
        private readonly PasswordService _passwordService;

        public AutorizacionRestauracionService(
            SkyPlusDbContext context,
            PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        public async Task<int?> ValidarSegundoAdministradorAsync(
            int idAdministradorSolicitante,
            RestaurarBackupRequest request)
        {
            if (!request.ConfirmarRestauracion)
                return null;

            if (string.IsNullOrWhiteSpace(
                request.EmailSegundoAdministrador) ||
                string.IsNullOrWhiteSpace(
                    request.PasswordSegundoAdministrador))
            {
                return null;
            }

            string email = request.EmailSegundoAdministrador.Trim();

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    u.EmailCorporativo == email);

            if (usuario == null ||
                usuario.IdUsuario == idAdministradorSolicitante ||
                usuario.EstadoCuenta != "Activo" ||
                usuario.Rol?.NombreRol != "Administrador")
            {
                return null;
            }

            bool passwordValida =
                _passwordService.VerificarPassword(
                    request.PasswordSegundoAdministrador,
                    usuario.ContrasenaHash);

            return passwordValida
                ? usuario.IdUsuario
                : null;
        }
    }
}
