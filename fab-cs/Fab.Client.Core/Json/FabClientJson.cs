using System.Text.Json;

namespace Fab.Client.Core.Json;

public static class FabClientJson {
	public static readonly JsonSerializerOptions Default = new() {
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true,
	};
}
