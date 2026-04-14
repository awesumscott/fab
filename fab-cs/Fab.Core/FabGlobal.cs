using Fab.Core.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Fab.Core;

public static class FabGlobal {
	private static IHost? _host;
	public static IServiceProvider Services { get; private set; } = null!;

	public static void BuildHost(Action<IHostBuilder> action) {
		var builder = Host.CreateDefaultBuilder()
			.ConfigureFab();
		action?.Invoke(builder);
		_host = builder.Build();
		Services = _host.Services;
	}

	public static async Task StartHost() => await _host!.StartAsync();
	public static async Task DestroyHost() {
		using (_host)
			await _host!.StopAsync(TimeSpan.FromSeconds(5));
	}

	public static IHostApplicationBuilder ConfigureFab(this IHostApplicationBuilder builder) {
		builder.Configuration
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("fab.json", false, true);
		return builder;
	}

	public static IHostBuilder ConfigureFab(this IHostBuilder builder) {
		return builder.ConfigureAppConfiguration(config => config
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("fab.json", false, true)
		);
	}

	public static IServiceCollection AddFabDatabase(this IServiceCollection services, IConfiguration configuration) {
		BindFabDatabaseOptions(services, configuration);
		services.AddDbContext<CmsWorkingDbContext>((sp, options) => {
			var dbOptions = sp.GetRequiredService<IOptions<FabDatabaseOptions>>().Value;
			options.UseSqlite(dbOptions.ConnectionString);
		});
		return services;
	}

	public static IServiceCollection AddFabDatabaseFactory(this IServiceCollection services, IConfiguration configuration) {
		BindFabDatabaseOptions(services, configuration);
		services.AddDbContextFactory<CmsWorkingDbContext>((sp, options) => {
			var dbOptions = sp.GetRequiredService<IOptions<FabDatabaseOptions>>().Value;
			options.UseSqlite(dbOptions.ConnectionString);
		});
		return services;
	}

	private static void BindFabDatabaseOptions(IServiceCollection services, IConfiguration configuration) {
		services.AddOptions<FabDatabaseOptions>()
			.Bind(configuration.GetSection(FabDatabaseOptions.Section))
			.ValidateDataAnnotations()
			.ValidateOnStart();
	}
}
