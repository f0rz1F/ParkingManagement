using System.ComponentModel.DataAnnotations;

namespace ServerApp.Models
{
    // Модель форми входу адміністратора
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введіть email")]
        [EmailAddress(ErrorMessage = "Некоректний формат email")]
        public string Email { get; set; } = string.Empty; // Електронна пошта

        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty; // Пароль
    }
}