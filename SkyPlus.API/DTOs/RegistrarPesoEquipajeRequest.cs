
namespace SkyPlus.API.DTOs
{
    public class RegistrarPesoEquipajeRequest
    {
        public int IdEquipajeReserva { get; set; }

        public int IdUsuarioAgente { get; set; }

        public decimal PesoKg { get; set; }
    }
}
