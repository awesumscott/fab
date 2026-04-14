using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Client.Core.Services;

namespace Fab.Client.Core.ViewModels;

public partial class MainWindowViewModel : ObservableObject {
	[ObservableProperty] private ArticleViewModel? _article;
	[ObservableProperty] private bool _isLoading;
	[ObservableProperty] private string? _errorMessage;

	private readonly IContentClient _client;

	public MainWindowViewModel(IContentClient client) {
		_client = client;
	}

	public async Task LoadFirstArticleAsync(CancellationToken cancellationToken = default) {
		IsLoading = true;
		ErrorMessage = null;
		try {
			var articles = await _client.GetArticlesAsync(cancellationToken: cancellationToken);
			var first = articles.FirstOrDefault();
			Article = first is null ? null : new ArticleViewModel(first);
		}
		catch (Exception ex) {
			ErrorMessage = ex.Message;
			Article = null;
		}
		finally {
			IsLoading = false;
		}
	}
}
