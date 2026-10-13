using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models
{
    // Модель бронювання (таблиця Reservations)
    public class Reservation
    {
        [Key]
        public int ID { get; set; } // Первинний ключ

        [Required]
        public int AttendantID { get; set; } // Зовнішній ключ на Attendant

        [Required]
        public int SpotID { get; set; } // Зовнішній ключ на ParkingSpot

        [Required]
        public DateTime ReservationDate { get; set; } // Дата бронювання

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Статус (Active / Completed / Cancelled)

        // Навігаційні властивості
        [ForeignKey("AttendantID")]
        public Attendant? Attendant { get; set; } // Працівник

        [ForeignKey("SpotID")]
        public ParkingSpot? ParkingSpot { get; set; } // Паркувальне місце
    }
}
