using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Services;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AeronavesController : ControllerBase
    {
        private readonly IAeronaveService _aeronaveService;

        public AeronavesController(
            IAeronaveService aeronaveService)
        {
            _aeronaveService = aeronaveService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var aeronaves =
                await _aeronaveService.ObtenerTodosAsync();

            return Ok(aeronaves);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var aeronave =
                await _aeronaveService.ObtenerPorIdAsync(id);

            if (aeronave == null)
            {
                return NotFound(new
                {
                    mensaje = "Aeronave no encontrada."
                });
            }

            return Ok(aeronave);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CrearAeronaveRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Matricula) ||
                string.IsNullOrWhiteSpace(request.Modelo))
            {
                return BadRequest(new
                {
                    mensaje = "Matrícula y modelo son obligatorios."
                });
            }

            var aeronave =
                await _aeronaveService.CrearAsync(request);

            if (aeronave == null)
            {
                return BadRequest(new
                {
                    mensaje = "La matrícula ya está registrada."
                });
            }

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = aeronave.IdAeronave },
                aeronave);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ActualizarAeronaveRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Matricula) ||
                string.IsNullOrWhiteSpace(request.Modelo))
            {
                return BadRequest(new
                {
                    mensaje = "Matrícula y modelo son obligatorios."
                });
            }

            var aeronave =
                await _aeronaveService.ActualizarAsync(id, request);

            if (aeronave == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La aeronave no existe o la matrícula ya está registrada."
                });
            }

            return Ok(aeronave);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado =
                await _aeronaveService.EliminarAsync(id);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "Aeronave no encontrada."
                });
            }

            return Ok(new
            {
                mensaje = "Aeronave eliminada correctamente."
            });
        }
    }
}