using Fab.Data;

namespace Fab.Client.Core.Services;

public interface IContentClient {
	Task<IReadOnlyList<Article>> GetArticlesAsync(bool includeAccessibility = true, CancellationToken cancellationToken = default);
	Task<Article?> GetArticleAsync(int id, bool includeAccessibility = true, CancellationToken cancellationToken = default);
}
