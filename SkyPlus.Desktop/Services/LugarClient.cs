// SkyPlus.Desktop/Services/LugarClientFake.cs
// TODO: eliminar cuando exista el LugarClient real conectado a la API.

using System.Collections.Generic;
using System.Linq;
using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class LugarClientFake
    {
        private static readonly List<LugarResponse> _lugares = new()
        {
            new LugarResponse { IdLugar = 1, CodigoIata = "EZE", Nombre = "Aeropuerto Internacional Ministro Pistarini", Ciudad = "Buenos Aires", Pais = "Argentina" },
            new LugarResponse { IdLugar = 2, CodigoIata = "AEP", Nombre = "Aeroparque Jorge Newbery", Ciudad = "Buenos Aires", Pais = "Argentina" },
            new LugarResponse { IdLugar = 3, CodigoIata = "COR", Nombre = "Aeropuerto Ingeniero Taravella", Ciudad = "Córdoba", Pais = "Argentina" },
            new LugarResponse { IdLugar = 4, CodigoIata = "CNQ", Nombre = "Aeropuerto Doctor Fernando Piragine Niveyro", Ciudad = "Corrientes", Pais = "Argentina" },
            new LugarResponse { IdLugar = 5, CodigoIata = "MDZ", Nombre = "Aeropuerto El Plumerillo", Ciudad = "Mendoza", Pais = "Argentina" },
            new LugarResponse { IdLugar = 6, CodigoIata = "SCL", Nombre = "Aeropuerto Arturo Merino Benítez", Ciudad = "Santiago", Pais = "Chile" },
            new LugarResponse { IdLugar = 7, CodigoIata = "MVD", Nombre = "Aeropuerto de Carrasco", Ciudad = "Montevideo", Pais = "Uruguay" },
            new LugarResponse { IdLugar = 8, CodigoIata = "GRU", Nombre = "Aeropuerto de Guarulhos", Ciudad = "São Paulo", Pais = "Brasil" },
        };

        public List<LugarResponse> ObtenerTodos() => _lugares.ToList();

        public void Crear(LugarResponse lugar)
        {
            lugar.IdLugar = _lugares.Count == 0 ? 1 : _lugares.Max(l => l.IdLugar) + 1;
            _lugares.Add(lugar);
        }

        public void Actualizar(LugarResponse lugar)
        {
            var existente = _lugares.FirstOrDefault(l => l.IdLugar == lugar.IdLugar);
            if (existente == null) return;

            existente.CodigoIata = lugar.CodigoIata;
            existente.Nombre = lugar.Nombre;
            existente.Ciudad = lugar.Ciudad;
            existente.Pais = lugar.Pais;
        }

        public void Eliminar(int idLugar)
        {
            var existente = _lugares.FirstOrDefault(l => l.IdLugar == idLugar);
            if (existente != null) _lugares.Remove(existente);
        }
    }
}