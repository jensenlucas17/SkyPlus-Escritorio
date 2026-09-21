using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class VueloService : IVueloService
    {
        private readonly SkyPlusDbContext _context;

        public VueloService(SkyPlusDbContext context)
        {
            _context = context;
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

        public async Task<VueloResponse?> CrearAsync(
            CrearVueloRequest request)
        {
            var numeroExiste =
                await _context.Vuelos.AnyAsync(
                    v => v.NumeroVuelo == request.NumeroVuelo);

            if (numeroExiste)
                return null;

            var aeronaveExiste =
                await _context.Aeronaves.AnyAsync(
                    a => a.IdAeronave == request.IdAeronave);

            var origenExiste =
                await _context.Lugares.AnyAsync(
                    l => l.IdLugar == request.IdLugarOrigen);

            var destinoExiste =
                await _context.Lugares.AnyAsync(
                    l => l.IdLugar == request.IdLugarDestino);

            var usuarioExiste =
                await _context.Usuarios.AnyAsync(
                    u => u.IdUsuario == request.IdUsuarioOperador);

            if (!aeronaveExiste ||
                !origenExiste ||
                !destinoExiste ||
                !usuarioExiste)
            {
                return null;
            }

            if (request.IdLugarOrigen ==
                request.IdLugarDestino)
            {
                return null;
            }

            if (request.Llegada <= request.Salida)
            {
                return null;
            }

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

            _context.Vuelos.Add(vuelo);

            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(
                vuelo.IdVuelo);
        }

        public async Task<VueloResponse?> ActualizarAsync(
            int id,
            ActualizarVueloRequest request)
        {
            var vuelo =
                await _context.Vuelos
                    .FirstOrDefaultAsync(
                        v => v.IdVuelo == id);

            if (vuelo == null)
                return null;

            var numeroExiste =
                await _context.Vuelos.AnyAsync(
                    v =>
                        v.NumeroVuelo ==
                        request.NumeroVuelo &&
                        v.IdVuelo != id);

            if (numeroExiste)
                return null;

            var aeronaveExiste =
                await _context.Aeronaves.AnyAsync(
                    a =>
                        a.IdAeronave ==
                        request.IdAeronave);

            var origenExiste =
                await _context.Lugares.AnyAsync(
                    l =>
                        l.IdLugar ==
                        request.IdLugarOrigen);

            var destinoExiste =
                await _context.Lugares.AnyAsync(
                    l =>
                        l.IdLugar ==
                        request.IdLugarDestino);

            var usuarioExiste =
                await _context.Usuarios.AnyAsync(
                    u =>
                        u.IdUsuario ==
                        request.IdUsuarioOperador);

            if (!aeronaveExiste ||
                !origenExiste ||
                !destinoExiste ||
                !usuarioExiste)
            {
                return null;
            }

            if (request.IdLugarOrigen ==
                request.IdLugarDestino)
            {
                return null;
            }

            if (request.Llegada <= request.Salida)
            {
                return null;
            }

            vuelo.NumeroVuelo =
                request.NumeroVuelo;

            vuelo.IdAeronave =
                request.IdAeronave;

            vuelo.IdUsuarioOperador =
                request.IdUsuarioOperador;

            vuelo.IdLugarOrigen =
                request.IdLugarOrigen;

            vuelo.IdLugarDestino =
                request.IdLugarDestino;

            vuelo.Salida =
                request.Salida;

            vuelo.Llegada =
                request.Llegada;

            vuelo.EstadoVuelo =
                request.EstadoVuelo;

            vuelo.Tarifa =
                request.Tarifa;

            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(id);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var vuelo =
                await _context.Vuelos
                    .FirstOrDefaultAsync(
                        v => v.IdVuelo == id);

            if (vuelo == null)
                return false;

            _context.Vuelos.Remove(vuelo);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}