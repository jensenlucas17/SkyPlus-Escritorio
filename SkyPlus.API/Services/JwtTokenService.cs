using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SkyPlus.Domain.Entities;

namespace SkyPlus.API.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTime Expiracion) GenerarToken(
            Usuario usuario,
            string nombreRol)
        {
            string key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "No está configurada la clave JWT.");

            string issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "No está configurado el emisor JWT.");

            string audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "No está configurada la audiencia JWT.");

            if (!int.TryParse(
                    _configuration["Jwt:ExpirationMinutes"],
                    out int minutos) ||
                minutos <= 0)
            {
                throw new InvalidOperationException(
                    "La duración del token JWT debe ser un número positivo.");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()),

                new(ClaimTypes.Name,
                    usuario.EmailCorporativo),

                new(ClaimTypes.Role, nombreRol),

                new(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            var clave = new SymmetricSecurityKey(
                Convert.FromBase64String(key));

            var credenciales = new SigningCredentials(
                clave,
                SecurityAlgorithms.HmacSha256);

            DateTime expiracion = DateTime.UtcNow
                .AddMinutes(minutos);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiracion,
                signingCredentials: credenciales
            );

            return (
                new JwtSecurityTokenHandler().WriteToken(token),
                expiracion
            );
        }
    }
}
