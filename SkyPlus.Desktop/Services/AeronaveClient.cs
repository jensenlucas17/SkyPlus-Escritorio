// SkyPlus.Desktop/Services/AeronaveClientFake.cs
// TODO: eliminar cuando exista el AeronaveClient real conectado a la API.

using System.Collections.Generic;
using System.Linq;
using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class AeronaveClientFake
    {
        private static readonly List<AeronaveResponse> _aeronaves = new()
        {
            new AeronaveResponse { IdAeronave = 1, Matricula = "LV-ABC", Modelo = "Boeing 737-800" },
            new AeronaveResponse { IdAeronave = 2, Matricula = "LV-DEF", Modelo = "Airbus A320" },
            new AeronaveResponse { IdAeronave = 3, Matricula = "LV-GHI", Modelo = "Embraer E190" },
            new AeronaveResponse { IdAeronave = 4, Matricula = "LV-JKL", Modelo = "Boeing 737 MAX 8" },
        };

        public List<AeronaveResponse> ObtenerTodos() => _aeronaves.ToList();

        public void Crear(AeronaveResponse aeronave)
        {
            aeronave.IdAeronave = _aeronaves.Count == 0 ? 1 : _aeronaves.Max(a => a.IdAeronave) + 1;
            _aeronaves.Add(aeronave);
        }

        public void Actualizar(AeronaveResponse aeronave)
        {
            var existente = _aeronaves.FirstOrDefault(a => a.IdAeronave == aeronave.IdAeronave);
            if (existente == null) return;

            existente.Matricula = aeronave.Matricula;
            existente.Modelo = aeronave.Modelo;
        }

        public void Eliminar(int idAeronave)
        {
            var existente = _aeronaves.FirstOrDefault(a => a.IdAeronave == idAeronave);
            if (existente != null) _aeronaves.Remove(existente);
        }
    }
}