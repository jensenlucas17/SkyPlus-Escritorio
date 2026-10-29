using Microsoft.EntityFrameworkCore;
using SkyPlus.API.Constants;
using SkyPlus.API.DTOs;
using SkyPlus.API.Exceptions;
using SkyPlus.Domain.Entities;
using SkyPlus.Infrastructure.Data;
using System.Data;

namespace SkyPlus.API.Services
{
    public class CheckInService : ICheckInService
    {
        private readonly SkyPlusDbContext _context;

        public CheckInService(SkyPlusDbContext context)
        {
            _context = context;
        }

        private IQueryable<CheckInResponse> Consultar()
        {
            return _context.CheckIns
                .AsNoTracking()
                .Select(c => new CheckInResponse
                {
                    IdCheckin = c.IdCheckin,
                    IdReserva = c.IdReserva,
                    CodigoReserva = c.Reserva != null
                        ? c.Reserva.CodigoReserva : "",
                    IdUsuarioAgente = c.IdUsuarioAgente,
                    CodigoQr = c.CodigoQr,
                    BoardingPass = c.BoardingPass,
                    FechaCheckin = c.FechaCheckin,
                    EstadoCheckin = c.EstadoCheckin
                });
        }

        public Task<List<CheckInResponse>> ObtenerTodosAsync()
        {
            return Consultar()
                .OrderByDescending(c => c.FechaCheckin)
                .ToListAsync();
        }

        public Task<CheckInResponse?> ObtenerPorIdAsync(int id)
        {
            return Consultar()
                .FirstOrDefaultAsync(c => c.IdCheckin == id);
        }

        // Todas las reglas para habilitar el check-in.
        // Este método es utilizado tanto por la consulta de estado
        // como por la creación del check-in.
        private async Task<EstadoCheckInResponse> EvaluarEstadoAsync(
            int idReserva)
        {
            if (idReserva <= 0)
                throw new ValidacionNegocioException(
                    "Seleccioná una reserva válida.");

            var reserva = await _context.Reservas
                .AsNoTracking()
                .Include(r => r.Vuelo)
                .Include(r => r.Pasajero)
                .Include(r => r.Asiento)
                .FirstOrDefaultAsync(r => r.IdReserva == idReserva);

            if (reserva == null)
                throw new ValidacionNegocioException(
                    "La reserva no existe.");

            bool tieneCheckIn = await _context.CheckIns
                .AnyAsync(c => c.IdReserva == idReserva);

            var equipajes = await _context.EquipajesReserva
                .AsNoTracking()
                .Where(e => e.IdReserva == idReserva)
                .ToListAsync();

            var cargos = await _context.CargosAdicionales
                .AsNoTracking()
                .Where(c => c.IdReserva == idReserva)
                .ToListAsync();

            int equipajesPendientes = equipajes.Count(e =>
                e.Estado == "PendientePago");

            var equipajesActivos = equipajes
                .Where(e => e.Estado == "Activo")
                .ToList();

            int equipajesSinPesar = equipajesActivos.Count(e =>
                !e.PesoKg.HasValue || e.PesoKg.Value <= 0);

            var cargosPendientes = cargos
                .Where(c => c.Estado == "Pendiente")
                .ToList();

            string mensaje = "";

            if (reserva.EstadoReserva != "Confirmada")
            {
                mensaje = "La reserva no está confirmada.";
            }
            else if (tieneCheckIn)
            {
                mensaje = "La reserva ya tiene un check-in registrado.";
            }
            else if (reserva.Pasajero == null ||
                     reserva.Vuelo == null ||
                     reserva.Asiento == null)
            {
                mensaje = "Faltan datos del pasajero, vuelo o asiento.";
            }
            else if (reserva.Vuelo.EstadoVuelo != "Programado" &&
                     reserva.Vuelo.EstadoVuelo != "Confirmado")
            {
                mensaje = "El vuelo no está habilitado para check-in.";
            }
            else if (equipajesPendientes > 0)
            {
                mensaje = "Hay equipajes pendientes de pago.";
            }
            else if (equipajesSinPesar > 0)
            {
                mensaje = "Hay equipajes sin peso válido registrado.";
            }
            else if (cargosPendientes.Count > 0)
            {
                mensaje = "Hay cargos adicionales pendientes de pago.";
            }
            else
            {
                foreach (var equipaje in equipajesActivos)
                {
                    decimal excesoKg = Math.Max(
                        0m,
                        equipaje.PesoKg!.Value -
                        equipaje.LimiteKgContratado);

                    decimal montoEsperado =
                        excesoKg *
                        TarifasEquipaje.PrecioPorKgExcedido;

                    var cargosExceso = cargos
                        .Where(c =>
                            c.IdEquipajeReserva ==
                                equipaje.IdEquipajeReserva &&
                            c.TipoCargo == "ExcesoPeso")
                        .ToList();

                    var cargosPagados = cargosExceso
                        .Where(c => c.Estado == "Pagado")
                        .ToList();

                    if (excesoKg > 0)
                    {
                        if (cargosPagados.Count != 1 ||
                            cargosPagados[0].Monto != montoEsperado)
                        {
                            mensaje =
                                "El exceso de peso no está " +
                                "correctamente abonado.";
                            break;
                        }
                    }
                    else if (cargosPagados.Count > 0)
                    {
                        mensaje =
                            "Existe un cargo por exceso de peso " +
                            "pagado que no coincide con el peso registrado.";
                        break;
                    }
                }
            }

            bool puede = string.IsNullOrEmpty(mensaje);

            if (puede)
                mensaje =
                    "La reserva está lista para realizar el check-in.";

            return new EstadoCheckInResponse
            {
                IdReserva = idReserva,
                PuedeRealizarCheckIn = puede,
                EquipajesSinPesar = equipajesSinPesar,
                CargosPendientes = cargosPendientes.Count,
                MontoPendiente = cargosPendientes.Sum(c => c.Monto),
                Mensaje = mensaje
            };
        }

        public Task<EstadoCheckInResponse> ConsultarEstadoAsync(
            int idReserva)
        {
            return EvaluarEstadoAsync(idReserva);
        }

        public async Task<CheckInResponse> CrearAsync(
            CrearCheckInRequest request,
            int idUsuarioAutenticado)
        {
            if (request.IdReserva <= 0)
                throw new ValidacionNegocioException(
                    "Seleccioná una reserva válida.");

            if (idUsuarioAutenticado <= 0)
                throw new ValidacionNegocioException(
                    "El usuario autenticado no es válido.");

            int idCheckIn;

            await using (var tx = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable))
            {
                var agente = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u =>
                        u.IdUsuario == idUsuarioAutenticado);
                if (agente == null ||
                    agente.EstadoCuenta != "Activo" ||
                    agente.Rol?.NombreRol != "Agente de Check-in")
                {
                    throw new ValidacionNegocioException(
                        "Solo un Agente de Check-in activo " +
                        "puede realizar el check-in.");
                }

                // Se aplican las mismas reglas que en
                // GET /api/CheckIns/estado/{idReserva}.
                var estado = await EvaluarEstadoAsync(
                    request.IdReserva);

                if (!estado.PuedeRealizarCheckIn)
                    throw new ValidacionNegocioException(
                        estado.Mensaje);

                var reserva = await _context.Reservas
                    .Include(r => r.Pasajero)
                    .Include(r => r.Vuelo)
                    .Include(r => r.Asiento)
                    .FirstAsync(r =>
                        r.IdReserva == request.IdReserva);

                var codigoQr = Guid.NewGuid().ToString("N");

                var boardingPass =
                    $"SKYPLUS | {reserva.CodigoReserva} | " +
                    $"{reserva.Pasajero!.Apellido}, " +
                    $"{reserva.Pasajero.Nombre} | " +
                    $"{reserva.Vuelo!.NumeroVuelo} | " +
                    $"{reserva.Asiento!.Fila}" +
                    $"{reserva.Asiento.Letra}";

                var checkIn = new CheckIn
                {
                    IdReserva = reserva.IdReserva,
                    IdUsuarioAgente = agente.IdUsuario,
                    CodigoQr = codigoQr,
                    BoardingPass = boardingPass,
                    FechaCheckin = DateTime.UtcNow,
                    EstadoCheckin = "Realizado"
                };

                _context.CheckIns.Add(checkIn);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                idCheckIn = checkIn.IdCheckin;
            }

            return await ObtenerPorIdAsync(idCheckIn)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar el check-in registrado.");
        }

        public async Task<ValidarQrResponse> ValidarQrAsync(string codigoQr)
        {
            if (string.IsNullOrWhiteSpace(codigoQr))
            {
                return new ValidarQrResponse
                {
                    EsValido = false,
                    Mensaje = "El código QR es obligatorio."
                };
            }

            codigoQr = codigoQr.Trim();

            var checkIn = await _context.CheckIns
                .AsNoTracking()
                .Include(c => c.Reserva)
                    .ThenInclude(r => r!.Pasajero)
                .Include(c => c.Reserva)
                    .ThenInclude(r => r!.Vuelo)
                .Include(c => c.Reserva)
                    .ThenInclude(r => r!.Asiento)
                .FirstOrDefaultAsync(c => c.CodigoQr == codigoQr);

            if (checkIn == null)
            {
                return new ValidarQrResponse
                {
                    EsValido = false,
                    Mensaje = "El código QR no corresponde a ningún check-in."
                };
            }

            var reserva = checkIn.Reserva;

            if (checkIn.EstadoCheckin == "Embarcado")
            {
                return new ValidarQrResponse
                {
                    EsValido = false,
                    IdCheckin = checkIn.IdCheckin,
                    IdReserva = checkIn.IdReserva,
                    EstadoCheckin = checkIn.EstadoCheckin,
                    Mensaje = "El pasajero ya embarcó."
                };
            }

            if (checkIn.EstadoCheckin != "Realizado")
            {
                return new ValidarQrResponse
                {
                    EsValido = false,
                    IdCheckin = checkIn.IdCheckin,
                    IdReserva = checkIn.IdReserva,
                    EstadoCheckin = checkIn.EstadoCheckin,
                    Mensaje = "El check-in no se encuentra habilitado."
                };
            }

            if (reserva == null ||
                reserva.EstadoReserva != "Confirmada")
            {
                return new ValidarQrResponse
                {
                    EsValido = false,
                    IdCheckin = checkIn.IdCheckin,
                    IdReserva = checkIn.IdReserva,
                    EstadoCheckin = checkIn.EstadoCheckin,
                    Mensaje = "La reserva no está confirmada."
                };
            }

            if (reserva.Pasajero == null ||
                reserva.Vuelo == null ||
                reserva.Asiento == null)
            {
                return new ValidarQrResponse
                {
                    EsValido = false,
                    IdCheckin = checkIn.IdCheckin,
                    IdReserva = checkIn.IdReserva,
                    EstadoCheckin = checkIn.EstadoCheckin,
                    Mensaje = "Faltan datos para validar el embarque."
                };
            }

            if (reserva.Vuelo.EstadoVuelo != "Programado" &&
                reserva.Vuelo.EstadoVuelo != "Confirmado")
            {
                return new ValidarQrResponse
                {
                    EsValido = false,
                    IdCheckin = checkIn.IdCheckin,
                    IdReserva = checkIn.IdReserva,
                    EstadoCheckin = checkIn.EstadoCheckin,
                    Mensaje = "El vuelo no está habilitado para embarque."
                };
            }

            return new ValidarQrResponse
            {
                EsValido = true,
                Mensaje = "Código QR válido. Check-in confirmado.",
                IdCheckin = checkIn.IdCheckin,
                IdReserva = checkIn.IdReserva,
                CodigoReserva = reserva.CodigoReserva,
                Pasajero =
                    $"{reserva.Pasajero.Apellido}, {reserva.Pasajero.Nombre}",
                NumeroVuelo = reserva.Vuelo.NumeroVuelo,
                Asiento =
                    $"{reserva.Asiento.Fila}{reserva.Asiento.Letra}",
                EstadoCheckin = checkIn.EstadoCheckin
            };
        }

        public async Task<ValidarQrResponse> RegistrarEmbarqueAsync(
                RegistrarEmbarqueRequest request,
                int idUsuarioAutenticado)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CodigoQr))
                throw new ValidacionNegocioException(
                    "El código QR es obligatorio.");

            if (idUsuarioAutenticado <= 0)
                throw new ValidacionNegocioException(
                    "El usuario autenticado no es válido.");

            string codigoQr = request.CodigoQr.Trim();

            await using var tx = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var agente = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == idUsuarioAutenticado);

            if (agente == null ||
                agente.EstadoCuenta != "Activo" ||
                agente.Rol?.NombreRol != "Agente de Check-in")
            {
                throw new ValidacionNegocioException(
                    "Solo un Agente de Check-in activo puede registrar embarques.");
            }

            // La validación se ejecuta dentro de la misma transacción.
            var validacion = await ValidarQrAsync(codigoQr);
            if (!validacion.EsValido)
                throw new ValidacionNegocioException(validacion.Mensaje);

            var checkIn = await _context.CheckIns
                .FirstOrDefaultAsync(c => c.CodigoQr == codigoQr);

            if (checkIn == null || checkIn.EstadoCheckin != "Realizado")
                throw new ValidacionNegocioException(
                    "El pasajero ya embarcó o el check-in no está habilitado.");

            checkIn.EstadoCheckin = "Embarcado";

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            validacion.EsValido = true;
            validacion.EstadoCheckin = "Embarcado";
            validacion.Mensaje = "Embarque registrado correctamente.";
            return validacion;
        }

    }
}
