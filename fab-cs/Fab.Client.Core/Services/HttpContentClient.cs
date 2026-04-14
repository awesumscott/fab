using System.Net;
using System.Net.Http.Json;
using Fab.Client.Core.Json;
using Fab.Data;

namespace Fab.Client.Core.Services;

public sealed class HttpContentClient(HttpClient httpClient) : IContentClient {
	public async Task<IReadOnlyList<Article>> GetArticlesAsync(bool includeAccessibility = true, CancellationToken cancellationToken = default) {
		var url = includeAccessibility ? "articles" : "articles?accessibility=false";
		var articles = await httpClient.GetFromJsonAsync<List<Article>>(url, FabClientJson.Default, cancellationToken);
		return articles ?? [];
	}

	public async Task<Article?> GetArticleAsync(int id, bool includeAccessibility = true, CancellationToken cancellationToken = default) {
		var url = includeAccessibility ? $"articles/{id}" : $"articles/{id}?accessibility=false";
		var response = await httpClient.GetAsync(url, cancellationToken);
		if (response.StatusCode == HttpStatusCode.NotFound) return null;
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<Article>(FabClientJson.Default, cancellationToken);
	}
}
