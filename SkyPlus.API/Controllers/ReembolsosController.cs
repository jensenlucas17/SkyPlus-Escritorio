
using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Exceptions;
using SkyPlus.API.Services;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReembolsosController : ControllerBase
    {
        private readonly IReembolsoService _service;

        public ReembolsosController(IReembolsoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReembolsoResponse>>>
            ObtenerTodos()
        {
            return Ok(await _service.ObtenerTodosAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReembolsoResponse>>
            ObtenerPorId(int id)
        {
            var reembolso = await _service.ObtenerPorIdAsync(id);

            if (reembolso == null)
                return NotFound(new
                {
                    mensaje = "El reembolso no existe."
                });

            return Ok(reembolso);
        }

        [HttpPost]
        public async Task<ActionResult<ReembolsoResponse>>
            Crear([FromBody] CrearReembolsoRequest request)
        {
            try
            {
                var reembolso = await _service.CrearAsync(request);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = reembolso.IdReembolso },
                    reembolso);
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
