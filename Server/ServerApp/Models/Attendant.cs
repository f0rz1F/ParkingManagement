using System.ComponentModel.DataAnnotations;

namespace ServerApp.Models
{
    // Модель працівника (таблиця Attendants)
    public class Attendant
    {
        [Key]
        public int ID { get; set; } // Первинний ключ

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // Ім'я

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty; // Електронна пошта

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public string PasswordHash { get; set; } = string.Empty; // Хеш пароля (встановлюється сервером)

        // Навігаційні властивості
        public ICollection<ParkingSpot> ParkingSpots { get; set; } = new List<ParkingSpot>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
