using Microsoft.Extensions.DependencyInjection;

namespace ClientApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Застосунок завжди працює в темній темі (чорний / сірий / білий)
		UserAppTheme = AppTheme.Dark;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}