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

        // Завантаження списку паркомісць із сервера
        [RelayCommand]
        public async Task LoadSpotsAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            EmptyMessage = string.Empty;
            UserName = _apiService.GetUserName();

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

                Spots.Clear();
                foreach (var spot in list)
                {
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
