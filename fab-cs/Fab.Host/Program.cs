using Fab.Core;
using Fab.Host.Json;
using Microsoft.EntityFrameworkCore;

namespace Fab.Host {
	public partial class Program {
		public static void Main(string[] args) {
			var builder = WebApplication.CreateSlimBuilder(args);
			builder.ConfigureFab();
			builder.Services.AddFabDatabase(builder.Configuration);

			builder.Services.ConfigureHttpJsonOptions(options => {
				options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
				options.SerializerOptions.PropertyNameCaseInsensitive = true;
			});

			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddOpenApi();

			var app = builder.Build();

			app.MapOpenApi();

			static Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Data.Article, Data.ContentBase> getLoadedArticles(CmsWorkingDbContext db) =>
				db.Articles.Include(x => x.Entries).ThenInclude(x => x.Content);

			var articlesApi = app.MapGroup("/articles");
			articlesApi.MapGet("/", (CmsWorkingDbContext db, bool? accessibility) => {
				var articles = getLoadedArticles(db).ToList();
				return Results.Json(articles, FabJson.ForRequest(accessibility ?? true));
			});
			articlesApi.MapGet("/{id}", (int id, CmsWorkingDbContext db, bool? accessibility) => {
				var article = getLoadedArticles(db).FirstOrDefault(a => a.Id == id);
				return article is null
					? Results.NotFound()
					: Results.Json(article, FabJson.ForRequest(accessibility ?? true));
			});

			app.Run();
		}
	}
}
