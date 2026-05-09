namespace ETOmniverse.Infrastructure.Persistence;

using ETOmniverse.Domain.Common.Entity;
using ETOmniverse.Domain.Common.Ports;
using Microsoft.EntityFrameworkCore;

public class RepositoryBase<T> : IRepository<T> where T : class, IAggregateRoot
{
    public RepositoryBase(EtOmniverseDbContext dbContext) => DbContext = dbContext;

    protected EtOmniverseDbContext DbContext { get; }

    protected DbSet<T> Set => DbContext.Set<T>();

    public Task AddAsync(T aggregate, CancellationToken cancellationToken = default) =>
        Set.AddAsync(aggregate, cancellationToken).AsTask();

    public ValueTask<T?> GetByIdAsync(object[] keyValues, CancellationToken cancellationToken = default) =>
        Set.FindAsync(keyValues, cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default) =>
        await Set.ToListAsync(cancellationToken);

    public void Remove(T aggregate) => Set.Remove(aggregate);
}
