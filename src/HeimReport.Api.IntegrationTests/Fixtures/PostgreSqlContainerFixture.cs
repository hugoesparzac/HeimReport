using HeimReport.Api.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace HeimReport.Api.IntegrationTests.Fixtures;

/// <summary>
/// Spawns a real Postgres container (via Docker) once per test run
/// and shares it across all test classes marked with [Collection("Database")].
/// Migrations are applied once during initialization, not for each individual test.
/// </summary>
public class PostgreSqlContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("HeimReportTestDb")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await ApplyMigrationsAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    private async Task ApplyMigrationsAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString);

        await using var context = new ApplicationDbContext(optionsBuilder.Options);
        await context.Database.MigrateAsync();
    }
}