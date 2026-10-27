namespace ServerApp.Models.DTOs
{
    // DTO для перегляду паркомісця (Items)
    public class SpotDto
    {
        public int ID { get; set; }
        public int Number { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? UserID { get; set; }
        public string? AttendantName { get; set; }
    }

    // DTO для створення нового бронювання (Actions)
    public class CreateReservationDto
    {
        public int SpotID { get; set; }
    }

    // DTO результату бронювання (Actions)
    public class ReservationDto
    {
        public int ID { get; set; }
        public int SpotID { get; set; }
        public int SpotNumber { get; set; }
        public int AttendantID { get; set; }
        public string AttendantName { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // DTO для запиту авторизації користувача через API
    public class UserLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // DTO відповіді успішної авторизації
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
