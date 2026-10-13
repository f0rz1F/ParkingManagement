using System.ComponentModel.DataAnnotations;

namespace ServerApp.Models
{
    // Працівник парковки (Users)
    public class Attendant
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public ICollection<ParkingSpot> ParkingSpots { get; set; } = new List<ParkingSpot>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
