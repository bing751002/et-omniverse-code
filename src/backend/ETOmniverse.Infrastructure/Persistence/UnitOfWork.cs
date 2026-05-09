namespace ETOmniverse.Infrastructure.Persistence;

using ETOmniverse.Domain.Common.Ports;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly EtOmniverseDbContext _dbContext;

    public UnitOfWork(EtOmniverseDbContext dbContext) => _dbContext = dbContext;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
