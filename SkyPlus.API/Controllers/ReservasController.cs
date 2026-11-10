
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
    public class ReservasController : ControllerBase
    {
        private readonly IReservaService _reservaService;

        public ReservasController(IReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        // CONSULTAR TODAS LAS RESERVAS
        [HttpGet]
        // Administrador y Gerente solo consultan (pantalla de Vuelos: reservas de cada vuelo)
        [Authorize(Roles = "Agente de Reservas,Agente de Check-in,Administrador,Gerente")]
        public async Task<IActionResult> ObtenerTodos()
        {
            var reservas = await _reservaService.ObtenerTodosAsync();

            return Ok(reservas);
        }

        // CONSULTAR RESERVA POR ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Agente de Reservas,Agente de Check-in")]
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

        // CONSULTAR RESERVAS DE UN PASAJERO
        [HttpGet("pasajero/{idPasajero}")]
        [Authorize(Roles = "Agente de Reservas,Agente de Check-in")]
        public async Task<IActionResult> ObtenerPorPasajero(
            int idPasajero)
        {
            var reservas = await _reservaService
                .ObtenerPorPasajeroAsync(idPasajero);

            return Ok(reservas);
        }

        // CREAR RESERVA
        [HttpPost]
        [Authorize(Roles = "Agente de Reservas")]
        public async Task<IActionResult> Crear(
            [FromBody] CrearReservaRequest request)
        {
            if (request.IdPasajero <= 0 ||
                request.IdVuelo <= 0 ||
                request.IdAsiento <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "Los identificadores del pasajero, vuelo y asiento deben ser mayores que cero."
                });
            }

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
                var reserva = await _reservaService.CrearAsync(
                    request, idUsuario);

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

        // CANCELAR RESERVA
        [HttpPut("{id}/cancelar")]
        [Authorize(Roles = "Agente de Reservas")]
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
