namespace ETOmniverse.Infrastructure.Tests.Persistence;

using ETOmniverse.Domain.Common.Entity;

internal sealed class SampleAggregate : IAggregateRoot
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = "";
}
