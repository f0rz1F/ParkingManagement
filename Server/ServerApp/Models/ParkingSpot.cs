using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models
{
    // Паркувальне місце (Items)
    public class ParkingSpot
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int Number { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Free"; // Free, Occupied

        public int? UserID { get; set; }

        [ForeignKey("UserID")]
        public Attendant? Attendant { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
