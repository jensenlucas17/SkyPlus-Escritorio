
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs.Reportes;
using SkyPlus.API.Exceptions;
using SkyPlus.API.Services;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Gerente")]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reporteService;

        public ReportesController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        // =========================================
        // VENTAS
        // GET api/Reportes/ventas
        // =========================================

        [HttpGet("ventas")]
        public async Task<ActionResult<ReporteVentasResponse>> Ventas(
            [FromQuery] ReporteFiltrosRequest filtros)
        {
            try
            {
                var resultado =
                    await _reporteService.ObtenerVentasAsync(filtros);

                return Ok(resultado);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // =========================================
        // RESERVAS
        // GET api/Reportes/reservas
        // =========================================

        [HttpGet("reservas")]
        public async Task<ActionResult<ReporteReservasResponse>> Reservas(
            [FromQuery] ReporteFiltrosRequest filtros)
        {
            try
            {
                var resultado =
                    await _reporteService.ObtenerReservasAsync(filtros);

                return Ok(resultado);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // =========================================
        // VUELOS
        // GET api/Reportes/vuelos
        // =========================================

        [HttpGet("vuelos")]
        public async Task<ActionResult<ReporteVuelosResponse>> Vuelos(
            [FromQuery] ReporteFiltrosRequest filtros)
        {
            try
            {
                var resultado =
                    await _reporteService.ObtenerVuelosAsync(filtros);

                return Ok(resultado);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // =========================================
        // PASAJEROS
        // GET api/Reportes/pasajeros
        // =========================================

        [HttpGet("pasajeros")]
        public async Task<ActionResult<ReportePasajerosResponse>> Pasajeros(
            [FromQuery] ReporteFiltrosRequest filtros)
        {
            try
            {
                var resultado =
                    await _reporteService.ObtenerPasajerosAsync(filtros);

                return Ok(resultado);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // =========================================
        // EQUIPAJES
        // GET api/Reportes/equipajes
        // =========================================

        [HttpGet("equipajes")]
        public async Task<ActionResult<ReporteEquipajesResponse>> Equipajes(
            [FromQuery] ReporteFiltrosRequest filtros)
        {
            try
            {
                var resultado =
                    await _reporteService.ObtenerEquipajesAsync(filtros);

                return Ok(resultado);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // =========================================
        // CHECK-INS
        // GET api/Reportes/checkins
        // =========================================

        [HttpGet("checkins")]
        public async Task<ActionResult<ReporteCheckInsResponse>> CheckIns(
            [FromQuery] ReporteFiltrosRequest filtros)
        {
            try
            {
                var resultado =
                    await _reporteService.ObtenerCheckInsAsync(filtros);

                return Ok(resultado);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
