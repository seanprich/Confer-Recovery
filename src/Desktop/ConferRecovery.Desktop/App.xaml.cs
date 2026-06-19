using ConferRecovery.Desktop.Application.DependencyInjection;
using ConferRecovery.Desktop.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace ConferRecovery.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
	private ServiceProvider? _services;

	protected override void OnStartup(System.Windows.StartupEventArgs e)
	{
		base.OnStartup(e);

		_services = ConfigureServices();

		var window = _services.GetRequiredService<MainWindow>();
		window.Show();
	}

	protected override void OnExit(System.Windows.ExitEventArgs e)
	{
		base.OnExit(e);
		_services?.Dispose();
	}

	private static ServiceProvider ConfigureServices()
	{
		var services = new ServiceCollection();
		var apiBaseUrl = Environment.GetEnvironmentVariable("CONFER_API_BASE_URL")
			?? "http://localhost:5132/";

		services
			.AddDesktopApplication()
			.AddDesktopInfrastructure(apiBaseUrl);

		services.AddSingleton<MainWindow>();

		return services.BuildServiceProvider();
	}
}

