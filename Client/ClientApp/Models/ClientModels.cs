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

        public Color StatusBadgeColor
        {
            get
            {
                if (IsMine) return Color.FromArgb("#0d6efd"); // Синій бейдж для свого місця
                return IsFree ? Color.FromArgb("#198754") : Color.FromArgb("#dc3545"); // Зелений або червоний
            }
        }

        public Color CardBackgroundColor
        {
            get
            {
                if (IsMine) return Color.FromArgb("#eff6ff"); // Світло-блакитний фон
                return IsFree ? Color.FromArgb("#f8fff9") : Color.FromArgb("#fff8f8");
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
