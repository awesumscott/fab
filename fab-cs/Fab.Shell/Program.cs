using Fab.Core;
using Fab.Shell.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fab.Shell;

internal sealed class Program {
	static async Task Main(string[] args) {
		FabGlobal.BuildHost(builder => builder
			.ConfigureServices((context, services) => services
				.AddTransient<ListDbCommand>()
				.AddTransient<PopulateDbCommand>()
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

		var menu = new Menu("Fab CMS Console", new Dictionary<string, Func<ShellCommand>> {
			{ PopulateDbCommand.Name, FabGlobal.Services.GetRequiredService<PopulateDbCommand> },
			{ ListDbCommand.Name, FabGlobal.Services.GetRequiredService<ListDbCommand> },
		});
		await menu.Execute(default);

		await FabGlobal.DestroyHost();
	}
	internal static void Pause() {
		Console.WriteLine($"Press any key to continue...");
		Console.ReadKey(true);
	}
}
