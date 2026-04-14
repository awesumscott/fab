using Fab.Client.Core.Services;
using Fab.Core;
using Fab.Data;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Fab.Host.Tests;

public class HttpContentClientTests : IClassFixture<FabHostFixture>, IAsyncLifetime {
	private readonly FabHostFixture _fixture;
	private readonly IContentClient _client;

	public HttpContentClientTests(FabHostFixture fixture) {
		_fixture = fixture;
		var http = fixture.CreateClient();
		http.BaseAddress = new Uri(http.BaseAddress!, "/");
		_client = new HttpContentClient(http);
	}

	public ValueTask InitializeAsync() => new(FabHostFixture.ResetDatabaseAsync(_fixture));
	public ValueTask DisposeAsync() => ValueTask.CompletedTask;

	[Fact]
	public async Task GetArticlesAsync_EmptyDatabase_ReturnsEmpty() {
		var ct = TestContext.Current.CancellationToken;
		var articles = await _client.GetArticlesAsync(cancellationToken: ct);
		articles.ShouldBeEmpty();
	}

	[Fact]
	public async Task GetArticleAsync_MissingId_ReturnsNull() {
		var ct = TestContext.Current.CancellationToken;
		var article = await _client.GetArticleAsync(9999, cancellationToken: ct);
		article.ShouldBeNull();
	}

	[Fact]
	public async Task GetArticleAsync_Polymorphic_DeserializesDerivedTypes() {
		var ct = TestContext.Current.CancellationToken;
		int id;
		await using (var scope = _fixture.CreateScope()) {
			var db = scope.ServiceProvider.GetRequiredService<CmsWorkingDbContext>();
			var article = new Article {
				Title = "Round-trip",
				Entries = {
					new OrderedContentEntry(0, new Paragraph { Text = "Body text." }),
					new OrderedContentEntry(1, new Image { Url = "https://example.com/x.jpg", Alt = "X" }),
				}
			};
			db.Articles.Add(article);
			await db.SaveChangesAsync(ct);
			id = article.Id;
		}

		var fetched = await _client.GetArticleAsync(id, cancellationToken: ct);
		fetched.ShouldNotBeNull();
		fetched.Title.ShouldBe("Round-trip");
		fetched.Entries.Count.ShouldBe(2);

		fetched.Entries.Single(e => e.Order == 0).Content.ShouldBeOfType<Paragraph>().Text.ShouldBe("Body text.");
		var image = fetched.Entries.Single(e => e.Order == 1).Content.ShouldBeOfType<Image>();
		image.Url.ShouldBe("https://example.com/x.jpg");
		image.Alt.ShouldBe("X");
	}

	[Fact]
	public async Task GetArticleAsync_AccessibilityFalse_DropsAltText() {
		var ct = TestContext.Current.CancellationToken;
		int id;
		await using (var scope = _fixture.CreateScope()) {
			var db = scope.ServiceProvider.GetRequiredService<CmsWorkingDbContext>();
			var article = new Article {
				Title = "Accessibility",
				Entries = { new OrderedContentEntry(0, new Image { Url = "https://example.com/x.jpg", Alt = "X" }) }
			};
			db.Articles.Add(article);
			await db.SaveChangesAsync(ct);
			id = article.Id;
		}

		var fetched = await _client.GetArticleAsync(id, includeAccessibility: false, cancellationToken: ct);
		fetched.ShouldNotBeNull();
		var image = fetched.Entries.Single().Content.ShouldBeOfType<Image>();
		image.Url.ShouldBe("https://example.com/x.jpg");
		image.Alt.ShouldBe(string.Empty);
	}
}
