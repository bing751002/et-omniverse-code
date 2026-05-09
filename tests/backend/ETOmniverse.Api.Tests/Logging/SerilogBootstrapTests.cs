namespace ETOmniverse.Api.Tests.Logging;

using System.Linq;
using System.Threading.Tasks;
using ETOmniverse.TestSupport.Logging;
using FluentAssertions;
using Serilog.Formatting.Compact;
using Xunit;

public class SerilogBootstrapTests
{
    [Fact]
    public async Task Bootstrap_first_log_is_CLEF_json_with_required_fields()
    {
        await using var factory = new LoggingTestWebAppFactory();
        var client = factory.CreateClient();

        // 觸發任一 request 確保 startup log + request log 都產出
        _ = await client.GetAsync("/health");

        factory.Sink.LogEvents.Should().NotBeEmpty();

        var first = factory.Sink.LogEvents.First();
        // CLEF JSON formatter 序列化測試（@t / @m / @l 是 CLEF spec 必備）
        using var sw = new System.IO.StringWriter();
        new RenderedCompactJsonFormatter().Format(first, sw);
        var json = sw.ToString();

        json.Should().Contain("\"@t\":");
        json.Should().Contain("\"@m\":");

        factory.Sink.ShouldHavePropertyOnAllEvents("AppName");
        factory.Sink.ShouldHavePropertyOnAllEvents("EnvironmentName");
    }
}
