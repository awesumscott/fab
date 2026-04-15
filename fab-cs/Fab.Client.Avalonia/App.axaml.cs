using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Fab.Client.Core;
using Fab.Client.Core.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace Fab.Client.Avalonia;

public partial class App : Application {
	private IHost? _host;

	public override void Initialize() => AvaloniaXamlLoader.Load(this);

	public override async void OnFrameworkInitializationCompleted() {
		var builder = Host.CreateApplicationBuilder();
		builder.Configuration
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("fab.json", optional: true, reloadOnChange: true);

		builder.Services.AddFabClient(builder.Configuration);
		builder.Services.AddSingleton<MainWindow>();

		_host = builder.Build();
		await _host.StartAsync();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			var window = _host.Services.GetRequiredService<MainWindow>();
			var viewModel = _host.Services.GetRequiredService<MainWindowViewModel>();
			window.DataContext = viewModel;
			desktop.MainWindow = window;
			desktop.ShutdownRequested += OnShutdownRequested;
			_ = viewModel.LoadFirstArticleAsync();
		}

		base.OnFrameworkInitializationCompleted();
	}

	private async void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e) {
		if (_host is not null) {
			using (_host)
				await _host.StopAsync(TimeSpan.FromSeconds(5));
		}
	}
}
