using System.ComponentModel.DataAnnotations;

namespace ApiSkyrimRP.Models.Account
{
    public class RefreshTokenRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Токен обязателен для заполнения")]
        [StringLength(50, MinimumLength = 40, ErrorMessage = "Невалидная строка")]
        public string RefreshToken { get; set; }
    }
}
