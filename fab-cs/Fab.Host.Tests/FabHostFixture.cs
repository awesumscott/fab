using Fab.Core;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fab.Host.Tests;

public sealed class FabHostFixture : WebApplicationFactory<Program>, IAsyncLifetime {
	private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"fab-test-{Guid.NewGuid():N}.db");

	protected override IHost CreateHost(IHostBuilder builder) {
		builder.ConfigureHostConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?> {
			["Database:ConnectionString"] = $"Data Source={_dbPath}"
		}));
		return base.CreateHost(builder);
	}

	public async Task InitializeAsync() {
		using var scope = Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<CmsWorkingDbContext>();
		await db.Database.EnsureCreatedAsync();
	}

	async Task IAsyncLifetime.DisposeAsync() {
		await DisposeAsync();
		if (File.Exists(_dbPath)) File.Delete(_dbPath);
	}

	public AsyncServiceScope CreateScope() => Services.CreateAsyncScope();

	public static async Task ResetDatabaseAsync(FabHostFixture fixture) {
		await using var scope = fixture.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<CmsWorkingDbContext>();
		await db.Database.ExecuteSqlRawAsync(
			"DELETE FROM OrderedContentEntry; " +
			"DELETE FROM Paragraphs; " +
			"DELETE FROM Images; " +
			"DELETE FROM Articles; " +
			"DELETE FROM ContentBases;");
	}
}
