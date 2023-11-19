using Fab.Core;
using Microsoft.EntityFrameworkCore;

namespace Fab.Host {
	public class Program {
		public static void Main(string[] args) {
			var builder = WebApplication.CreateSlimBuilder(args);
			builder.ConfigureFab();
			//builder.Services.AddControllers();
			//builder.Services.ConfigureHttpJsonOptions(options => {
			//	options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
			//});
			//builder.Services.AddTransient<   >();

			var app = builder.Build();

			//app.MapControllers();

			static Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Data.Article, Data.ContentBase> getLoadedArticles(CmsWorkingDbContext db) =>
				db.Articles.Include(x => x.Entries).ThenInclude(x => x.Content);

			var articlesApi = app.MapGroup("/articles");
			articlesApi.MapGet("/", (CmsWorkingDbContext db) => getLoadedArticles(db).ToList());
			articlesApi.MapGet("/{id}", (int id, CmsWorkingDbContext db) =>
				getLoadedArticles(db).FirstOrDefault(a => a.Id == id) is { } article
					? Results.Ok(article)
					: Results.NotFound());

			//var sampleButts = new string[] { "A", "B", "C", "D", "E" };
			//var buttApi = app.MapGroup("/butts");
			//buttApi.MapGet("/", () => sampleButts);
			//buttApi.MapGet("/{id}", (int id) =>
			//	id < sampleButts.Length && id >= 0
			//		? Results.Ok(sampleButts[id])
			//		: Results.NotFound());

			//if (app.Environment.IsDevelopment()) {
			//	app.UseSwagger();
			//	app.UseSwaggerUI();
			//}

			app.Run();
		}
	}

	//public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

	//[JsonSerializable(typeof(Todo[]))]
	//[JsonSerializable(typeof(string[]))]
	//internal partial class AppJsonSerializerContext : JsonSerializerContext {}
}
