using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Fab.Data;

namespace Fab.Host.Json;

public static class FabJson {
	public static readonly JsonSerializerOptions WithAccessibility = Build(includeAccessibility: true);
	public static readonly JsonSerializerOptions WithoutAccessibility = Build(includeAccessibility: false);

	public static JsonSerializerOptions ForRequest(bool includeAccessibility) =>
		includeAccessibility ? WithAccessibility : WithoutAccessibility;

	private static JsonSerializerOptions Build(bool includeAccessibility) {
		var options = new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true,
		};

		if (!includeAccessibility) {
			var resolver = new DefaultJsonTypeInfoResolver();
			resolver.Modifiers.Add(typeInfo => {
				foreach (var prop in typeInfo.Properties) {
					var attrs = prop.AttributeProvider?.GetCustomAttributes(typeof(AccessibilityAttribute), inherit: true);
					if (attrs is { Length: > 0 })
						prop.ShouldSerialize = (_, _) => false;
				}
			});
			options.TypeInfoResolver = resolver;
		}

		return options;
	}
}
