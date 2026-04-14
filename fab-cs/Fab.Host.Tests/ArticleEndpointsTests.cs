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

	public ValueTask InitializeAsync() => new(FabHostFixture.ResetDatabaseAsync(_fixture));

	public ValueTask DisposeAsync() => ValueTask.CompletedTask;

	[Fact]
	public async Task GetAll_EmptyDatabase_ReturnsEmptyArray() {
		var ct = TestContext.Current.CancellationToken;
		var response = await _client.GetAsync("/articles", ct);
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var articles = await response.Content.ReadFromJsonAsync<List<Article>>(ct);
		articles.ShouldNotBeNull();
		articles.ShouldBeEmpty();
	}

	[Fact]
	public async Task GetById_MissingId_Returns404() {
		var ct = TestContext.Current.CancellationToken;
		var response = await _client.GetAsync("/articles/9999", ct);
		response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetById_ExistingArticle_ReturnsArticleWithPolymorphicEntries() {
		var ct = TestContext.Current.CancellationToken;
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
			await db.SaveChangesAsync(ct);
			articleId = article.Id;
		}

		var response = await _client.GetAsync($"/articles/{articleId}", ct);
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var fetched = await response.Content.ReadFromJsonAsync<Article>(ct);
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
		var ct = TestContext.Current.CancellationToken;
		await using (var scope = _fixture.CreateScope()) {
			var db = scope.ServiceProvider.GetRequiredService<CmsWorkingDbContext>();
			db.Articles.AddRange(
				new Article { Title = "First" },
				new Article { Title = "Second" },
				new Article { Title = "Third" });
			await db.SaveChangesAsync(ct);
		}

		var response = await _client.GetAsync("/articles", ct);
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		var articles = await response.Content.ReadFromJsonAsync<List<Article>>(ct);
		articles.ShouldNotBeNull();
		articles.Count.ShouldBe(3);
		articles.Select(a => a.Title).ShouldBe(new[] { "First", "Second", "Third" }, ignoreOrder: true);
	}
}
