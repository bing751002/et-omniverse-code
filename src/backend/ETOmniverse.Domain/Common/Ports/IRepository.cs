namespace ETOmniverse.Domain.Common.Ports;

using ETOmniverse.Domain.Common.Entity;

public interface IRepository<T> where T : class, IAggregateRoot
{
    Task AddAsync(T aggregate, CancellationToken cancellationToken = default);

    ValueTask<T?> GetByIdAsync(object[] keyValues, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);

    void Remove(T aggregate);
}
