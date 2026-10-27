using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ClientApp.Models;

namespace ClientApp.Services
{
    // Сервіс взаємодії MAUI застосунку з REST API сервером
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string TokenKey = "auth_jwt_token";
        private const string UserNameKey = "user_name";
        private const string UserEmailKey = "user_email";
        private const string UserIdKey = "user_id";

        public ApiService(HttpClient? httpClient = null)
        {
            // Для Android емулятора localhost доступний через 10.0.2.2, для Windows — через localhost
            var baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5088"
                : "http://localhost:5088";

            _httpClient = httpClient ?? new HttpClient();
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        // Авторизація користувача (POST: /api/auth/login)
        public async Task<(bool Success, string Message, UserSession? Session)> LoginAsync(string email, string password)
        {
            try
            {
                var payload = new { email, password };
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", payload);

                if (response.IsSuccessStatusCode)
                {
                    var session = await response.Content.ReadFromJsonAsync<UserSession>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (session != null && !string.IsNullOrEmpty(session.Token))
                    {
                        Preferences.Default.Set(TokenKey, session.Token);
                        Preferences.Default.Set(UserNameKey, session.Name);
                        Preferences.Default.Set(UserEmailKey, session.Email);
                        Preferences.Default.Set(UserIdKey, session.UserID);
                        return (true, "Успішний вхід", session);
                    }
                }

                return (false, "Невірний email або пароль", null);
            }
            catch (Exception ex)
            {
                return (false, $"Помилка з'єднання: {ex.Message}", null);
            }
        }

        // Отримання списку всіх паркомісць (GET: /api/items)
        public async Task<List<SpotItem>> GetSpotsAsync()
        {
            ApplyAuthorizationHeader();
            try
            {
                var spots = await _httpClient.GetFromJsonAsync<List<SpotItem>>("/api/items", new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return spots ?? new List<SpotItem>();
            }
            catch
            {
                return new List<SpotItem>();
            }
        }

        // Створення нового бронювання (POST: /api/actions)
        public async Task<(bool Success, string Message)> CreateReservationAsync(int spotId)
        {
            ApplyAuthorizationHeader();
            try
            {
                var payload = new { spotID = spotId };
                var response = await _httpClient.PostAsJsonAsync("/api/actions", payload);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Паркомісце успішно заброньовано!");
                }

                var errorText = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrEmpty(errorText) ? "Не вдалося забронювати місце" : errorText);
            }
            catch (Exception ex)
            {
                return (false, $"Помилка запиту: {ex.Message}");
            }
        }

        // Вихід із системи (очищення збереженого токена)
        public void Logout()
        {
            Preferences.Default.Remove(TokenKey);
            Preferences.Default.Remove(UserNameKey);
            Preferences.Default.Remove(UserEmailKey);
            Preferences.Default.Remove(UserIdKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        // Перевірка наявності збереженої авторизації
        public bool IsAuthenticated() => !string.IsNullOrEmpty(GetToken());

        public string GetToken() => Preferences.Default.Get(TokenKey, string.Empty);
        public string GetUserName() => Preferences.Default.Get(UserNameKey, "Користувач");

        private void ApplyAuthorizationHeader()
        {
            var token = GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
