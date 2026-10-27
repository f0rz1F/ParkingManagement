using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClientApp.Services;

namespace ClientApp.ViewModels
{
    // ViewModel для сторінки авторизації (LoginPage)
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string email = "ivan@parking.com";

        [ObservableProperty]
        private string password = "Password123!";

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public LoginViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Команда виконання логіну
        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Будь ласка, введіть email та пароль";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var (success, message, _) = await _apiService.LoginAsync(Email, Password);
                if (success)
                {
                    // Перехід до головного списку паркомісць
                    await Shell.Current.GoToAsync("//ItemsPage");
                }
                else
                {
                    ErrorMessage = message;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
