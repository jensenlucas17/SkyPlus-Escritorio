using System.ComponentModel.DataAnnotations;

namespace SkyPlus.API.DTOs
{
    public class RestaurarBackupRequest
    {
        [Required]
        [EmailAddress]
        public string EmailSegundoAdministrador { get; set; } =
            string.Empty;

        [Required]
        public string PasswordSegundoAdministrador { get; set; } =
            string.Empty;

        [Required]
        public bool ConfirmarRestauracion { get; set; }
    }
}
