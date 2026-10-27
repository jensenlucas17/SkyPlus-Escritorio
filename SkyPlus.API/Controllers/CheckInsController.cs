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
    [Authorize(Roles = "Agente de Check-in")]
    public class CheckInsController : ControllerBase
    {
        private readonly ICheckInService _checkInService;
        private readonly BoardingPassService _boardingPassService;

        public CheckInsController(
            ICheckInService checkInService,
            BoardingPassService boardingPassService)
        {
            _checkInService = checkInService;
            _boardingPassService = boardingPassService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            return Ok(await _checkInService.ObtenerTodosAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var checkIn = await _checkInService.ObtenerPorIdAsync(id);

            return checkIn == null
                ? NotFound(new { mensaje = "Check-in no encontrado." })
                : Ok(checkIn);
        }

        [HttpGet("estado/{idReserva:int}")]
        public async Task<IActionResult> ConsultarEstado(int idReserva)
        {
            try
            {
                var estado = await _checkInService
                    .ConsultarEstadoAsync(idReserva);

                return Ok(estado);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CrearCheckInRequest request)
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
                var checkIn = await _checkInService.CrearAsync(
                    request, idUsuario);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = checkIn.IdCheckin },
                    checkIn);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // Descargar tarjeta de embarque en PDF con código QR.
        [HttpGet("{id:int}/boarding-pass")]
        public async Task<IActionResult> DescargarBoardingPass(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje = "Seleccioná un check-in válido."
                    });
                }

                var checkIn = await _checkInService
                    .ObtenerPorIdAsync(id);

                if (checkIn == null)
                {
                    return NotFound(new
                    {
                        mensaje = "Check-in no encontrado."
                    });
                }

                byte[] pdf = await _boardingPassService
                    .GenerarPdfAsync(id);

                return File(
                    pdf,
                    "application/pdf",
                    $"SkyPlus_BoardingPass_{id}.pdf");
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpGet("validar-qr/{codigoQr}")]
        public async Task<IActionResult> ValidarQr(string codigoQr)
        {
            var resultado = await _checkInService.ValidarQrAsync(codigoQr);

            return Ok(resultado);
        }

        [HttpPost("embarcar")]
        public async Task<IActionResult> RegistrarEmbarque(
            [FromBody] RegistrarEmbarqueRequest request)
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
                var resultado = await _checkInService
                    .RegistrarEmbarqueAsync(request, idUsuario);

                return Ok(resultado);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

    }
}
