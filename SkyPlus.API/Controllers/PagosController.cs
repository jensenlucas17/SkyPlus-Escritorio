
using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Exceptions;
using SkyPlus.API.Services;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagosController : ControllerBase
    {
        private readonly IPagoService _pagoService;

        public PagosController(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
            => Ok(await _pagoService.ObtenerTodosAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var pago = await _pagoService.ObtenerPorIdAsync(id);

            return pago == null
                ? NotFound(new { mensaje = "Pago no encontrado." })
                : Ok(pago);
        }

        // Pago inicial: pasaje + equipaje inicial.
        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CrearPagoRequest request)
        {
            try
            {
                var pago = await _pagoService.CrearAsync(request);

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
        [HttpPost("adicional")]
        public async Task<IActionResult> CrearAdicional(
            [FromBody] CrearPagoRequest request)
        {
            try
            {
                var pago = await _pagoService
                    .CrearAdicionalAsync(request);

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
