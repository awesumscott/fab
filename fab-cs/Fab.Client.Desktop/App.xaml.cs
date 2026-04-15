using Fab.Client.Core;
using Fab.Client.Core.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;

namespace Fab.Client.Desktop;

public partial class App : Application {
	private IHost? _host;

	protected override async void OnStartup(StartupEventArgs e) {
		base.OnStartup(e);

		var builder = Host.CreateApplicationBuilder();
		builder.Configuration
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("fab.json", optional: true, reloadOnChange: true)
			.AddEnvironmentVariables();

		builder.Services.AddFabClient(builder.Configuration);
		builder.Services.AddSingleton<MainWindow>();

		_host = builder.Build();
		await _host.StartAsync();

		var mainWindow = _host.Services.GetRequiredService<MainWindow>();
		var viewModel = _host.Services.GetRequiredService<MainWindowViewModel>();
		mainWindow.DataContext = viewModel;
		Current.MainWindow = mainWindow;
		mainWindow.Show();

		_ = viewModel.LoadFirstArticleAsync();
	}

	protected override async void OnExit(ExitEventArgs e) {
		if (_host is not null) {
			using (_host)
				await _host.StopAsync(TimeSpan.FromSeconds(5));
		}
		base.OnExit(e);
	}
}
