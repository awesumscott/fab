using Fab.Core;
using Fab.Editor.Core.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fab.Editor.Core;

public static class FabEditorServiceCollectionExtensions {
	public static IServiceCollection AddFabEditor(this IServiceCollection services, IConfiguration configuration) {
		services.AddFabDatabaseFactory(configuration);
		services.AddSingleton<EditorMainWindowViewModel>();
		return services;
	}
}
