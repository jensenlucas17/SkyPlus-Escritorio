using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Agente de Reservas")]
    public class PasajerosController : ControllerBase
    {
        private readonly IPasajeroService _pasajeroService;

        public PasajerosController(
            IPasajeroService pasajeroService)
        {
            _pasajeroService = pasajeroService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var pasajeros =
                await _pasajeroService.ObtenerTodosAsync();

            return Ok(pasajeros);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var pasajero =
                await _pasajeroService.ObtenerPorIdAsync(id);

            if (pasajero == null)
            {
                return NotFound(new
                {
                    mensaje = "Pasajero no encontrado."
                });
            }

            return Ok(pasajero);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CrearPasajeroRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellido) ||
                string.IsNullOrWhiteSpace(request.Documento))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Nombre, apellido y documento son obligatorios."
                });
            }

            var pasajero =
                await _pasajeroService.CrearAsync(request);

            if (pasajero == null)
            {
                return BadRequest(new
                {
                    mensaje = "No se pudo crear el pasajero. Verificá los datos ingresados o si el documento ya está registrado."
                });
            }

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new
                {
                    id = pasajero.IdPasajero
                },
                pasajero);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ActualizarPasajeroRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellido) ||
                string.IsNullOrWhiteSpace(request.Documento))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Nombre, apellido y documento son obligatorios."
                });
            }

            var pasajero =
                await _pasajeroService.ActualizarAsync(
                    id,
                    request);

            if (pasajero == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El pasajero no existe o el documento " +
                        "ya está registrado."
                });
            }

            return Ok(pasajero);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado =
                await _pasajeroService.EliminarAsync(id);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "Pasajero no encontrado."
                });
            }

            return Ok(new
            {
                mensaje =
                    "Pasajero eliminado correctamente."
            });
        }

        [HttpPatch("{id}/reactivar")]
        public async Task<IActionResult> Reactivar(int id)
        {
            var reactivado = await _pasajeroService.ReactivarAsync(id);

            if (!reactivado)
                return BadRequest("El pasajero no existe o ya está activo.");

            return Ok(new { mensaje = "Pasajero reactivado correctamente." });
        }
    }
}