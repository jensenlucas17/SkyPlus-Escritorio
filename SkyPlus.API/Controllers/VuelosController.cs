using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Services;
using SkyPlus.API.Exceptions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int idUsuario))
                    return Unauthorized(new { mensaje = "No se pudo identificar al usuario." });

                // Cancela el vuelo y reembolsa automáticamente todas sus reservas activas
                var (reservas, total) = await _vueloService.CancelarConReembolsosAsync(id, idUsuario);

                return Ok(new
                {
                    mensaje = reservas == 0
                        ? "Vuelo cancelado correctamente. No tenía reservas activas."
                        : $"Vuelo cancelado. Se cancelaron {reservas} reservas y se reembolsaron {total:C0} a sus pasajeros. " +
                          "Los asientos quedaron libres y la aeronave quedó disponible para otros vuelos.",
                    reservasCanceladas = reservas,
                    totalReembolsado = total
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
