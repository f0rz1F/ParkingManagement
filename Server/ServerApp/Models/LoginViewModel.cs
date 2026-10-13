using System.ComponentModel.DataAnnotations;

namespace ServerApp.Models
{
    // Модель для форми входу
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введіть email")]
        [EmailAddress(ErrorMessage = "Некоректний формат email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}