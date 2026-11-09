using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyPlus.API.Services;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Controllers
{
    // Operaciones del usuario sobre su propia cuenta (cualquier rol con sesión iniciada).
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CuentaController : ControllerBase
    {
        private readonly SkyPlusDbContext _context;
        private readonly PasswordService _passwordService;

        public CuentaController(SkyPlusDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        public class CambiarPasswordRequest
        {
            public string PasswordActual { get; set; } = string.Empty;
            public string PasswordNueva { get; set; } = string.Empty;
        }

        // POST api/Cuenta/cambiar-password
        [HttpPost("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest request)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int idUsuario) || idUsuario <= 0)
                return Unauthorized(new { mensaje = "No se pudo identificar al usuario." });

            if (string.IsNullOrWhiteSpace(request.PasswordActual) || string.IsNullOrWhiteSpace(request.PasswordNueva))
                return BadRequest(new { mensaje = "Completá la contraseña actual y la nueva." });

            // Misma regla que al crear usuarios
            if (request.PasswordNueva.Length < 12)
                return BadRequest(new { mensaje = "La contraseña nueva debe tener al menos 12 caracteres." });

            if (!request.PasswordNueva.Any(char.IsUpper) || !request.PasswordNueva.Any(char.IsLower) || !request.PasswordNueva.Any(char.IsDigit))
                return BadRequest(new { mensaje = "La contraseña nueva debe tener mayúsculas, minúsculas y números." });

            if (request.PasswordNueva == request.PasswordActual)
                return BadRequest(new { mensaje = "La contraseña nueva debe ser distinta de la actual." });

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
            if (usuario == null || usuario.EstadoCuenta != "Activo")
                return Unauthorized(new { mensaje = "El usuario no existe o está inactivo." });

            if (!_passwordService.VerificarPassword(request.PasswordActual, usuario.ContrasenaHash))
                return BadRequest(new { mensaje = "La contraseña actual no es correcta." });

            usuario.ContrasenaHash = _passwordService.GenerarHash(request.PasswordNueva);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña actualizada correctamente." });
        }
    }
}
