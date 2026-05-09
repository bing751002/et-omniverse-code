namespace ETOmniverse.TestSupport.Time;

using Microsoft.Extensions.Time.Testing;
using Xunit;

/// <summary>
/// xUnit collection fixture for tests that depend on time control.
/// Per F-007 / D-20: tests inject this fixture's Provider in place of TimeProvider.System.
/// Anchor at 2026-01-01T00:00:00Z so test outputs are deterministic.
/// </summary>
public sealed class FakeTimeProviderFixture
{
    public FakeTimeProvider Provider { get; } =
        new FakeTimeProvider(DateTimeOffset.Parse("2026-01-01T00:00:00Z"));
}

[CollectionDefinition("Time")]
public sealed class TimeCollection : ICollectionFixture<FakeTimeProviderFixture>
{
    // intentionally empty — xUnit discovers via attribute
}
