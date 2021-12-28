using System.ComponentModel.DataAnnotations;

namespace Domain.Services.Users.Models
{
    public class RegUserInfo
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Юзернейм обязателен для заполнения")]
        [MaxLength(30, ErrorMessage = "Юзернейм должен быть не больше 30 символов")]
        [MinLength(3, ErrorMessage = "Юзернейм должен быть не меньше 3 символов")]
        public string Username { get; set; }

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
