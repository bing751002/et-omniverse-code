namespace ETOmniverse.Api.Tests.Middleware;

using System.Linq;
using System.Threading.Tasks;
using ETOmniverse.TestSupport.Logging;
using FluentAssertions;
using Xunit;

[Collection("LoggingTests")]
public class RequestLoggingMiddlewareTests
{
    [Fact]
    public async Task Health_endpoint_does_not_emit_request_log()
    {
        await using var f = new LoggingTestWebAppFactory();
        _ = await f.CreateClient().GetAsync("/health");
        f.Sink.LogEvents.Any(e => e.RenderMessage().Contains("HTTP GET /health")).Should().BeFalse();
    }

    [Fact]
    public async Task Non_health_request_emits_summary_log_with_required_structured_properties()
    {
        await using var f = new LoggingTestWebAppFactory();
        _ = await f.CreateClient().PostAsync("/test/echo",
            new System.Net.Http.StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        var summary = f.Sink.LogEvents.First(e => e.MessageTemplate.Text.StartsWith("HTTP "));
        summary.Properties.Should().ContainKeys("Method", "Path", "StatusCode", "DurationMs", "CorrelationId");
        summary.Properties.Should().NotContainKey("UserId");
    }

    [Fact]
    public async Task Status_404_logs_at_Warning_level()
    {
        await using var f = new LoggingTestWebAppFactory();
        _ = await f.CreateClient().GetAsync("/this-route-does-not-exist");
        var summary = f.Sink.LogEvents.First(e => e.MessageTemplate.Text.StartsWith("HTTP "));
        summary.Level.Should().Be(Serilog.Events.LogEventLevel.Warning);
    }

    [Fact]
    public async Task Status_500_unhandled_exception_logs_at_Error_level()
    {
        // B2 強制：5xx 級必須有機械驗證，不能省略
        await using var f = new LoggingTestWebAppFactory();
        try { _ = await f.CreateClient().GetAsync("/test/throw"); }
        catch { /* HttpClient may surface server exception in TestServer; ignore */ }

        var summary = f.Sink.LogEvents.First(e => e.MessageTemplate.Text.StartsWith("HTTP "));
        summary.Level.Should().Be(Serilog.Events.LogEventLevel.Error,
            "AC-3 spec 硬規則：5xx → Error level，必須機械驗證");
        summary.Properties["StatusCode"].ToString().Should().Be("500");
    }
}
