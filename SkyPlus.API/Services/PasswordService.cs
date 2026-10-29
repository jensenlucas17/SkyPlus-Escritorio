using System.Security.Cryptography;

namespace SkyPlus.API.Services
{
    public class PasswordService
    {
        private const int Iteraciones = 600000;
        private const int TamanoSal = 16;
        private const int TamanoHash = 32;

        public string GenerarHash(string password)
        {
            byte[] sal = RandomNumberGenerator.GetBytes(TamanoSal);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                sal,
                Iteraciones,
                HashAlgorithmName.SHA256,
                TamanoHash);

            return $"PBKDF2${Iteraciones}$" +
                   $"{Convert.ToBase64String(sal)}$" +
                   $"{Convert.ToBase64String(hash)}";
        }

        public bool VerificarPassword(
            string password,
            string hashGuardado)
        {
            if (string.IsNullOrWhiteSpace(hashGuardado))
                return false;

            string[] partes = hashGuardado.Split('$');

            if (partes.Length != 4 ||
                partes[0] != "PBKDF2" ||
                !int.TryParse(partes[1], out int iteraciones) ||
                iteraciones != Iteraciones)
            {
                return false;
            }

            try
            {
                byte[] sal = Convert.FromBase64String(partes[2]);
                byte[] hashOriginal =
                    Convert.FromBase64String(partes[3]);

                if (sal.Length != TamanoSal ||
                    hashOriginal.Length != TamanoHash)
                {
                    return false;
                }

                byte[] hashCalculado =
                    Rfc2898DeriveBytes.Pbkdf2(
                        password,
                        sal,
                        iteraciones,
                        HashAlgorithmName.SHA256,
                        TamanoHash);

                return CryptographicOperations.FixedTimeEquals(
                    hashCalculado,
                    hashOriginal);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}