using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClientApp.Models;
using ClientApp.Services;

namespace ClientApp.ViewModels
{
    // ViewModel для створення дії бронювання (ActionPage)
    [QueryProperty(nameof(SelectedSpot), "SelectedSpot")]
    public partial class ActionViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private SpotItem? selectedSpot;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        public ActionViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Підтвердження та створення бронювання
        [RelayCommand]
        private async Task MakeReservationAsync()
        {
            if (SelectedSpot == null || IsBusy) return;

            IsBusy = true;
            StatusMessage = string.Empty;

            try
            {
                var (success, message) = await _apiService.CreateReservationAsync(SelectedSpot.ID);
                if (success)
                {
                    ItemsViewModel.InvalidateCache();
                    await Shell.Current.DisplayAlert("Успіх", $"Місце № {SelectedSpot.Number} успішно заброньовано!", "OK");
                    // Повернення до списку місць
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    StatusMessage = message;
                    await Shell.Current.DisplayAlert("Помилка", message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Скасування та повернення назад
        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
