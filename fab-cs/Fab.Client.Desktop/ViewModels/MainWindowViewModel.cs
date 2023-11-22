using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Core;
using Microsoft.EntityFrameworkCore;

namespace Fab.Client.Desktop.ViewModels;

partial class MainWindowViewModel : ObservableObject {
	[ObservableProperty] private ArticleViewModel _article;

	private IDbContextFactory<CmsWorkingDbContext> _dbContextFactory;

	public MainWindowViewModel(IDbContextFactory<CmsWorkingDbContext> dbContextFactory) {
		_dbContextFactory = dbContextFactory;

		using var db = _dbContextFactory.CreateDbContext();
		_article = new ArticleViewModel(db.Articles.Include(x => x.Entries).ThenInclude(x => x.Content).FirstOrDefault());
		
	}
}
