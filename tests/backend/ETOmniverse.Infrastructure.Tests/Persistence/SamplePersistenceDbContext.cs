namespace ETOmniverse.Infrastructure.Tests.Persistence;

using ETOmniverse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

internal sealed class SamplePersistenceDbContext : EtOmniverseDbContext
{
    public SamplePersistenceDbContext(DbContextOptions<EtOmniverseDbContext> options)
        : base(options)
    {
    }

    public DbSet<SampleAggregate> SampleAggregates => Set<SampleAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<SampleAggregate>().HasKey(x => x.Id);
        modelBuilder.ApplyEtOmniversePluralTableNames();
    }
}
