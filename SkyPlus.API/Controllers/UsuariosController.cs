using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Services;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                    mensaje = "El rol no existe o el email ya está registrado."
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
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