using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fab.Core;
using Fab.Data;
using Microsoft.EntityFrameworkCore;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class EditorMainWindowViewModel : ObservableObject, IDisposable {
	private readonly CmsWorkingDbContext _db;

	public ObservableCollection<ArticleListItemViewModel> Articles { get; } = [];

	[ObservableProperty] private ArticleListItemViewModel? _selectedArticle;
	[ObservableProperty] private EditGenericModelViewModel? _editor;
	[ObservableProperty] private string? _status;
	[ObservableProperty] private bool _isBusy;
	[ObservableProperty] private string _title = "Fab CMS Editor";

	public EditorMainWindowViewModel(IDbContextFactory<CmsWorkingDbContext> factory) {
		_db = factory.CreateDbContext();
		Title = $"Fab CMS Editor — {_db.Database.GetConnectionString()}";
	}

	public async Task LoadAsync(CancellationToken cancellationToken = default) {
		IsBusy = true;
		try {
			await _db.Database.MigrateAsync(cancellationToken);
			var articles = await _db.Articles
				.Include(a => a.Entries).ThenInclude(e => e.Content)
				.ToListAsync(cancellationToken);

			Articles.Clear();
			foreach (var article in articles)
				Articles.Add(new ArticleListItemViewModel(article));

			SelectedArticle = Articles.FirstOrDefault();
			Status = Articles.Count == 0
				? "No articles in database"
				: $"Loaded {Articles.Count} article(s)";
		}
		catch (Exception ex) {
			Status = $"Load failed: {ex.Message}";
			Articles.Clear();
			Editor = null;
		}
		finally {
			IsBusy = false;
		}
	}

	partial void OnSelectedArticleChanged(ArticleListItemViewModel? value) {
		Editor = value is null ? null : new EditGenericModelViewModel(value.Article);
	}

	[RelayCommand]
	private async Task NewArticleAsync(CancellationToken cancellationToken) {
		IsBusy = true;
		try {
			await _db.Database.MigrateAsync(cancellationToken);
			var article = new Article { Title = "Untitled" };
			_db.Articles.Add(article);
			await _db.SaveChangesAsync(cancellationToken);

			var item = new ArticleListItemViewModel(article);
			Articles.Add(item);
			SelectedArticle = item;
			Status = $"Created article #{article.Id}";
		}
		catch (Exception ex) {
			Status = $"Create failed: {ex.Message}";
		}
		finally {
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task SaveAsync(CancellationToken cancellationToken) {
		if (SelectedArticle is null) return;
		IsBusy = true;
		try {
			await _db.SaveChangesAsync(cancellationToken);
			SelectedArticle.RefreshTitle();
			Status = $"Saved article #{SelectedArticle.Id}";
		}
		catch (Exception ex) {
			Status = $"Save failed: {ex.Message}";
		}
		finally {
			IsBusy = false;
		}
	}

	public void Dispose() => _db.Dispose();
}
