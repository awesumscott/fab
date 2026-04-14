using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;

namespace Fab.Client.Core.ViewModels;

public partial class ArticleViewModel : ObservableObject, IFabRenderable {
	[ObservableProperty] private string _title = string.Empty;
	[ObservableProperty] private List<OrderedContentEntryViewModel> _entries = [];

	public ArticleViewModel(Article article) {
		Title = article.Title;
		Entries = article.Entries
			.OrderBy(e => e.Order)
			.Select(e => new OrderedContentEntryViewModel(e))
			.ToList();
	}
}
