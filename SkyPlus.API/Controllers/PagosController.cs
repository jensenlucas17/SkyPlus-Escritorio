using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Exceptions;
using SkyPlus.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Cada acción indica qué roles la pueden usar.
    // El Agente de Check-in solo puede registrar el pago adicional
    // (equipaje agregado y exceso de peso, cobrados en el mostrador).
    [Authorize]
    public class PagosController : ControllerBase
    {
        private readonly IPagoService _pagoService;

        public PagosController(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

        [HttpGet]
        [Authorize(Roles = "Agente de Reservas")]
        public async Task<IActionResult> ObtenerTodos()
            => Ok(await _pagoService.ObtenerTodosAsync());

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Agente de Reservas,Agente de Check-in")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var pago = await _pagoService.ObtenerPorIdAsync(id);

            return pago == null
                ? NotFound(new { mensaje = "Pago no encontrado." })
                : Ok(pago);
        }

        // Pago inicial: pasaje + equipaje inicial.
        [HttpPost]
        [Authorize(Roles = "Agente de Reservas")]
        public async Task<IActionResult> Crear(
            [FromBody] CrearPagoRequest request)
        {
            var idUsuarioClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(idUsuarioClaim, out int idUsuario)
                || idUsuario <= 0)
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario autenticado."
                });
            }

            try
            {
                var pago = await _pagoService.CrearAsync(
                    request, idUsuario);

                if (pago == null)
                    return BadRequest(new
                    {
                        mensaje = "No se pudo registrar el pago."
                    });

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = pago.IdPago },
                    pago);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // Pago adicional: equipaje y cargos pendientes.
        // El Agente de Check-in solo cobra equipaje y excesos de peso (ver PagoService).
        [HttpPost("adicional")]
        [Authorize(Roles = "Agente de Reservas,Agente de Check-in")]
        public async Task<IActionResult> CrearAdicional(
            [FromBody] CrearPagoRequest request)
        {
            var idUsuarioClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(idUsuarioClaim, out int idUsuario)
                || idUsuario <= 0)
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario autenticado."
                });
            }

            try
            {
                var pago = await _pagoService.CrearAdicionalAsync(
                    request, idUsuario);

                if (pago == null)
                    return BadRequest(new
                    {
                        mensaje = "No se pudo registrar el pago adicional."
                    });

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = pago.IdPago },
                    pago);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}/anular")]
        [Authorize(Roles = "Agente de Reservas")]
        public async Task<IActionResult> Anular(int id)
        {
            try
            {
                await _pagoService.AnularAsync(id);

                return Ok(new
                {
                    mensaje = "Pago anulado correctamente. " +
                              "Se conservó el historial."
                });
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}