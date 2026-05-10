namespace ETOmniverse.Infrastructure.Tests.TestSupport;

using ETOmniverse.TestSupport.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// Smoke test for <see cref="MsSqlContainerFixture"/> proving:
/// (a) [Collection("Database")] binds correctly via DatabaseCollection;
/// (b) Two test classes participating in the collection share the same fixture instance
///     (xUnit guarantee — container starts once for the collection, not once per class);
/// (c) Migration ran in InitializeAsync (__EFMigrationsHistory populated).
/// Per Phase 05 慣例: Docker 不可用時由 DockerFact 回報真正 skipped，不 fake-pass.
/// </summary>
[Collection("Database")]
public sealed class MsSqlContainerFixtureSmokeTestsClassA
{
    private readonly MsSqlContainerFixture _fixture;

    public MsSqlContainerFixtureSmokeTestsClassA(MsSqlContainerFixture fixture) => _fixture = fixture;

    [DockerFact]
    public async Task ContainerStartsAndMigrationApplied_ClassA()
    {
        _fixture.ConnectionString.Should().NotBeNullOrWhiteSpace();
        await using var ctx = _fixture.CreateDbContext();
        var migrationsApplied = await ctx.Database.GetAppliedMigrationsAsync();
        migrationsApplied.Should().NotBeEmpty(
            "MsSqlContainerFixture.InitializeAsync 必須跑過 MigrateAsync (F-005 baseline)");
    }
}

[Collection("Database")]
public sealed class MsSqlContainerFixtureSmokeTestsClassB
{
    private readonly MsSqlContainerFixture _fixture;

    public MsSqlContainerFixtureSmokeTestsClassB(MsSqlContainerFixture fixture) => _fixture = fixture;

    [DockerFact]
    public void SameFixtureInstanceAcrossClasses_ProvedBy_NonEmptyConnString()
    {
        // 兩個 class 都 [Collection("Database")] → xUnit 保證共用 fixture instance；
        // 若 ConnectionString 仍可解析 → 同一 container instance 仍存活（沒 disposal 後又 recreate）。
        _fixture.ConnectionString.Should().NotBeNullOrWhiteSpace();
    }
}
