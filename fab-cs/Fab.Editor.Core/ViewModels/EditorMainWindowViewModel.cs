using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fab.Core;
using Fab.Data;
using Microsoft.EntityFrameworkCore;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class EditorMainWindowViewModel : ObservableObject, IDisposable {
	private readonly CmsWorkingDbContext _db;
	private Article? _article;

	[ObservableProperty] private EditGenericModelViewModel? _editor;
	[ObservableProperty] private string? _status;
	[ObservableProperty] private bool _isBusy;
	[ObservableProperty] private string _title = "Fab CMS Editor";

	public EditorMainWindowViewModel(IDbContextFactory<CmsWorkingDbContext> factory) {
		_db = factory.CreateDbContext();
		Title = $"Fab CMS Editor — {_db.Database.GetConnectionString()}";
	}

	public async Task LoadFirstArticleAsync(CancellationToken cancellationToken = default) {
		IsBusy = true;
		try {
			await _db.Database.EnsureCreatedAsync(cancellationToken);
			_article = await _db.Articles
				.Include(a => a.Entries).ThenInclude(e => e.Content)
				.FirstOrDefaultAsync(cancellationToken);
			Editor = _article is null ? null : new EditGenericModelViewModel(_article);
			Status = _article is null ? "No articles in database" : $"Loaded article #{_article.Id}";
		}
		catch (Exception ex) {
			Status = $"Load failed: {ex.Message}";
			Editor = null;
		}
		finally {
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task SaveAsync(CancellationToken cancellationToken) {
		if (_article is null) return;
		IsBusy = true;
		try {
			await _db.SaveChangesAsync(cancellationToken);
			Status = $"Saved article #{_article.Id}";
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
