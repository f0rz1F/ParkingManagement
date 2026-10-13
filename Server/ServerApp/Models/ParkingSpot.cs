using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models
{
    // Модель паркувального місця (таблиця ParkingSpots)
    public class ParkingSpot
    {
        [Key]
        public int ID { get; set; } // Первинний ключ

        [Required]
        public int Number { get; set; } // Номер місця

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Free"; // Статус (Free / Occupied)

        public int? UserID { get; set; } // Зовнішній ключ на Attendant (може бути null)

        [ForeignKey("UserID")]
        public Attendant? Attendant { get; set; } // Працівник

        // Навігаційна властивість для бронювань
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
