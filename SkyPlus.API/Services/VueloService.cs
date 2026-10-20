using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.Infrastructure.Data;
using SkyPlus.API.Exceptions;

namespace SkyPlus.API.Services
{
    public class VueloService : IVueloService
    {
        private readonly SkyPlusDbContext _context;
        private readonly IAsientoService _asientoService;

        public VueloService(
            SkyPlusDbContext context,
            IAsientoService asientoService)
        {
            _context = context;
            _asientoService = asientoService;
        }

        public async Task<List<VueloResponse>> ObtenerTodosAsync()
        {
            return await _context.Vuelos
                .Include(v => v.Aeronave)
                .Include(v => v.LugarOrigen)
                .Include(v => v.LugarDestino)
                .Select(v => new VueloResponse
                {
                    IdVuelo = v.IdVuelo,
                    NumeroVuelo = v.NumeroVuelo,
                    IdAeronave = v.IdAeronave,
                    IdUsuarioOperador = v.IdUsuarioOperador,
                    IdLugarOrigen = v.IdLugarOrigen,
                    IdLugarDestino = v.IdLugarDestino,
                    Salida = v.Salida,
                    Llegada = v.Llegada,
                    EstadoVuelo = v.EstadoVuelo,
                    Tarifa = v.Tarifa,

                    Aeronave =
                        v.Aeronave.Matricula +
                        " - " +
                        v.Aeronave.Modelo,

                    LugarOrigen =
                        v.LugarOrigen.CodigoIata +
                        " - " +
                        v.LugarOrigen.Ciudad,

                    LugarDestino =
                        v.LugarDestino.CodigoIata +
                        " - " +
                        v.LugarDestino.Ciudad
                })
                .ToListAsync();
        }

        public async Task<VueloResponse?> ObtenerPorIdAsync(int id)
        {
            return await _context.Vuelos
                .Include(v => v.Aeronave)
                .Include(v => v.LugarOrigen)
                .Include(v => v.LugarDestino)
                .Where(v => v.IdVuelo == id)
                .Select(v => new VueloResponse
                {
                    IdVuelo = v.IdVuelo,
                    NumeroVuelo = v.NumeroVuelo,
                    IdAeronave = v.IdAeronave,
                    IdUsuarioOperador = v.IdUsuarioOperador,
                    IdLugarOrigen = v.IdLugarOrigen,
                    IdLugarDestino = v.IdLugarDestino,
                    Salida = v.Salida,
                    Llegada = v.Llegada,
                    EstadoVuelo = v.EstadoVuelo,
                    Tarifa = v.Tarifa,

                    Aeronave =
                        v.Aeronave.Matricula +
                        " - " +
                        v.Aeronave.Modelo,

                    LugarOrigen =
                        v.LugarOrigen.CodigoIata +
                        " - " +
                        v.LugarOrigen.Ciudad,

                    LugarDestino =
                        v.LugarDestino.CodigoIata +
                        " - " +
                        v.LugarDestino.Ciudad
                })
                .FirstOrDefaultAsync();
        }

        public async Task<VueloResponse?> CrearAsync(CrearVueloRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NumeroVuelo))
                throw new ValidacionNegocioException("El número de vuelo es obligatorio.");

            if (await _context.Vuelos.AnyAsync(v => v.NumeroVuelo == request.NumeroVuelo))
                throw new ValidacionNegocioException($"Ya existe un vuelo con el número {request.NumeroVuelo}.");

            var aeronave = await _context.Aeronaves.FindAsync(request.IdAeronave);
            if (aeronave == null)
                throw new ValidacionNegocioException("La aeronave seleccionada no existe.");
            if (aeronave.Estado != "Activo")
                throw new ValidacionNegocioException($"La aeronave {aeronave.Matricula} está inactiva. Debe reactivarla antes de asignarla.");

            var origen = await _context.Lugares.FindAsync(request.IdLugarOrigen);
            if (origen == null)
                throw new ValidacionNegocioException("El aeropuerto de origen no existe.");
            if (origen.Estado != "Activo")
                throw new ValidacionNegocioException($"El aeropuerto de origen {origen.CodigoIata} está inactivo.");

            var destino = await _context.Lugares.FindAsync(request.IdLugarDestino);
            if (destino == null)
                throw new ValidacionNegocioException("El aeropuerto de destino no existe.");
            if (destino.Estado != "Activo")
                throw new ValidacionNegocioException($"El aeropuerto de destino {destino.CodigoIata} está inactivo.");

            if (request.IdLugarOrigen == request.IdLugarDestino)
                throw new ValidacionNegocioException("El origen y el destino deben ser diferentes.");

            var operador = await _context.Usuarios.FindAsync(request.IdUsuarioOperador);
            if (operador == null)
                throw new ValidacionNegocioException("El usuario operador no existe.");
            if (operador.EstadoCuenta != "Activo")
                throw new ValidacionNegocioException("El usuario operador está inactivo.");

            if (request.Llegada <= request.Salida)
                throw new ValidacionNegocioException("La llegada debe ser posterior a la salida.");
            if (request.Tarifa <= 0)
                throw new ValidacionNegocioException("La tarifa debe ser mayor que cero.");
            if (request.EstadoVuelo != "Programado" && request.EstadoVuelo != "Confirmado")
                throw new ValidacionNegocioException("Un vuelo nuevo debe estar Programado o Confirmado.");

            var vuelo = new Domain.Entities.Vuelo
            {
                NumeroVuelo = request.NumeroVuelo,
                IdAeronave = request.IdAeronave,
                IdUsuarioOperador = request.IdUsuarioOperador,
                IdLugarOrigen = request.IdLugarOrigen,
                IdLugarDestino = request.IdLugarDestino,
                Salida = request.Salida,
                Llegada = request.Llegada,
                EstadoVuelo = request.EstadoVuelo,
                Tarifa = request.Tarifa
            };

            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Vuelos.Add(vuelo);
                await _context.SaveChangesAsync();

                if (!await _asientoService.GenerarAsientosAsync(vuelo.IdVuelo))
                {
                    throw new ValidacionNegocioException("No se pudieron generar los asientos del vuelo. No se guardó el vuelo.");
                }

                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();
                _context.ChangeTracker.Clear();
                throw;
            }

            return await ObtenerPorIdAsync(vuelo.IdVuelo);
        }

        public async Task<VueloResponse?> ActualizarAsync(int id, ActualizarVueloRequest request)
        {
            var vuelo = await _context.Vuelos.FindAsync(id);
            if (vuelo == null)
                throw new ValidacionNegocioException("El vuelo que desea actualizar no existe.");
            if (vuelo.EstadoVuelo == "Cancelado" || vuelo.EstadoVuelo == "Finalizado")
                throw new ValidacionNegocioException("No se puede modificar un vuelo cancelado o finalizado.");
            if (string.IsNullOrWhiteSpace(request.NumeroVuelo))
                throw new ValidacionNegocioException("El número de vuelo es obligatorio.");
            if (await _context.Vuelos.AnyAsync(v => v.NumeroVuelo == request.NumeroVuelo && v.IdVuelo != id))
                throw new ValidacionNegocioException($"Ya existe otro vuelo con el número {request.NumeroVuelo}.");

            var aeronave = await _context.Aeronaves.FindAsync(request.IdAeronave);
            if (aeronave == null)
                throw new ValidacionNegocioException("La aeronave seleccionada no existe.");
            if (aeronave.Estado != "Activo" && request.IdAeronave != vuelo.IdAeronave)
                throw new ValidacionNegocioException($"La aeronave {aeronave.Matricula} está inactiva.");

            var origen = await _context.Lugares.FindAsync(request.IdLugarOrigen);
            if (origen == null)
                throw new ValidacionNegocioException("El aeropuerto de origen no existe.");
            if (origen.Estado != "Activo" && request.IdLugarOrigen != vuelo.IdLugarOrigen)
                throw new ValidacionNegocioException($"El aeropuerto de origen {origen.CodigoIata} está inactivo.");

            var destino = await _context.Lugares.FindAsync(request.IdLugarDestino);
            if (destino == null)
                throw new ValidacionNegocioException("El aeropuerto de destino no existe.");
            if (destino.Estado != "Activo" && request.IdLugarDestino != vuelo.IdLugarDestino)
                throw new ValidacionNegocioException($"El aeropuerto de destino {destino.CodigoIata} está inactivo.");

            if (request.IdLugarOrigen == request.IdLugarDestino)
                throw new ValidacionNegocioException("El origen y el destino deben ser diferentes.");

            var operador = await _context.Usuarios.FindAsync(request.IdUsuarioOperador);
            if (operador == null)
                throw new ValidacionNegocioException("El usuario operador no existe.");
            if (operador.EstadoCuenta != "Activo" && request.IdUsuarioOperador != vuelo.IdUsuarioOperador)
                throw new ValidacionNegocioException("El usuario operador seleccionado está inactivo.");

            if (request.Llegada <= request.Salida)
                throw new ValidacionNegocioException("La llegada debe ser posterior a la salida.");
            if (request.Tarifa <= 0)
                throw new ValidacionNegocioException("La tarifa debe ser mayor que cero.");
            if (request.EstadoVuelo == "Cancelado")
                throw new ValidacionNegocioException("Para cancelar un vuelo utilice la operación de baja/cancelación.");
            if (request.EstadoVuelo != "Programado" &&
                request.EstadoVuelo != "Confirmado" &&
                request.EstadoVuelo != "Finalizado")
                throw new ValidacionNegocioException("El estado del vuelo no es válido.");

            var tieneReservas = await _context.Reservas.AnyAsync(r =>
                r.IdVuelo == id && r.EstadoReserva != "Cancelada");
            if (tieneReservas &&
                (request.IdAeronave != vuelo.IdAeronave ||
                 request.IdLugarOrigen != vuelo.IdLugarOrigen ||
                 request.IdLugarDestino != vuelo.IdLugarDestino ||
                 request.Salida != vuelo.Salida ||
                 request.Llegada != vuelo.Llegada))
                throw new ValidacionNegocioException(
                    "El vuelo tiene reservas activas. No se pueden cambiar aeronave, ruta ni horarios sin gestionar las reservas.");

            vuelo.NumeroVuelo = request.NumeroVuelo;
            vuelo.IdAeronave = request.IdAeronave;
            vuelo.IdUsuarioOperador = request.IdUsuarioOperador;
            vuelo.IdLugarOrigen = request.IdLugarOrigen;
            vuelo.IdLugarDestino = request.IdLugarDestino;
            vuelo.Salida = request.Salida;
            vuelo.Llegada = request.Llegada;
            vuelo.EstadoVuelo = request.EstadoVuelo;
            vuelo.Tarifa = request.Tarifa;

            await _context.SaveChangesAsync();
            return await ObtenerPorIdAsync(id);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var vuelo = await _context.Vuelos
                .FirstOrDefaultAsync(v => v.IdVuelo == id);

            if (vuelo == null)
                return false;

            if (vuelo.EstadoVuelo == "Cancelado" || vuelo.EstadoVuelo == "Finalizado")
                throw new ValidacionNegocioException("El vuelo ya está cancelado o finalizado.");

            var tieneReservasActivas = await _context.Reservas
                .AnyAsync(r =>
                    r.IdVuelo == id &&
                    r.EstadoReserva != "Cancelada");

            if (tieneReservasActivas)
                throw new ValidacionNegocioException("El vuelo tiene reservas activas. Primero debe gestionarlas antes de cancelarlo.");

            vuelo.EstadoVuelo = "Cancelado";

            await _context.SaveChangesAsync();

            return true;
        }
    }
}