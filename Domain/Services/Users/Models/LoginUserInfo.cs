using System.ComponentModel.DataAnnotations;

namespace Domain.Services.Users.Models
{
    public class LoginUserInfo
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Почта обязательна для заполнения")]
        [EmailAddress(ErrorMessage = "Неверный формат почты")]
        [MaxLength(60, ErrorMessage = "Почта должна быть не больше 60 символов")]
        [MinLength(5, ErrorMessage = "Почта должна быть не меньше 5 символов")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Пароль обязателен для заполнения")]
        [MaxLength(48, ErrorMessage = "Пароль должен быть не больше 48 символов")]
        [MinLength(8, ErrorMessage = "Пароль должен быть не меньше 8 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
