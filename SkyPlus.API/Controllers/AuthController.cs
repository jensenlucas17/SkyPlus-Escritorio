using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Services;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    mensaje = "El email y la contraseña son obligatorios."
                });
            }

            var resultado = await _authService.LoginAsync(request);

            if (resultado == null)
            {
                return Unauthorized(new
                {
                    mensaje = "Credenciales incorrectas o usuario inactivo."
                });
            }

            return Ok(resultado);
        }
    }
}