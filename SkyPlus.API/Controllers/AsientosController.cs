using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AsientosController : ControllerBase
    {
        private readonly IAsientoService _asientoService;

        public AsientosController(
            IAsientoService asientoService)
        {
            _asientoService = asientoService;
        }

        [HttpGet("vuelo/{idVuelo}")]
        [Authorize(Roles = "Administrador,Gerente,Agente de Reservas,Agente de Check-in")]
        public async Task<IActionResult> ObtenerPorVuelo(
            int idVuelo)
        {
            var asientos =
                await _asientoService.ObtenerPorVueloAsync(
                    idVuelo);

            return Ok(asientos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Gerente,Agente de Reservas,Agente de Check-in")]
        public async Task<IActionResult> ObtenerPorId(
            int id)
        {
            var asiento =
                await _asientoService.ObtenerPorIdAsync(id);

            if (asiento == null)
            {
                return NotFound(new
                {
                    mensaje = "Asiento no encontrado."
                });
            }

            return Ok(asiento);
        }

        [HttpPost("generar/{idVuelo}")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Generar(
            int idVuelo)
        {
            var resultado =
                await _asientoService.GenerarAsientosAsync(
                    idVuelo);

            if (!resultado)
            {
                var vueloExiste =
                    await _asientoService
                        .ObtenerPorVueloAsync(idVuelo);

                if (vueloExiste.Count > 0)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El vuelo ya tiene sus asientos generados."
                    });
                }

                return NotFound(new
                {
                    mensaje =
                        "El vuelo no existe."
                });
            }

            return Ok(new
            {
                mensaje =
                    "Los 180 asientos fueron generados correctamente."
            });
        }

        [HttpPut("{id}/estado")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> ActualizarEstado(
            int id,
            [FromBody] ActualizarEstadoAsientoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Estado))
            {
                return BadRequest(new
                {
                    mensaje = "El estado es obligatorio."
                });
            }

            var asiento =
                await _asientoService.ActualizarEstadoAsync(
                    id,
                    request);

            if (asiento == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El asiento no existe o el estado no es válido."
                });
            }

            return Ok(asiento);
        }
    }
}