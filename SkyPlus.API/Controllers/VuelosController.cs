using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Services;
using SkyPlus.API.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VuelosController : ControllerBase
    {
        private readonly IVueloService _vueloService;

        public VuelosController(IVueloService vueloService)
        {
            _vueloService = vueloService;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gerente,Agente de Reservas,Agente de Check-in")]
        public async Task<IActionResult> ObtenerTodos()
        {
            var vuelos = await _vueloService.ObtenerTodosAsync();
            return Ok(vuelos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Gerente,Agente de Reservas,Agente de Check-in")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var vuelo = await _vueloService.ObtenerPorIdAsync(id);

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
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Crear(
            [FromBody] CrearVueloRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NumeroVuelo))
            {
                return BadRequest(new
                {
                    mensaje = "El número de vuelo es obligatorio."
                });
            }

            if (request.Tarifa < 0)
            {
                return BadRequest(new
                {
                    mensaje = "La tarifa no puede ser negativa."
                });
            }

            try
            {
                var vuelo = await _vueloService.CrearAsync(request);

                if (vuelo == null)
                {
                    return BadRequest(new
                    {
                        mensaje = "No se pudo crear el vuelo. Verificá los datos ingresados."
                    });
                }

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = vuelo.IdVuelo },
                    vuelo);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ActualizarVueloRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NumeroVuelo))
            {
                return BadRequest(new
                {
                    mensaje = "El número de vuelo es obligatorio."
                });
            }

            if (request.Tarifa < 0)
            {
                return BadRequest(new
                {
                    mensaje = "La tarifa no puede ser negativa."
                });
            }

            try
            {
                var vuelo = await _vueloService.ActualizarAsync(id, request);

                if (vuelo == null)
                {
                    return BadRequest(new
                    {
                        mensaje = "No se pudo actualizar el vuelo. Verificá los datos ingresados."
                    });
                }

                return Ok(vuelo);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var resultado = await _vueloService.EliminarAsync(id);

                if (!resultado)
                {
                    return BadRequest(new
                    {
                        mensaje = "No se pudo cancelar el vuelo. Puede que no exista, ya esté cancelado o finalizado, o tenga reservas activas."
                    });
                }

                return Ok(new
                {
                    mensaje = "Vuelo cancelado correctamente. Se conservaron sus datos históricos."
                });
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }
    }
}
