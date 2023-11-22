using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;

namespace Fab.Client.Desktop.ViewModels;

partial class ArticleViewModel : ObservableObject, IFabRenderable {
	[ObservableProperty] private string _title;
	[ObservableProperty] private List<OrderedContentEntryViewModel> _entries;
	private Article? _article;

	public ArticleViewModel(Article? article) {
		_article = article;
		if (article is not null) {
			Title = _article.Title;
			Entries = _article.Entries.Select(x => new OrderedContentEntryViewModel(x)).ToList();
		}
	}
}
