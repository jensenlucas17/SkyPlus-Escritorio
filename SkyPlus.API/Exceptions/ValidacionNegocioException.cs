namespace SkyPlus.API.Exceptions
{
    public class ValidacionNegocioException : Exception
    {
        public ValidacionNegocioException(string mensaje)
            : base(mensaje)
        {
        }
    }
}
