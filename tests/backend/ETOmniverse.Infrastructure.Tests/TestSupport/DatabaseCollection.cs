namespace ETOmniverse.Infrastructure.Tests.TestSupport;

using ETOmniverse.TestSupport.Database;
using Xunit;

/// <summary>
/// xUnit v2 limitation: [CollectionDefinition] is only discovered in the same assembly as the test class.
/// 因此每個 consuming test project 都要 re-declare 一次（只是綁同一個 <see cref="MsSqlContainerFixture"/>）。
/// 這是必然的 boilerplate 一行檔，不是雙寫設定。Future 業務 phase 寫 integration test 時複製即可。
/// </summary>
[CollectionDefinition("Database")]
public sealed class DatabaseCollection : ICollectionFixture<MsSqlContainerFixture>
{
    // intentionally empty — xUnit discovers via attribute
}
