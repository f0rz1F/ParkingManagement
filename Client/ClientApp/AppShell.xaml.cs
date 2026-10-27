using ClientApp.Views;

namespace ClientApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Реєстрація маршруту переходу до сторінки створення бронювання
        Routing.RegisterRoute(nameof(ActionPage), typeof(ActionPage));
    }
}
