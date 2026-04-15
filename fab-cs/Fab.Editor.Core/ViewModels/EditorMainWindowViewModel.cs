using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
	private readonly UndoService _undo = new();
	private CmsWorkingDbContext _db;

	public ObservableCollection<ArticleListItemViewModel> Articles { get; } = [];

	[ObservableProperty] private ArticleListItemViewModel? _selectedArticle;
	[ObservableProperty] private EditGenericModelViewModel? _editor;
	[ObservableProperty] private string? _status;
	[ObservableProperty] private bool _isBusy;
	[ObservableProperty] private string _title = "Fab CMS Editor";

	public bool HasUnsavedChanges => Articles.Any(a => a.IsDirty);
	public bool CanUndo => _undo.CanUndo;
	public bool CanRedo => _undo.CanRedo;

	public EditorMainWindowViewModel(IDbContextFactory<CmsWorkingDbContext> factory, IConfirmationService? confirmation = null) {
		_factory = factory;
		_confirmation = confirmation;
		_db = factory.CreateDbContext();
		Title = $"Fab CMS Editor — {_db.Database.GetConnectionString()}";
		Articles.CollectionChanged += OnArticlesChanged;
		_undo.Changed += (_, _) => {
			OnPropertyChanged(nameof(CanUndo));
			OnPropertyChanged(nameof(CanRedo));
			UndoCommand.NotifyCanExecuteChanged();
			RedoCommand.NotifyCanExecuteChanged();
		};
	}

	public async Task LoadAsync(CancellationToken cancellationToken = default) {
		IsBusy = true;
		try {
			await _db.Database.MigrateAsync(cancellationToken);
			var articles = await _db.Articles
				.Include(a => a.Entries).ThenInclude(e => e.Content)
				.ToListAsync(cancellationToken);

			foreach (var old in Articles) old.PropertyChanged -= OnArticleItemPropertyChanged;
			Articles.Clear();
			foreach (var article in articles) {
				var item = new ArticleListItemViewModel(article);
				item.PropertyChanged += OnArticleItemPropertyChanged;
				Articles.Add(item);
			}

			_undo.Clear();
			SelectedArticle = Articles.FirstOrDefault();
			OnPropertyChanged(nameof(HasUnsavedChanges));
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
		_undo.Clear();
		Editor = value is null
			? null
			: new EditGenericModelViewModel(value.Article, _confirmation, () => value.IsDirty = true, _undo);
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
			item.PropertyChanged += OnArticleItemPropertyChanged;
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
			foreach (var item in Articles) {
				item.RefreshTitle();
				item.IsDirty = false;
			}
			_undo.Clear();
			Status = $"Saved article #{SelectedArticle.Id}";
		}
		catch (Exception ex) {
			HandleFailure("Save", ex);
		}
		finally {
			IsBusy = false;
		}
	}

	[RelayCommand(CanExecute = nameof(CanUndo))]
	private void Undo() {
		_undo.Undo();
	}

	[RelayCommand(CanExecute = nameof(CanRedo))]
	private void Redo() {
		_undo.Redo();
	}

	public async Task<bool> ConfirmCloseAsync(CancellationToken cancellationToken = default) {
		if (!HasUnsavedChanges) return true;
		if (_confirmation is null) return true;

		var result = await _confirmation.PromptUnsavedAsync(
			"You have unsaved changes. Save before closing?");
		switch (result) {
			case UnsavedChangesResult.Save:
				await SaveAsync(cancellationToken);
				return !HasUnsavedChanges;
			case UnsavedChangesResult.Discard:
				return true;
			default:
				return false;
		}
	}

	private void OnArticleItemPropertyChanged(object? sender, PropertyChangedEventArgs e) {
		if (e.PropertyName == nameof(ArticleListItemViewModel.IsDirty))
			OnPropertyChanged(nameof(HasUnsavedChanges));
	}

	private void OnArticlesChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		OnPropertyChanged(nameof(HasUnsavedChanges));
	}

	private void HandleFailure(string operation, Exception ex) {
		Status = $"{operation} failed — reloading: {ex.Message}";
		try { _db.Dispose(); } catch { }
		_db = _factory.CreateDbContext();
		_undo.Clear();
		_ = LoadAsync();
	}

	public void Dispose() => _db.Dispose();
}
