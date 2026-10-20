
using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Services;
using SkyPlus.API.Exceptions;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservasController : ControllerBase
    {
        private readonly IReservaService _reservaService;

        public ReservasController(IReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var reservas = await _reservaService.ObtenerTodosAsync();
            return Ok(reservas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var reserva = await _reservaService.ObtenerPorIdAsync(id);

            if (reserva == null)
            {
                return NotFound(new
                {
                    mensaje = "Reserva no encontrada."
                });
            }

            return Ok(reserva);
        }

        [HttpGet("pasajero/{idPasajero}")]
        public async Task<IActionResult> ObtenerPorPasajero(
            int idPasajero)
        {
            var reservas = await _reservaService
                .ObtenerPorPasajeroAsync(idPasajero);

            return Ok(reservas);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CrearReservaRequest request)
        {
            if (request.IdPasajero <= 0 ||
                request.IdVuelo <= 0 ||
                request.IdAsiento <= 0 ||
                request.IdUsuarioAgente <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "Los identificadores del pasajero, vuelo, asiento y agente deben ser mayores que cero."
                });
            }

            try
            {
                var reserva = await _reservaService.CrearAsync(request);

                if (reserva == null)
                {
                    return BadRequest(new
                    {
                        mensaje = "No se pudo crear la reserva. Verificá los datos ingresados."
                    });
                }

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = reserva.IdReserva },
                    reserva);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var resultado = await _reservaService.CancelarAsync(id);

                if (!resultado)
                {
                    return BadRequest(new
                    {
                        mensaje = "La reserva no existe o ya se encuentra cancelada."
                    });
                }

                return Ok(new
                {
                    mensaje = "Reserva cancelada correctamente y asiento liberado."
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
