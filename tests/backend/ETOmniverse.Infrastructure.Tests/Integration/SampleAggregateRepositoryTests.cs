namespace ETOmniverse.Infrastructure.Tests.Integration;

using FluentAssertions;

public sealed class SampleAggregateRepositoryTests
{
    [Fact(Skip = "Requires Docker daemon for Testcontainers MSSQL; Docker is not running in the current environment.")]
    public async Task CanMigrateContainerDatabase()
    {
        await using var fixture = new MsSqlFixture();
        await fixture.InitializeAsync();

        await using var dbContext = fixture.CreateDbContext();
        (await dbContext.Database.CanConnectAsync()).Should().BeTrue();
    }
}
