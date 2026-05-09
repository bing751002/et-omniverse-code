namespace ETOmniverse.TestSupport.Database;

using ETOmniverse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Xunit;

/// <summary>
/// Shared MSSQL Testcontainer for the [Collection("Database")] xUnit collection (per F-007 / D-21).
/// Container is started once per collection (InitializeAsync) and EF Core migration runs once
/// via the F-005 design-time factory. DisposeAsync stops and disposes the container.
/// Per-test data isolation is the responsibility of TransactionalTestBase (07-03) — this fixture
/// only guarantees container lifecycle and that the schema is migrated.
/// </summary>
public sealed class MsSqlContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public EtOmniverseDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EtOmniverseDbContext>();
        EtOmniverseDbContextFactory.ConfigureSqlServer(options, ConnectionString);
        return new EtOmniverseDbContext(options.Options);
    }
}
