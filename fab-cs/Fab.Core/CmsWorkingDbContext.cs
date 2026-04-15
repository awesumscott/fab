using Fab.Data;
using Microsoft.EntityFrameworkCore;

namespace Fab.Core;

public class CmsWorkingDbContext(DbContextOptions<CmsWorkingDbContext> options) : DbContext(options) {
	public virtual DbSet<Article> Articles => Set<Article>();
	public virtual DbSet<Paragraph> Paragraphs => Set<Paragraph>();
	public virtual DbSet<Image> Images => Set<Image>();
	public virtual DbSet<Heading> Headings => Set<Heading>();
	public virtual DbSet<Link> Links => Set<Link>();
	public virtual DbSet<Divider> Dividers => Set<Divider>();

	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
	//	optionsBuilder.UseSqlite($"Data Source={AppDomain.CurrentDomain.SetupInformation.ApplicationBase}fab.db");
	//}

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		modelBuilder.Entity<ContentBase>().ToTable("ContentBases");
		modelBuilder.Entity<Paragraph>().ToTable(nameof(Paragraphs));
		modelBuilder.Entity<Image>().ToTable(nameof(Images));
		modelBuilder.Entity<Heading>().ToTable(nameof(Headings));
		modelBuilder.Entity<Link>().ToTable(nameof(Links));
		modelBuilder.Entity<Divider>().ToTable(nameof(Dividers));
	}
}
