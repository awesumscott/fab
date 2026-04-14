using Fab.Data;
using Microsoft.EntityFrameworkCore;

namespace Fab.Core;

public class CmsWorkingDbContext(DbContextOptions<CmsWorkingDbContext> options) : DbContext(options) {


	public DbSet<Content> Content => Set<Content>();
	public DbSet<PublishedContent> PublishedContent => Set<PublishedContent>();
	public DbSet<Page> Pages => Set<Page>();
	public DbSet<PublishedPage> PublishedPages => Set<PublishedPage>();







	//[Obsolete] public virtual DbSet<Article> Articles => Set<Article>();
	//[Obsolete] public virtual DbSet<TextSection> Paragraphs => Set<TextSection>();
	//[Obsolete] public virtual DbSet<Image> Images => Set<Image>();

	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
	//	optionsBuilder.UseSqlite($"Data Source={AppDomain.CurrentDomain.SetupInformation.ApplicationBase}fab.db");
	//}

	//protected override void OnModelCreating(ModelBuilder modelBuilder) {
	//	modelBuilder.Entity<ContentBase>().ToTable("ContentBases");
	//	modelBuilder.Entity<Paragraph>().ToTable(nameof(Paragraphs));
	//	modelBuilder.Entity<Image>().ToTable(nameof(Images));
	//}



	//This shit might not work. I think it'll be necessary to use a JsonSerializer and convert to string properties directly.
	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		modelBuilder.Entity<Content>().OwnsOne(
			content => content.Value, ownedNavigationBuilder => {
				ownedNavigationBuilder.ToJson();
				//ownedNavigationBuilder.OwnsOne(contactDetails => contactDetails.Address);
			});
		modelBuilder.Entity<PublishedContent>().OwnsOne(
			content => content.Value, ownedNavigationBuilder => {
				ownedNavigationBuilder.ToJson();
			});
		modelBuilder.Entity<Page>().OwnsOne(
			content => content.Value, ownedNavigationBuilder => {
				ownedNavigationBuilder.ToJson();
			});
		modelBuilder.Entity<PublishedPage>().OwnsOne(
			content => content.Value, ownedNavigationBuilder => {
				ownedNavigationBuilder.ToJson();
			});
	}
}
