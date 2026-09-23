using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Services;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VuelosController : ControllerBase
    {
        private readonly IVueloService _vueloService;

        public VuelosController(IVueloService vueloService)
        {
            _vueloService = vueloService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var vuelos =
                await _vueloService.ObtenerTodosAsync();

            return Ok(vuelos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var vuelo =
                await _vueloService.ObtenerPorIdAsync(id);

            if (vuelo == null)
            {
                return NotFound(new
                {
                    mensaje = "Vuelo no encontrado."
                });
            }

            return Ok(vuelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CrearVueloRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                request.NumeroVuelo))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El número de vuelo es obligatorio."
                });
            }

            if (request.Tarifa < 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La tarifa no puede ser negativa."
                });
            }

            var vuelo =
                await _vueloService.CrearAsync(request);

            if (vuelo == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se pudo crear el vuelo. " +
                        "Verifique los datos relacionados, " +
                        "el número de vuelo y las fechas."
                });
            }

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = vuelo.IdVuelo },
                vuelo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ActualizarVueloRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                request.NumeroVuelo))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El número de vuelo es obligatorio."
                });
            }

            if (request.Tarifa < 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La tarifa no puede ser negativa."
                });
            }

            var vuelo =
                await _vueloService.ActualizarAsync(
                    id,
                    request);

            if (vuelo == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se pudo actualizar el vuelo. " +
                        "Verifique los datos relacionados, " +
                        "el número de vuelo y las fechas."
                });
            }

            return Ok(vuelo);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado =
                await _vueloService.EliminarAsync(id);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "Vuelo no encontrado."
                });
            }

            return Ok(new
            {
                mensaje =
                    "Vuelo eliminado correctamente."
            });
        }
    }
}