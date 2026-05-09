namespace ETOmniverse.TestSupport.Database;

using Xunit;

/// <summary>
/// xUnit collection definition binding [Collection("Database")] to <see cref="MsSqlContainerFixture"/>.
/// Test classes participating in this collection share the same container instance.
/// Combine with TransactionalTestBase (07-03) for per-method rollback isolation.
/// </summary>
[CollectionDefinition("Database")]
public sealed class DatabaseCollection : ICollectionFixture<MsSqlContainerFixture>
{
    // intentionally empty — xUnit discovers via attribute
}
