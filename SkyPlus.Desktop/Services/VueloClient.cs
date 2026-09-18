// SkyPlus.Desktop/Services/VueloClientFake.cs
// TODO: eliminar cuando exista el VueloClient real conectado a la API.

using System;
using System.Collections.Generic;
using System.Linq;
using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class VueloClientFake
    {
        private readonly LugarClientFake _lugarClient = new();
        private readonly AeronaveClientFake _aeronaveClient = new();

        private static readonly List<VueloResponse> _vuelos = new()
        {
            new VueloResponse { IdVuelo = 1, NumeroVuelo = "SP1001", IdAeronave = 1, IdLugarOrigen = 1, IdLugarDestino = 3, Salida = DateTime.Today.AddDays(1).AddHours(8), Llegada = DateTime.Today.AddDays(1).AddHours(10), EstadoVuelo = "Programado", Tarifa = 85000m, IdUsuarioOperador = 1 },
            new VueloResponse { IdVuelo = 2, NumeroVuelo = "SP1002", IdAeronave = 2, IdLugarOrigen = 1, IdLugarDestino = 4, Salida = DateTime.Today.AddDays(1).AddHours(14), Llegada = DateTime.Today.AddDays(1).AddHours(16), EstadoVuelo = "Programado", Tarifa = 72000m, IdUsuarioOperador = 1 },
            new VueloResponse { IdVuelo = 3, NumeroVuelo = "SP2001", IdAeronave = 3, IdLugarOrigen = 3, IdLugarDestino = 1, Salida = DateTime.Today.AddDays(2).AddHours(9), Llegada = DateTime.Today.AddDays(2).AddHours(11), EstadoVuelo = "Confirmado", Tarifa = 88000m, IdUsuarioOperador = 1 },
        };

        public List<VueloResponse> ObtenerTodos()
        {
            // Simula el join que haría la API entre vuelo, lugar y aeronave
            var lugares = _lugarClient.ObtenerTodos();
            var aeronaves = _aeronaveClient.ObtenerTodos();

            foreach (var vuelo in _vuelos)
            {
                vuelo.LugarOrigen = lugares.FirstOrDefault(l => l.IdLugar == vuelo.IdLugarOrigen)?.IataCiudad ?? "?";
                vuelo.LugarDestino = lugares.FirstOrDefault(l => l.IdLugar == vuelo.IdLugarDestino)?.IataCiudad ?? "?";
                vuelo.Aeronave = aeronaves.FirstOrDefault(a => a.IdAeronave == vuelo.IdAeronave)?.MatriculaModelo ?? "?";
            }

            return _vuelos.ToList();
        }

        public void Crear(VueloResponse vuelo)
        {
            vuelo.IdVuelo = _vuelos.Count == 0 ? 1 : _vuelos.Max(v => v.IdVuelo) + 1;
            _vuelos.Add(vuelo);
        }

        public void Actualizar(VueloResponse vuelo)
        {
            var existente = _vuelos.FirstOrDefault(v => v.IdVuelo == vuelo.IdVuelo);
            if (existente == null) return;

            existente.NumeroVuelo = vuelo.NumeroVuelo;
            existente.IdAeronave = vuelo.IdAeronave;
            existente.IdLugarOrigen = vuelo.IdLugarOrigen;
            existente.IdLugarDestino = vuelo.IdLugarDestino;
            existente.Salida = vuelo.Salida;
            existente.Llegada = vuelo.Llegada;
            existente.EstadoVuelo = vuelo.EstadoVuelo;
            existente.Tarifa = vuelo.Tarifa;
        }

        public void Eliminar(int idVuelo)
        {
            var existente = _vuelos.FirstOrDefault(v => v.IdVuelo == idVuelo);
            if (existente != null) _vuelos.Remove(existente);
        }
    }
}