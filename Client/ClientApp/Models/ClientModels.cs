namespace ClientApp.Models
{
    // Модель паркомісця для MAUI застосунку (Items)
    public class SpotItem
    {
        public int ID { get; set; }
        public int Number { get; set; }
        public string Status { get; set; } = "Free";
        public int? UserID { get; set; }
        public string? AttendantName { get; set; }
        public bool IsMine { get; set; }

        public bool IsFree => Status == "Free";
        public bool HasAttendant => !string.IsNullOrEmpty(AttendantName);

        public string DisplayStatus
        {
            get
            {
                if (IsMine) return "Заброньовано вами";
                return IsFree ? "Вільне" : "Зайняте";
            }
        }

        // Колір тексту статусу (на темному фоні)
        public Color StatusBadgeColor
        {
            get
            {
                if (IsMine) return Colors.White;                     // Своє місце — білий
                return IsFree ? Color.FromArgb("#4ADE80")            // Вільне — світло-зелений
                              : Color.FromArgb("#F87171");           // Зайняте — світло-червоний
            }
        }

        // Фон картки місця (темна монохромна гама)
        public Color CardBackgroundColor
        {
            get
            {
                if (IsMine) return Color.FromArgb("#262626"); // Своє місце — трохи світліше
                return IsFree ? Color.FromArgb("#161616")     // Вільне
                              : Color.FromArgb("#121212");    // Зайняте — темніше
            }
        }
    }

    // Модель результату бронювання (Actions)
    public class ReservationItem
    {
        public int ID { get; set; }
        public int SpotID { get; set; }
        public int SpotNumber { get; set; }
        public int AttendantID { get; set; }
        public string AttendantName { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // Модель відповіді сервера при авторизації
    public class UserSession
    {
        public string Token { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
