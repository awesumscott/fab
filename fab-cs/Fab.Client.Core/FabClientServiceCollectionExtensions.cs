using Fab.Client.Core.Configuration;
using Fab.Client.Core.Services;
using Fab.Client.Core.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fab.Client.Core;

public static class FabClientServiceCollectionExtensions {
	public static IServiceCollection AddFabClient(this IServiceCollection services, IConfiguration configuration) {
		services.AddOptions<FabClientOptions>()
			.Bind(configuration.GetSection(FabClientOptions.Section))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.AddHttpClient<IContentClient, HttpContentClient>((sp, client) => {
			var opts = sp.GetRequiredService<IOptions<FabClientOptions>>().Value;
			client.BaseAddress = new Uri(opts.BaseUrl.EndsWith('/') ? opts.BaseUrl : opts.BaseUrl + "/");
		});

		services.AddTransient<MainWindowViewModel>();

		return services;
	}
}
