using Fab.Client.Desktop.ViewModels;
using Fab.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Fab.Client.Desktop;

public partial class App : Application {
	protected override async void OnStartup(StartupEventArgs e) {
		FabGlobal.BuildHost(builder => builder
			.ConfigureServices((context, services) => services
				.AddTransient<TestService>()
				.AddTransient<MainWindowViewModel>()
			)
			.ConfigureLogging((context, logging) => {
				var env = context.HostingEnvironment;
				var config = context.Configuration.GetSection("Logging");
				// ...
				logging.AddConfiguration(config);
				logging.AddConsole();
				// ...
				logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
			})
		);
		await FabGlobal.StartHost();

		Current.MainWindow = new MainWindow {
			DataContext = FabGlobal.Services.GetService<MainWindowViewModel>()
		};
		Current.MainWindow.Show();
	}

	protected override async void OnExit(ExitEventArgs e) {
		await FabGlobal.DestroyHost();
		base.OnExit(e);
	}
}
