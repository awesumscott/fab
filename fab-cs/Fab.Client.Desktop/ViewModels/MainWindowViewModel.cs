using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Client.Desktop.ViewModels.Browse;
using Fab.Client.Desktop.ViewModels.Edit;
using Fab.Core;
using Microsoft.EntityFrameworkCore;

namespace Fab.Client.Desktop.ViewModels;

partial class MainWindowViewModel : ObservableObject {
	[ObservableProperty] private ArticleViewModel _article;
	[ObservableProperty] private EditGenericModelViewModel _editThing;

	private readonly IDbContextFactory<CmsWorkingDbContext> _dbContextFactory;

	public MainWindowViewModel(IDbContextFactory<CmsWorkingDbContext> dbContextFactory) {
		_dbContextFactory = dbContextFactory;

		//Loading directly from the database here for debugging, but I think it should still
		//be included later even after HTTP is added, for working on sites locally
		using var db = _dbContextFactory.CreateDbContext();
		var article = db.Pages.FirstOrDefault(); //.Articles.Include(x => x.Entries).ThenInclude(x => x.Content).FirstOrDefault();
		//Article = new ArticleViewModel(article);
		EditThing = new EditGenericModelViewModel(article);
	}
}
