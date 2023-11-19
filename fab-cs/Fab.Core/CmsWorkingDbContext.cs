using Fab.Data;
using Microsoft.EntityFrameworkCore;

namespace Fab.Core;

public class CmsWorkingDbContext(DbContextOptions<CmsWorkingDbContext> options) : DbContext(options) {
	public virtual DbSet<Article> Articles => Set<Article>();
	public virtual DbSet<Paragraph> Paragraphs => Set<Paragraph>();
	public virtual DbSet<Image> Images => Set<Image>();

	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
	//	optionsBuilder.UseSqlite($"Data Source={AppDomain.CurrentDomain.SetupInformation.ApplicationBase}fab.db");
	//}

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		modelBuilder.Entity<ContentBase>().ToTable("ContentBases");
		modelBuilder.Entity<Paragraph>().ToTable(nameof(Paragraphs));
		modelBuilder.Entity<Image>().ToTable(nameof(Images));
	}
}
