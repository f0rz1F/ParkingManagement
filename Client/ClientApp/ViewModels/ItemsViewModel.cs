using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClientApp.Models;
using ClientApp.Services;

namespace ClientApp.ViewModels
{
    // ViewModel для сторінки перегляду списку паркомісць (ItemsPage)
    public partial class ItemsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        public ObservableCollection<SpotItem> Spots { get; } = new();

        // Кешування даних у пам'яті клієнта для оптимізації повторних переходів (Лабораторна 11)
        private static List<SpotItem>? _cachedSpots;
        private static DateTime _cacheExpiration = DateTime.MinValue;
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);

        public static void InvalidateCache()
        {
            _cachedSpots = null;
            _cacheExpiration = DateTime.MinValue;
        }

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string userName = string.Empty;

        [ObservableProperty]
        private string emptyMessage = string.Empty;

        public ItemsViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Завантаження списку паркомісць із сервера (з використанням кешу)
        [RelayCommand]
        public async Task LoadSpotsAsync()
        {
            if (IsBusy) return;

            UserName = _apiService.GetUserName();

            // Перевірка наявності валідного кешу (якщо не було жесту pull-to-refresh)
            if (!IsRefreshing && _cachedSpots != null && DateTime.UtcNow < _cacheExpiration)
            {
                var currentUserId = _apiService.GetUserId();
                Spots.Clear();
                foreach (var spot in _cachedSpots)
                {
                    spot.IsMine = (spot.UserID.HasValue && spot.UserID.Value == currentUserId);
                    Spots.Add(spot);
                }
                return;
            }

            IsBusy = true;
            EmptyMessage = string.Empty;

            try
            {
                var (list, errorMessage) = await _apiService.GetSpotsAsync();

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Spots.Clear();
                    EmptyMessage = errorMessage;

                    // Повернення до вікна логіну при втраті сесії
                    if (!_apiService.IsAuthenticated())
                    {
                        await Shell.Current.DisplayAlert("Авторизація", errorMessage, "OK");
                        await Shell.Current.GoToAsync("//LoginPage");
                    }
                    return;
                }

                // Збереження в кеш клієнта
                _cachedSpots = list;
                _cacheExpiration = DateTime.UtcNow.Add(_cacheDuration);

                var myUserId = _apiService.GetUserId();
                Spots.Clear();
                foreach (var spot in list)
                {
                    spot.IsMine = (spot.UserID.HasValue && spot.UserID.Value == myUserId);
                    Spots.Add(spot);
                }

                if (Spots.Count == 0)
                {
                    EmptyMessage = "Паркувальних місць не знайдено";
                }
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        // Скасування власного бронювання та звільнення місця
        [RelayCommand]
        private async Task CancelReservationAsync(SpotItem? spot)
        {
            if (spot == null || IsBusy) return;

            bool confirm = await Shell.Current.DisplayAlert("Звільнення місця", $"Ви бажаєте скасувати бронювання місця № {spot.Number}?", "Так, звільнити", "Ні");
            if (!confirm) return;

            IsBusy = true;
            try
            {
                var (success, message) = await _apiService.CancelReservationAsync(spot.ID);
                if (success)
                {
                    InvalidateCache();
                    await Shell.Current.DisplayAlert("Успіх", message, "OK");
                    await LoadSpotsAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Помилка", message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Перехід до екрану бронювання обраного місця (ActionPage)
        [RelayCommand]
        private async Task GoToReservationAsync(SpotItem? spot)
        {
            if (spot == null) return;

            if (!spot.IsFree)
            {
                await Shell.Current.DisplayAlert("Увага", "Це місце вже зайняте іншим авто", "OK");
                return;
            }

            var navigationParameter = new Dictionary<string, object>
            {
                { "SelectedSpot", spot }
            };

            await Shell.Current.GoToAsync("ActionPage", navigationParameter);
        }

        // Вихід із облікового запису
        [RelayCommand]
        private async Task LogoutAsync()
        {
            _apiService.Logout();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
