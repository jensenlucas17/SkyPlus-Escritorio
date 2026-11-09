using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyPlus.API.DTOs;
using SkyPlus.API.Services;
using System.Security.Claims;
using System.Text.Json;

namespace SkyPlus.API.Controllers
{
    [ApiController]
    [Route("api/backups")]
    [Authorize]
    public class BackupController : ControllerBase
    {
        private readonly IBackupService _backupService;
        private readonly AutorizacionRestauracionService _autorizacion;

        public BackupController(
            IBackupService backupService,
            AutorizacionRestauracionService autorizacion)
        {
            _backupService = backupService;
            _autorizacion = autorizacion;
        }

        // =========================================
        // GENERAR BACKUP
        // POST /api/backups
        // Solo Administrador
        // =========================================

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GenerarBackup(
            CancellationToken cancellationToken)
        {
            var idClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out int idAdministrador) ||
                idAdministrador <= 0)
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al administrador."
                });
            }

            try
            {
                string archivo =
                    await _backupService.GenerarBackupAsync(
                        idAdministrador,
                        cancellationToken);

                return Ok(new
                {
                    mensaje = "Backup generado correctamente.",
                    archivo
                });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(503, new
                {
                    mensaje = "La operación de backup fue interrumpida."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo generar el backup. Revisá los registros del servidor."
                });
            }
        }

        // =========================================
        // CONSULTAR HISTORIAL
        // GET /api/backups
        // Administrador y Gerente
        // =========================================

        [HttpGet]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> ObtenerHistorial(
            CancellationToken cancellationToken)
        {
            try
            {
                var historial =
                    await _backupService.ObtenerHistorialAsync(
                        cancellationToken);

                return Ok(historial);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(503, new
                {
                    mensaje = "La consulta fue interrumpida."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo consultar el historial de backups."
                });
            }
        }

        // =========================================
        // ELIMINAR BACKUP
        // DELETE /api/backups/{id}
        // Solo Administrador
        // =========================================

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarBackup(
            Guid id,
            CancellationToken cancellationToken)
        {
            var idClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out int idAdministrador) ||
                idAdministrador <= 0)
            {
                return Unauthorized(new
                {
                    mensaje = "Administrador no identificado."
                });
            }

            try
            {
                bool eliminado =
                    await _backupService.EliminarBackupAsync(
                        id,
                        idAdministrador,
                        cancellationToken);

                if (!eliminado)
                {
                    return NotFound(new
                    {
                        mensaje = "El backup no existe o no está disponible."
                    });
                }

                return Ok(new
                {
                    mensaje = "Backup eliminado correctamente. Se conservó su historial."
                });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new
                {
                    mensaje = "No se pudo eliminar el backup solicitado."
                });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(503, new
                {
                    mensaje = "La eliminación fue interrumpida."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno al eliminar el backup."
                });
            }
        }

        // =========================================
        // VALIDAR AUTORIZACIÓN CRUZADA
        // POST /api/backups/validar-restauracion
        // Solo Administrador
        // =========================================

        [HttpPost("validar-restauracion")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ValidarRestauracion(
            [FromBody] RestaurarBackupRequest request)
        {
            var idClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out int idAdministrador) ||
                idAdministrador <= 0)
            {
                return Unauthorized(new
                {
                    mensaje = "Administrador no identificado."
                });
            }

            int? idSegundoAdministrador =
                await _autorizacion.ValidarSegundoAdministradorAsync(
                    idAdministrador,
                    request);

            if (idSegundoAdministrador == null)
            {
                return BadRequest(new
                {
                    mensaje = "La autorización cruzada no es válida."
                });
            }

            return Ok(new
            {
                mensaje = "Doble autorización validada correctamente.",
                idAdministradorSolicitante = idAdministrador,
                idAdministradorAutorizante = idSegundoAdministrador
            });
        }

        // =========================================
        // RESTAURAR BACKUP EN BASE DE PRUEBA
        // POST /api/backups/{id}/restaurar-prueba
        // Solo Administrador
        // Requiere autorización de otro Administrador
        // =========================================

        [HttpPost("{id:guid}/restaurar-prueba")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RestaurarPrueba(
            Guid id,
            [FromBody] RestaurarBackupRequest request,
            CancellationToken cancellationToken)
        {
            var idClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out int idSolicitante) ||
                idSolicitante <= 0)
            {
                return Unauthorized(new
                {
                    mensaje = "Administrador no identificado."
                });
            }

            // Validar nuevamente al segundo administrador
            // dentro de esta misma solicitud.
            int? idAutorizante =
                await _autorizacion.ValidarSegundoAdministradorAsync(
                    idSolicitante,
                    request);

            if (idAutorizante == null)
            {
                return BadRequest(new
                {
                    mensaje = "La autorización cruzada no es válida."
                });
            }

            try
            {
                var resultado =
                    await _backupService.RestaurarPruebaAsync(
                        id,
                        idSolicitante,
                        idAutorizante.Value,
                        cancellationToken);

                return Ok(new
                {
                    mensaje = "Restauración de prueba completada correctamente.",
                    resultado
                });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(503, new
                {
                    mensaje = "La restauración fue interrumpida."
                });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new
                {
                    mensaje = "No se pudo completar la restauración. Revisá los registros del servidor."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno durante la restauración."
                });
            }
        }

        [HttpPost("autorizar-restauracion-produccion")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AutorizarRestauracionProduccion(
            [FromBody] AutorizarRestauracionProduccionRequest request,
            [FromServices] RestoreAuthorizationSigningService signingService)
        {
            var idClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out int idSolicitante) ||
                idSolicitante <= 0)
            {
                return Unauthorized(new
                {
                    mensaje = "Administrador no identificado."
                });
            }

            if (request.BackupId == Guid.Empty)
            {
                return BadRequest(new
                {
                    mensaje = "Debe seleccionar un backup válido."
                });
            }

            var credenciales = new RestaurarBackupRequest
            {
                ConfirmarRestauracion = request.ConfirmarRestauracion,
                EmailSegundoAdministrador =
                    request.EmailSegundoAdministrador,
                PasswordSegundoAdministrador =
                    request.PasswordSegundoAdministrador
            };

            int? idAutorizante =
                await _autorizacion.ValidarSegundoAdministradorAsync(
                    idSolicitante,
                    credenciales);

            if (idAutorizante == null)
            {
                return BadRequest(new
                {
                    mensaje = "La autorización del segundo administrador no es válida."
                });
            }

            var ahora = DateTime.UtcNow;

            var payload =
                new SkyPlus.Domain.Backup.RestoreAuthorizationPayload
                {
                    OperationId = Guid.NewGuid(),
                    BackupId = request.BackupId,
                    FirstAdministratorId = idSolicitante,
                    SecondAdministratorId = idAutorizante.Value,
                    ApprovedAtUtc = ahora,
                    ExpiresAtUtc = ahora.AddMinutes(10),
                    Purpose = "ProductionRestore",
                    TargetDatabase = "skyplus"
                };

            string autorizacionFirmada =
                signingService.Firmar(payload);

            return Ok(new
            {
                mensaje = "Autorización de producción firmada.",
                operationId = payload.OperationId,
                backupId = payload.BackupId,
                expiresAtUtc = payload.ExpiresAtUtc,
                autorizacionFirmada
            });
        }

        [HttpPost("autorizar-continuacion-produccion")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AutorizarContinuacionProduccion(
            [FromBody] AutorizarContinuacionProduccionRequest request,
            [FromServices] RestoreAuthorizationSigningService signingService,
            [FromServices] ProductionRestorePendingStateService pendingStateService,
            CancellationToken cancellationToken)
        {
            var idClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out int idSolicitante) ||
                idSolicitante <= 0)
            {
                return Unauthorized(new
                {
                    mensaje = "Administrador no identificado."
                });
            }

            if (request.OperationId == Guid.Empty ||
                request.BackupId == Guid.Empty ||
                !request.ConfirmarRestauracion)
            {
                return BadRequest(new
                {
                    mensaje = "Datos de continuación inválidos."
                });
            }

            var credenciales = new RestaurarBackupRequest
            {
                ConfirmarRestauracion = request.ConfirmarRestauracion,
                EmailSegundoAdministrador =
                    request.EmailSegundoAdministrador,
                PasswordSegundoAdministrador =
                    request.PasswordSegundoAdministrador
            };

            try
            {
                await pendingStateService.ValidarPendienteAsync(
                    request.OperationId,
                    request.BackupId,
                    cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new
                {
                    mensaje = "No existe una operación pendiente válida para continuar."
                });
            }
            catch (JsonException)
            {
                return StatusCode(500, new
                {
                    mensaje = "El estado de producción no tiene un formato válido."
                });
            }

            int? idAutorizante =
                await _autorizacion.ValidarSegundoAdministradorAsync(
                    idSolicitante,
                    credenciales);

            if (idAutorizante == null)
            {
                return BadRequest(new
                {
                    mensaje = "La autorización cruzada no es válida."
                });
            }

            var ahora = DateTime.UtcNow;

            var payload =
                new SkyPlus.Domain.Backup.RestoreAuthorizationPayload
                {
                    OperationId = request.OperationId,
                    BackupId = request.BackupId,
                    FirstAdministratorId = idSolicitante,
                    SecondAdministratorId = idAutorizante.Value,
                    ApprovedAtUtc = ahora,
                    ExpiresAtUtc = ahora.AddMinutes(10),
                    Purpose = "ProductionRestoreContinuation",
                    TargetDatabase = "skyplus"
                };

            string autorizacionFirmada =
                signingService.Firmar(payload);

            return Ok(new
            {
                mensaje = "Autorización de continuación firmada.",
                operationId = payload.OperationId,
                backupId = payload.BackupId,
                expiresAtUtc = payload.ExpiresAtUtc,
                autorizacionFirmada
            });
        }
    }
}
