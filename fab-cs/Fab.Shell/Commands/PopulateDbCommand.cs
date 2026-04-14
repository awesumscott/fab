using Fab.Core;
using Fab.Data;
using Microsoft.EntityFrameworkCore;

namespace Fab.Shell.Commands;

class PopulateDbCommand(IDbContextFactory<CmsWorkingDbContext> dbContextFactory) : ShellCommand {
	public new static string Name => "Populate Database";

	private readonly IDbContextFactory<CmsWorkingDbContext> _dbContextFactory = dbContextFactory;

	public override async Task Execute(CancellationToken cancellationToken) {
		Console.WriteLine("Populating Db");

		using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		await db.Database.EnsureDeletedAsync(cancellationToken);
		await db.Database.EnsureCreatedAsync(cancellationToken);
		//await db.Database.MigrateAsync(cancellationToken);

		try {
			db.Content.RemoveRange(db.Content);
			db.PublishedContent.RemoveRange(db.PublishedContent);
			db.Pages.RemoveRange(db.Pages);
			db.PublishedPages.RemoveRange(db.PublishedPages);
			await db.SaveChangesAsync(cancellationToken);
		} catch { }

		var cb = new Content[] {
			new() { Type = "text", Version = 0, Value = new TextSection() { Body = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur iaculis, purus et porttitor posuere, nulla sapien convallis nisi, nec imperdiet arcu lorem eu elit. Praesent accumsan luctus dolor eget condimentum. Nullam id dui sed velit pharetra ullamcorper eu non quam. Pellentesque efficitur ante nisi, in ullamcorper sem pellentesque ut. Maecenas blandit commodo aliquam. Curabitur blandit nisl nec diam lacinia eleifend. Praesent eu consectetur nunc, nec ultrices leo. Vestibulum quis felis quis purus eleifend tempus. Integer blandit mauris quis dictum interdum. Aenean lobortis ante eu urna suscipit condimentum. Nam efficitur quis leo et dapibus. Nam imperdiet odio nunc, at tincidunt ex fringilla vitae. Pellentesque tempor sollicitudin aliquet." } },
			new() { Type = "image", Version = 0, Value = new Image() { Url = "img/image0.png", Alt = "Sample image" } },
			new() { Type = "text", Version = 0, Value = new TextSection() { Body = "Suspendisse mattis efficitur ullamcorper. Donec eleifend eget ipsum quis pharetra. Nunc lobortis, elit a eleifend porttitor, velit magna iaculis magna, vel fringilla velit dui at nisi. Vivamus eleifend feugiat quam, in sagittis orci. Aenean a enim euismod, suscipit nisi porta, volutpat purus. Interdum et malesuada fames ac ante ipsum primis in faucibus. Nunc aliquet magna nec mi tristique luctus. Aenean vulputate elementum condimentum. Nulla ac erat nec arcu efficitur molestie sed et mi. Nunc scelerisque volutpat tempus. Aliquam fermentum porttitor elit, a pulvinar erat elementum nec." } },
		};
		await db.Pages.AddAsync(new() {
			Name = "Test Article 0",
			Type = "article",
			Contents = [.. cb],
			Value = new Article() {
				Children = [
					new LayoutNode() { Value = cb[0].Value },
					new LayoutNode() { Value = cb[1].Value },
					new LayoutNode() { Value = cb[2].Value },
				]
			}
		}, cancellationToken);
		await db.SaveChangesAsync(cancellationToken);

		Console.WriteLine();
		Program.Pause();
	}
}
