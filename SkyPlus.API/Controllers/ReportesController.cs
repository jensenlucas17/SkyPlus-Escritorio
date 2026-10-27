using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.Exceptions;
using SkyPlus.API.Services;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reporteService;

        public ReportesController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [HttpGet("ventas")]
        public async Task<IActionResult> ObtenerVentas(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            try
            {
                if (desde == default || hasta == default)
                {
                    return BadRequest(new
                    {
                        mensaje = "Debés indicar las fechas desde y hasta."
                    });
                }

                var reporte = await _reporteService
                    .ObtenerVentasAsync(desde, hasta);

                return Ok(reporte);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpGet("reservas")]
        public async Task<IActionResult> ObtenerReservas(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            try
            {
                if (desde == default || hasta == default)
                {
                    return BadRequest(new
                    {
                        mensaje = "Debés indicar las fechas desde y hasta."
                    });
                }

                var reporte = await _reporteService
                    .ObtenerReservasAsync(desde, hasta);

                return Ok(reporte);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpGet("vuelos")]
        public async Task<IActionResult> ObtenerVuelos(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            try
            {
                if (desde == default || hasta == default)
                {
                    return BadRequest(new
                    {
                        mensaje = "Debés indicar las fechas desde y hasta."
                    });
                }

                var reporte = await _reporteService
                    .ObtenerVuelosAsync(desde, hasta);

                return Ok(reporte);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpGet("pasajeros")]
        public async Task<IActionResult> ObtenerPasajeros(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            try
            {
                if (desde == default || hasta == default)
                {
                    return BadRequest(new
                    {
                        mensaje = "Debés indicar las fechas desde y hasta."
                    });
                }

                var reporte = await _reporteService
                    .ObtenerPasajerosAsync(desde, hasta);

                return Ok(reporte);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpGet("equipajes")]
        public async Task<IActionResult> ObtenerEquipajes(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            try
            {
                if (desde == default || hasta == default)
                {
                    return BadRequest(new
                    {
                        mensaje = "Debés indicar las fechas desde y hasta."
                    });
                }

                var reporte = await _reporteService
                    .ObtenerEquipajesAsync(desde, hasta);

                return Ok(reporte);
            }
            catch (ValidacionNegocioException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpGet("checkins")]
        public async Task<IActionResult> ObtenerCheckIns(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            try
            {
                if (desde == default || hasta == default)
                {
                    return BadRequest(new
                    {
                        mensaje = "Debés indicar las fechas desde y hasta."
                    });
                }

                var reporte = await _reporteService
                    .ObtenerCheckInsAsync(desde, hasta);

                return Ok(reporte);
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
