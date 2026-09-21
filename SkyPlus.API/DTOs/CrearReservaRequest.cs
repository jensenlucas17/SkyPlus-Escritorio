namespace SkyPlus.API.DTOs
{
    public class CrearReservaRequest
    {
        public int IdPasajero { get; set; }

        public int IdVuelo { get; set; }

        public int IdAsiento { get; set; }

        public int IdUsuarioAgente { get; set; }
    }
}