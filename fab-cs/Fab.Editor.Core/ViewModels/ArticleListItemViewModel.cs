using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class ArticleListItemViewModel : ObservableObject {
	public Article Article { get; }
	public int Id => Article.Id;

	[ObservableProperty] private string _title = string.Empty;

	public ArticleListItemViewModel(Article article) {
		Article = article;
		RefreshTitle();
	}

	public void RefreshTitle() =>
		Title = string.IsNullOrWhiteSpace(Article.Title) ? "(untitled)" : Article.Title;
}
