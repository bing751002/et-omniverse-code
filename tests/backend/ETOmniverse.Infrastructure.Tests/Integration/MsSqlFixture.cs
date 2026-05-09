namespace ETOmniverse.Infrastructure.Tests.Integration;

using ETOmniverse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

internal sealed class MsSqlFixture : IAsyncLifetime
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
