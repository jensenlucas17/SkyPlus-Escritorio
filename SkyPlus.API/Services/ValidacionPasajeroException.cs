using System;

namespace SkyPlus.API.Services
{
    public class ValidacionPasajeroException : Exception
    {
        public ValidacionPasajeroException(string mensaje)
            : base(mensaje)
        {
        }
    }
}
