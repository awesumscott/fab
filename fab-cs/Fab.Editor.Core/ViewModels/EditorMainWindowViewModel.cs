using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fab.Core;
using Fab.Data;
using Fab.Editor.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class EditorMainWindowViewModel : ObservableObject, IDisposable {
	private readonly IDbContextFactory<CmsWorkingDbContext> _factory;
	private readonly IConfirmationService? _confirmation;
	private CmsWorkingDbContext _db;

	public ObservableCollection<ArticleListItemViewModel> Articles { get; } = [];

	[ObservableProperty] private ArticleListItemViewModel? _selectedArticle;
	[ObservableProperty] private EditGenericModelViewModel? _editor;
	[ObservableProperty] private string? _status;
	[ObservableProperty] private bool _isBusy;
	[ObservableProperty] private string _title = "Fab CMS Editor";

	public EditorMainWindowViewModel(IDbContextFactory<CmsWorkingDbContext> factory, IConfirmationService? confirmation = null) {
		_factory = factory;
		_confirmation = confirmation;
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
			HandleFailure("Load", ex);
		}
		finally {
			IsBusy = false;
		}
	}

	partial void OnSelectedArticleChanged(ArticleListItemViewModel? value) {
		Editor = value is null ? null : new EditGenericModelViewModel(value.Article, _confirmation);
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
			HandleFailure("Create", ex);
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
			HandleFailure("Save", ex);
		}
		finally {
			IsBusy = false;
		}
	}

	private void HandleFailure(string operation, Exception ex) {
		Status = $"{operation} failed — reloading: {ex.Message}";
		// EF can leave the context in an unrecoverable state after a failed
		// SaveChanges/query. Drop it and rebuild so the next operation has
		// a clean slate, then re-Load so the UI reflects persisted state
		// instead of stale references to the disposed context's entities.
		// Unsaved in-memory edits are intentionally lost — recovering them
		// would require re-applying field-by-field against a fresh graph.
		try { _db.Dispose(); } catch { }
		_db = _factory.CreateDbContext();
		_ = LoadAsync();
	}

	public void Dispose() => _db.Dispose();
}
