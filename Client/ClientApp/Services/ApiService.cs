using ClientApp.Models;

namespace ClientApp.Services
{
    // Сервіс для підготовки взаємодії з REST API (макет для перевірки MVVM)
    public class ApiService
    {
        private string _userName = "Іван Петренко";

        // Перевірка авторизації користувача
        public async Task<(bool Success, string Message, UserSession? Session)> LoginAsync(string email, string password)
        {
            await Task.Delay(100);
            return (true, "Успішний вхід", new UserSession { Token = "mock_token", Name = _userName, Email = email, Role = "User" });
        }

        // Отримання тестового списку паркомісць
        public async Task<List<SpotItem>> GetSpotsAsync()
        {
            await Task.Delay(100);
            return new List<SpotItem>
            {
                new SpotItem { ID = 1, Number = 101, Status = "Free" },
                new SpotItem { ID = 2, Number = 102, Status = "Occupied", AttendantName = "Іван Петренко" },
                new SpotItem { ID = 3, Number = 103, Status = "Free" }
            };
        }

        // Створення нового бронювання
        public async Task<(bool Success, string Message)> CreateReservationAsync(int spotId)
        {
            await Task.Delay(100);
            return (true, "Паркомісце успішно заброньовано");
        }

        // Вихід із системи
        public void Logout() { }

        public bool IsAuthenticated() => true;
        public string GetUserName() => _userName;
    }
}
