namespace ETOmniverse.TestSupport.Logging;

using System.Linq;
using FluentAssertions;
using Serilog.Sinks.InMemory;

public static class InMemoryLogAssertions
{
    public static void ShouldHaveLogContaining(this InMemorySink sink, string fragment)
        => sink.LogEvents.Any(e => e.RenderMessage().Contains(fragment))
            .Should().BeTrue($"expected a log event whose rendered message contains '{fragment}'");

    public static void ShouldHavePropertyOnAllEvents(this InMemorySink sink, string propertyName)
        => sink.LogEvents.All(e => e.Properties.ContainsKey(propertyName))
            .Should().BeTrue($"expected every log event to carry property '{propertyName}'");

    public static void ShouldNotHaveProperty(this InMemorySink sink, string propertyName)
        => sink.LogEvents.Any(e => e.Properties.ContainsKey(propertyName))
            .Should().BeFalse($"expected no log event to carry property '{propertyName}'");
}
