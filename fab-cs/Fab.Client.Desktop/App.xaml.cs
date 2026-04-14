using Fab.Client.Desktop.ViewModels;
using Fab.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Fab.Client.Desktop;

public partial class App : Application {
	private IHost? _host;

	protected override async void OnStartup(StartupEventArgs e) {
		base.OnStartup(e);

		var builder = Host.CreateApplicationBuilder();
		builder.ConfigureFab();
		builder.Services.AddFabDatabaseFactory(builder.Configuration);
		builder.Services.AddTransient<TestService>();
		builder.Services.AddTransient<MainWindowViewModel>();
		builder.Services.AddSingleton<MainWindow>();
		builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

		_host = builder.Build();
		await _host.StartAsync();

		var mainWindow = _host.Services.GetRequiredService<MainWindow>();
		mainWindow.DataContext = _host.Services.GetRequiredService<MainWindowViewModel>();
		Current.MainWindow = mainWindow;
		mainWindow.Show();
	}

	protected override async void OnExit(ExitEventArgs e) {
		if (_host is not null) {
			using (_host)
				await _host.StopAsync(TimeSpan.FromSeconds(5));
		}
		base.OnExit(e);
	}
}
