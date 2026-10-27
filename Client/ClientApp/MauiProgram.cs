using Microsoft.Extensions.Logging;
using ClientApp.Services;
using ClientApp.ViewModels;
using ClientApp.Views;

namespace ClientApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Реєстрація сервісу взаємодії з REST API
		builder.Services.AddSingleton<HttpClient>();
		builder.Services.AddSingleton<ApiService>();

		// Реєстрація ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<ItemsViewModel>();
		builder.Services.AddTransient<ActionViewModel>();

		// Реєстрація Views
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<ItemsPage>();
		builder.Services.AddTransient<ActionPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
