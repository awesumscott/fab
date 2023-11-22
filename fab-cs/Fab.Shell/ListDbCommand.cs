using Fab.Core;
using Fab.Data;
using Fab.Shell.Commands;
using Microsoft.EntityFrameworkCore;

namespace Fab.Shell;

class ListDbCommand : ShellCommand {

	public new static string Name => "List Database Contents";

	private IDbContextFactory<CmsWorkingDbContext> _dbContextFactory;

	public ListDbCommand(IDbContextFactory<CmsWorkingDbContext> dbContextFactory) => _dbContextFactory = dbContextFactory;

	public override async Task Execute(CancellationToken cancellationToken) {
		Console.WriteLine("Db Contents:");

		using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var articles = await db.Articles
			.Include(x => x.Entries)
			.ThenInclude(x => x.Content)
			.ToListAsync(cancellationToken);//.RemoveRange(db.Movies);
		foreach (var article in articles) {
			Console.WriteLine($"{article.Title} - {article.Entries.Count}");
			foreach (var entry in article.Entries) {
				switch (entry.Content) {
					case Paragraph p:
						Console.WriteLine($"\t{p.Text}");
						break;
					case Image i:
						Console.WriteLine($"\tImage(");
						Console.WriteLine($"\t\tURL: {i.Url}");
						Console.WriteLine($"\t\tAlt: \"{i.Alt}\"");
						Console.WriteLine($"\t)");
						break;
					default: break;
				}
			}
		}

		Console.WriteLine();
		Program.Pause();
    }
}
