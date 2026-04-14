using System.Net;
using System.Net.Http.Json;
using Fab.Core;
using Fab.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Fab.Host.Tests;

public class ArticleEndpointsTests : IClassFixture<FabHostFixture>, IAsyncLifetime {
	private readonly FabHostFixture _fixture;
	private readonly HttpClient _client;

	public ArticleEndpointsTests(FabHostFixture fixture) {
		_fixture = fixture;
		_client = fixture.CreateClient();
	}

	public Task InitializeAsync() => FabHostFixture.ResetDatabaseAsync(_fixture);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public async Task GetAll_EmptyDatabase_ReturnsEmptyArray() {
		var response = await _client.GetAsync("/articles");
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var articles = await response.Content.ReadFromJsonAsync<List<Article>>();
		articles.ShouldNotBeNull();
		articles.ShouldBeEmpty();
	}

	[Fact]
	public async Task GetById_MissingId_Returns404() {
		var response = await _client.GetAsync("/articles/9999");
		response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetById_ExistingArticle_ReturnsArticleWithPolymorphicEntries() {
		int articleId;
		await using (var scope = _fixture.CreateScope()) {
			var db = scope.ServiceProvider.GetRequiredService<CmsWorkingDbContext>();
			var article = new Article {
				Title = "Test Article",
				Entries = {
					new OrderedContentEntry(0, new Paragraph { Text = "Hello world." }),
					new OrderedContentEntry(1, new Image { Url = "https://example.com/cat.jpg", Alt = "A cat" }),
				}
			};
			db.Articles.Add(article);
			await db.SaveChangesAsync();
			articleId = article.Id;
		}

		var response = await _client.GetAsync($"/articles/{articleId}");
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var fetched = await response.Content.ReadFromJsonAsync<Article>();
		fetched.ShouldNotBeNull();
		fetched.Title.ShouldBe("Test Article");
		fetched.Entries.Count.ShouldBe(2);

		var paragraph = fetched.Entries.Single(e => e.Order == 0).Content.ShouldBeOfType<Paragraph>();
		paragraph.Text.ShouldBe("Hello world.");

		var image = fetched.Entries.Single(e => e.Order == 1).Content.ShouldBeOfType<Image>();
		image.Url.ShouldBe("https://example.com/cat.jpg");
		image.Alt.ShouldBe("A cat");
	}

	[Fact]
	public async Task GetAll_MultipleArticles_ReturnsAll() {
		await using (var scope = _fixture.CreateScope()) {
			var db = scope.ServiceProvider.GetRequiredService<CmsWorkingDbContext>();
			db.Articles.AddRange(
				new Article { Title = "First" },
				new Article { Title = "Second" },
				new Article { Title = "Third" });
			await db.SaveChangesAsync();
		}

		var response = await _client.GetAsync("/articles");
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		var articles = await response.Content.ReadFromJsonAsync<List<Article>>();
		articles.ShouldNotBeNull();
		articles.Count.ShouldBe(3);
		articles.Select(a => a.Title).ShouldBe(new[] { "First", "Second", "Third" }, ignoreOrder: true);
	}
}
