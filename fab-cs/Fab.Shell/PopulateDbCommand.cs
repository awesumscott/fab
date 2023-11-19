using Fab.Core;
using Fab.Data;
using Fab.Shell.Commands;
using Microsoft.EntityFrameworkCore;

namespace Fab.Shell;

class PopulateDbCommand(IDbContextFactory<CmsWorkingDbContext> dbContextFactory) : ShellCommand {
	public new static string Name => "Populate Database";

	private IDbContextFactory<CmsWorkingDbContext> _dbContextFactory = dbContextFactory;

	public override async Task Execute(CancellationToken cancellationToken) {
		Console.WriteLine("Populating Db");

		using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		await db.Database.EnsureCreatedAsync(cancellationToken);
		//await db.Database.MigrateAsync(cancellationToken);

		try {
			db.Articles.RemoveRange(db.Articles);
			db.Images.RemoveRange(db.Images);
			db.Paragraphs.RemoveRange(db.Paragraphs);
			await db.SaveChangesAsync(cancellationToken);
		} catch { }

		await db.Articles.AddAsync(new Article() {
			Title = "Test Article 0",
			Entries = [
				new(0, new Paragraph() { Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur iaculis, purus et porttitor posuere, nulla sapien convallis nisi, nec imperdiet arcu lorem eu elit. Praesent accumsan luctus dolor eget condimentum. Nullam id dui sed velit pharetra ullamcorper eu non quam. Pellentesque efficitur ante nisi, in ullamcorper sem pellentesque ut. Maecenas blandit commodo aliquam. Curabitur blandit nisl nec diam lacinia eleifend. Praesent eu consectetur nunc, nec ultrices leo. Vestibulum quis felis quis purus eleifend tempus. Integer blandit mauris quis dictum interdum. Aenean lobortis ante eu urna suscipit condimentum. Nam efficitur quis leo et dapibus. Nam imperdiet odio nunc, at tincidunt ex fringilla vitae. Pellentesque tempor sollicitudin aliquet." }),
				new(1, new Image() { Url = "img/image0.png", Alt = "Sample image" }),
				new(2, new Paragraph() { Text = "Suspendisse mattis efficitur ullamcorper. Donec eleifend eget ipsum quis pharetra. Nunc lobortis, elit a eleifend porttitor, velit magna iaculis magna, vel fringilla velit dui at nisi. Vivamus eleifend feugiat quam, in sagittis orci. Aenean a enim euismod, suscipit nisi porta, volutpat purus. Interdum et malesuada fames ac ante ipsum primis in faucibus. Nunc aliquet magna nec mi tristique luctus. Aenean vulputate elementum condimentum. Nulla ac erat nec arcu efficitur molestie sed et mi. Nunc scelerisque volutpat tempus. Aliquam fermentum porttitor elit, a pulvinar erat elementum nec." })
			]
		}, cancellationToken);
		await db.SaveChangesAsync(cancellationToken);

		Console.WriteLine();
		Program.Pause();
    }
}
