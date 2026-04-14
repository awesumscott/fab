using System.Net;
using System.Text.Json;
using Fab.Core;
using Fab.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Fab.Host.Tests;

public class JsonContractTests : IClassFixture<FabHostFixture>, IAsyncLifetime {
	private readonly FabHostFixture _fixture;
	private readonly HttpClient _client;

	public JsonContractTests(FabHostFixture fixture) {
		_fixture = fixture;
		_client = fixture.CreateClient();
	}

	public Task InitializeAsync() => FabHostFixture.ResetDatabaseAsync(_fixture);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public async Task Response_UsesCamelCaseProperties() {
		var id = await SeedArticleWithImage();

		var raw = await _client.GetStringAsync($"/articles/{id}");
		using var doc = JsonDocument.Parse(raw);

		doc.RootElement.TryGetProperty("title", out _).ShouldBeTrue("expected camelCase 'title'");
		doc.RootElement.TryGetProperty("Title", out _).ShouldBeFalse("server should not emit PascalCase");
		doc.RootElement.TryGetProperty("entries", out _).ShouldBeTrue();
	}

	[Fact]
	public async Task PolymorphicDiscriminator_IsStable() {
		var id = await SeedArticleWithImage();

		var raw = await _client.GetStringAsync($"/articles/{id}");
		using var doc = JsonDocument.Parse(raw);

		var firstEntry = doc.RootElement.GetProperty("entries")[0];
		var content = firstEntry.GetProperty("content");
		content.GetProperty("type").GetString().ShouldBe("image");
	}

	[Fact]
	public async Task GetById_AccessibilityDefault_IncludesAltText() {
		var id = await SeedArticleWithImage();

		var raw = await _client.GetStringAsync($"/articles/{id}");
		using var doc = JsonDocument.Parse(raw);

		var content = doc.RootElement.GetProperty("entries")[0].GetProperty("content");
		content.TryGetProperty("alt", out var alt).ShouldBeTrue();
		alt.GetString().ShouldBe("A cat");
	}

	[Fact]
	public async Task GetById_AccessibilityFalse_OmitsAltText() {
		var id = await SeedArticleWithImage();

		var raw = await _client.GetStringAsync($"/articles/{id}?accessibility=false");
		using var doc = JsonDocument.Parse(raw);

		var content = doc.RootElement.GetProperty("entries")[0].GetProperty("content");
		content.TryGetProperty("alt", out _).ShouldBeFalse("alt is marked [Accessibility]; should drop when accessibility=false");
		content.GetProperty("url").GetString().ShouldBe("https://example.com/cat.jpg");
	}

	[Fact]
	public async Task GetAll_AccessibilityFalse_FiltersAcrossMultipleArticles() {
		await SeedArticleWithImage("First");
		await SeedArticleWithImage("Second");

		var raw = await _client.GetStringAsync("/articles?accessibility=false");
		using var doc = JsonDocument.Parse(raw);

		foreach (var article in doc.RootElement.EnumerateArray()) {
			var content = article.GetProperty("entries")[0].GetProperty("content");
			content.TryGetProperty("alt", out _).ShouldBeFalse();
		}
	}

	[Fact]
	public async Task OpenApi_EndpointIsReachable() {
		var response = await _client.GetAsync("/openapi/v1.json");
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var body = await response.Content.ReadAsStringAsync();
		using var doc = JsonDocument.Parse(body);
		doc.RootElement.GetProperty("openapi").GetString().ShouldNotBeNullOrWhiteSpace();
		doc.RootElement.GetProperty("paths").TryGetProperty("/articles/{id}", out _).ShouldBeTrue();
	}

	private async Task<int> SeedArticleWithImage(string title = "Test") {
		await using var scope = _fixture.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<CmsWorkingDbContext>();
		var article = new Article {
			Title = title,
			Entries = {
				new OrderedContentEntry(0, new Image { Url = "https://example.com/cat.jpg", Alt = "A cat" }),
			}
		};
		db.Articles.Add(article);
		await db.SaveChangesAsync();
		return article.Id;
	}
}
