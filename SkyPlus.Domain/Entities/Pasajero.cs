namespace SkyPlus.Domain.Entities
{
    public class Pasajero
    {
        public int IdPasajero { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Documento { get; set; } = string.Empty;

        public string Nacionalidad { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;
    }
}