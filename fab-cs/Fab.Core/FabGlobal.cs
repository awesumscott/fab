using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fab.Core;

public static class FabGlobal {
	private static IHost _host;
	public static IServiceProvider Services { get; private set; }

	public static void BuildHost(Action<IHostBuilder> action) {
		var builder = Host.CreateDefaultBuilder()
			.ConfigureFab();
		action?.Invoke(builder);
		_host = builder.Build();
		Services = _host.Services;
	}

	public static async Task StartHost() => await _host.StartAsync();
	public static async Task DestroyHost() {
		using (_host)
			await _host.StopAsync(TimeSpan.FromSeconds(5));
	}

	public static IHostApplicationBuilder ConfigureFab(this IHostApplicationBuilder builder) {
		builder.Configuration
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("fab.json", false, true);
		builder.Services
			.AddDbContext<CmsWorkingDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DbConnection")));
		//.AddDbContextFactory<CmsWorkingDbContext>();
		return builder;
	}
	public static IHostBuilder ConfigureFab(this IHostBuilder builder) {
		return builder
			.ConfigureAppConfiguration(config => config
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("fab.json", false, true)
			)
			.ConfigureServices((context, services) => services
				.AddDbContext<CmsWorkingDbContext>(options => options.UseSqlite(context.Configuration.GetConnectionString("DbConnection")))
				.AddDbContextFactory<CmsWorkingDbContext>()
			);
	}
}