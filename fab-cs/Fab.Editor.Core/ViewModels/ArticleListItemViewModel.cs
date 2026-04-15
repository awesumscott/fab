using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class ArticleListItemViewModel : ObservableObject {
	public Article Article { get; }
	public int Id => Article.Id;

	[ObservableProperty] private string _title = string.Empty;
	[ObservableProperty] private bool _isDirty;

	public string DisplayTitle => IsDirty ? $"{Title} *" : Title;

	public ArticleListItemViewModel(Article article) {
		Article = article;
		RefreshTitle();
	}

	public void RefreshTitle() =>
		Title = string.IsNullOrWhiteSpace(Article.Title) ? "(untitled)" : Article.Title;

	partial void OnTitleChanged(string value) => OnPropertyChanged(nameof(DisplayTitle));
	partial void OnIsDirtyChanged(bool value) => OnPropertyChanged(nameof(DisplayTitle));
}
