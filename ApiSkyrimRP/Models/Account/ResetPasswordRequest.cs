using System.ComponentModel.DataAnnotations;

namespace ApiSkyrimRP.Models.Account
{
    public class ResetPasswordRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Почта обязательна для заполнения")]
        [EmailAddress(ErrorMessage = "Неверный формат почты")]
        [MaxLength(60, ErrorMessage = "Почта должна быть не больше 60 символов")]
        [MinLength(5, ErrorMessage = "Почта должна быть не меньше 5 символов")]
        public string Email { get; set; }
    }
}
