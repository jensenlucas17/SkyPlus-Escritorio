using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();

            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);

            if (usuario == null)
                return NotFound(new
                {
                    mensaje = "Usuario no encontrado."
                });

            return Ok(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CrearUsuarioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellido) ||
                string.IsNullOrWhiteSpace(request.EmailCorporativo) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    mensaje = "Todos los campos obligatorios deben estar completos."
                });
            }

            var usuario = await _usuarioService.CrearAsync(request);

            if (usuario == null)
            {
                return BadRequest(new
                {
                    mensaje = "No se pudo crear el usuario. Verificá el rol, nombre, apellido, DNI, email y contraseña (mínimo 12 caracteres)."
                });
            }

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = usuario.IdUsuario },
                usuario);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ActualizarUsuarioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellido) ||
                string.IsNullOrWhiteSpace(request.EmailCorporativo))
            {
                return BadRequest(new
                {
                    mensaje = "Los campos obligatorios deben estar completos."
                });
            }

            var idUsuarioClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(idUsuarioClaim, out int idAutenticado))
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario autenticado."
                });
            }

            if (id == idAutenticado)
            {
                var usuarioActual = await _usuarioService.ObtenerPorIdAsync(id);

                if (usuarioActual == null)
                {
                    return NotFound(new
                    {
                        mensaje = "Usuario no encontrado."
                    });
                }

                if (request.IdRol != usuarioActual.IdRol ||
                    request.EstadoCuenta != usuarioActual.EstadoCuenta)
                {
                    return BadRequest(new
                    {
                        mensaje = "No podés modificar tu propio rol ni estado de cuenta."
                    });
                }
            }

            var usuario = await _usuarioService.ActualizarAsync(id, request);

            if (usuario == null)
            {
                return BadRequest(new
                {
                    mensaje = "El usuario no existe, el rol no existe o el email ya está registrado."
                });
            }

            return Ok(usuario);
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var idUsuarioClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(idUsuarioClaim, out int idAutenticado))
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario autenticado."
                });
            }

            if (id == idAutenticado)
            {
                return BadRequest(new
                {
                    mensaje = "No podés desactivar tu propia cuenta."
                });
            }

            var resultado = await _usuarioService.EliminarAsync(id);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "Usuario no encontrado."
                });
            }

            return Ok(new
            {
                mensaje = "Usuario desactivado correctamente."
            });
        }

    }
}