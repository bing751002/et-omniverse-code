namespace ETOmniverse.Infrastructure.Tests.Integration;

using ETOmniverse.Infrastructure.Persistence;
using ETOmniverse.Infrastructure.Tests.Persistence;
using ETOmniverse.TestSupport.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

public sealed class SampleAggregateRepositoryTests
{
    [DockerFact]
    public async Task Repository_PerformsCrudAgainstMsSql()
    {
        await using var fixture = new MsSqlFixture();
        await fixture.InitializeAsync();

        await using var dbContext = fixture.CreateSamplePersistenceDbContext();
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[sample_aggregates]', N'U') IS NULL
                CREATE TABLE [dbo].[sample_aggregates] (
                    [id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [display_name] nvarchar(max) NOT NULL
                );
            """);

        var repository = new RepositoryBase<SampleAggregate>(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var id = Guid.NewGuid();

        await repository.AddAsync(new SampleAggregate { Id = id, DisplayName = "first" });
        await unitOfWork.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(new object[] { id });
        loaded.Should().NotBeNull();
        loaded!.DisplayName.Should().Be("first");

        var all = await repository.ListAsync();
        all.Should().Contain(x => x.Id == id);

        repository.Remove(loaded);
        await unitOfWork.SaveChangesAsync();

        var deleted = await repository.GetByIdAsync(new object[] { id });
        deleted.Should().BeNull();
    }
}
