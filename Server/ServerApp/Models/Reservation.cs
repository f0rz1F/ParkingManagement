using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models
{
    // Бронювання (Actions)
    public class Reservation
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int AttendantID { get; set; }

        [Required]
        public int SpotID { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Completed, Cancelled

        [ForeignKey("AttendantID")]
        public Attendant? Attendant { get; set; }

        [ForeignKey("SpotID")]
        public ParkingSpot? ParkingSpot { get; set; }
    }
}
