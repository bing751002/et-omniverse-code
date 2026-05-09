namespace ETOmniverse.Infrastructure.Tests.Persistence;

using ETOmniverse.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

public sealed class NamingConventionTests
{
    [Fact]
    public void Model_UsesSnakeCaseColumnsAndPluralTables()
    {
        var options = new DbContextOptionsBuilder<EtOmniverseDbContext>()
            .UseSqlServer("Server=localhost;Database=ETOmniverseTest;Trusted_Connection=True;TrustServerCertificate=True")
            .UseSnakeCaseNamingConvention()
            .Options;

        using var dbContext = new SamplePersistenceDbContext(options);
        var entity = dbContext.Model.FindEntityType(typeof(SampleAggregate));

        entity.Should().NotBeNull();
        var nonNullEntity = entity!;
        nonNullEntity.GetTableName().Should().Be("sample_aggregates");
        var displayName = nonNullEntity.FindProperty(nameof(SampleAggregate.DisplayName));
        displayName.Should().NotBeNull();
        displayName!.GetColumnName().Should().Be("display_name");
    }

    [Fact]
    public void MigrationHistoryTable_RemainsEfDefaultName()
    {
        var options = new DbContextOptionsBuilder<EtOmniverseDbContext>()
            .UseSqlServer("Server=localhost;Database=ETOmniverseTest;Trusted_Connection=True;TrustServerCertificate=True")
            .UseSnakeCaseNamingConvention()
            .Options;

        using var dbContext = new EtOmniverseDbContext(options);

        dbContext.Database.GetDbConnection().Should().NotBeNull();
    }
}
