
using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Exceptions;
using SkyPlus.API.Services;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipajesController : ControllerBase
    {
        private readonly IEquipajeService _equipajeService;

        public EquipajesController(IEquipajeService equipajeService)
        {
            _equipajeService = equipajeService;
        }

        // GET: api/Equipajes/tipos
        [HttpGet("tipos")]
        public async Task<ActionResult<List<TipoEquipajeResponse>>>
            ObtenerTipos()
        {
            var tipos = await _equipajeService.ObtenerTiposAsync();

            return Ok(tipos);
        }

        // GET: api/Equipajes/reserva/5
        [HttpGet("reserva/{idReserva:int}")]
        public async Task<ActionResult<List<EquipajeReservaResponse>>>
            ObtenerPorReserva(int idReserva)
        {
            if (idReserva <= 0)
                return BadRequest("El ID de reserva no es válido.");

            var equipajes = await _equipajeService
                .ObtenerPorReservaAsync(idReserva);

            return Ok(equipajes);
        }

        // POST: api/Equipajes/contratar
        [HttpPost("contratar")]
        public async Task<ActionResult<EquipajeReservaResponse>>
            Contratar([FromBody] ContratarEquipajeRequest request)
        {
            if (request.IdReserva <= 0 ||
                request.IdTipoEquipaje <= 0)
            {
                return BadRequest(
                    "La reserva y el tipo de equipaje son obligatorios.");
            }

            try
            {
                var equipaje = await _equipajeService
                    .ContratarAsync(request);

                return CreatedAtAction(
                    nameof(ObtenerPorReserva),
                    new { idReserva = equipaje.IdReserva },
                    equipaje);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("registrar-peso")]
        public async Task<IActionResult> RegistrarPeso(
            [FromBody] RegistrarPesoEquipajeRequest request)
        {
            try
            {
                var resultado = await _equipajeService.RegistrarPesoAsync(request);
                return Ok(resultado);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

    }
}
