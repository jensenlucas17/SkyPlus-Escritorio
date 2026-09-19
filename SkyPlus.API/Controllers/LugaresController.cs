using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Services;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LugaresController : ControllerBase
    {
        private readonly ILugarService _lugarService;

        public LugaresController(ILugarService lugarService)
        {
            _lugarService = lugarService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var lugares = await _lugarService.ObtenerTodosAsync();

            return Ok(lugares);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var lugar = await _lugarService.ObtenerPorIdAsync(id);

            if (lugar == null)
            {
                return NotFound(new
                {
                    mensaje = "Lugar no encontrado."
                });
            }

            return Ok(lugar);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CrearLugarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CodigoIata) ||
                string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Ciudad) ||
                string.IsNullOrWhiteSpace(request.Pais))
            {
                return BadRequest(new
                {
                    mensaje = "Todos los campos son obligatorios."
                });
            }

            var lugar = await _lugarService.CrearAsync(request);

            if (lugar == null)
            {
                return BadRequest(new
                {
                    mensaje = "El código IATA ya está registrado."
                });
            }

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = lugar.IdLugar },
                lugar);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ActualizarLugarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CodigoIata) ||
                string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Ciudad) ||
                string.IsNullOrWhiteSpace(request.Pais))
            {
                return BadRequest(new
                {
                    mensaje = "Todos los campos son obligatorios."
                });
            }

            var lugar = await _lugarService.ActualizarAsync(
                id,
                request);

            if (lugar == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El lugar no existe o el código IATA ya está registrado."
                });
            }

            return Ok(lugar);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _lugarService.EliminarAsync(id);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "Lugar no encontrado."
                });
            }

            return Ok(new
            {
                mensaje = "Lugar eliminado correctamente."
            });
        }
    }
}