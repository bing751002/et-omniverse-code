namespace ETOmniverse.Api.Tests.Logging;

using System.Threading.Tasks;
using ETOmniverse.TestSupport.Logging;
using Xunit;

[Collection("LoggingTests")]
public class EnricherTests
{
    [Fact]
    public async Task All_log_events_carry_required_enricher_properties_and_no_UserId()
    {
        await using var factory = new LoggingTestWebAppFactory();
        _ = await factory.CreateClient().GetAsync("/health");

        factory.Sink.ShouldHavePropertyOnAllEvents("MachineName");
        factory.Sink.ShouldHavePropertyOnAllEvents("EnvironmentName");
        factory.Sink.ShouldHavePropertyOnAllEvents("AppName");
        factory.Sink.ShouldHavePropertyOnAllEvents("AppVersion");
        factory.Sink.ShouldNotHaveProperty("UserId");   // F-002 不掛 UserId enricher
    }
}
