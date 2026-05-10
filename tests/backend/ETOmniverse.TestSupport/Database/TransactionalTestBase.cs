namespace ETOmniverse.TestSupport.Database;

using ETOmniverse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

/// <summary>
/// Per-test transaction rollback base class (per F-007 / D-21 integration test 主路線).
/// Inheriting test class MUST be in <c>[Collection("Database")]</c> to receive <see cref="MsSqlContainerFixture"/>.
/// <see cref="InitializeAsync"/> opens a DbContext-bound transaction; <see cref="DisposeAsync"/> rolls it back.
/// SaveChanges in test methods is observable within the test method, invisible to other test methods.
/// </summary>
public abstract class TransactionalTestBase : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;
    private IDbContextTransaction? _transaction;

    /// <summary>
    /// Per-test DbContext bound to the rollback transaction. <c>null</c> sentinel before
    /// <see cref="InitializeAsync"/>; tests should not access pre-init.
    /// </summary>
    protected EtOmniverseDbContext DbContext { get; private set; } = null!;

    protected TransactionalTestBase(MsSqlContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        if (!_fixture.IsContainerAvailable)
        {
            // Docker-only tests use DockerFact/DockerTheory to report a real skip.
            return;
        }
        DbContext = _fixture.CreateDbContext();
        _transaction = await DbContext.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        if (DbContext is not null)
        {
            await DbContext.DisposeAsync();
        }
    }
}
