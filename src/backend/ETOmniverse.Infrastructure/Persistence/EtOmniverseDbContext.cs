namespace ETOmniverse.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

public partial class EtOmniverseDbContext : DbContext
{
    public EtOmniverseDbContext(DbContextOptions<EtOmniverseDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyEtOmniversePluralTableNames();
    }
}
