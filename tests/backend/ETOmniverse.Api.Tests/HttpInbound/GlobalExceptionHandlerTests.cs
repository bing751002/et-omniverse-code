namespace ETOmniverse.Api.Tests.HttpInbound;

using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ETOmniverse.Api.Middleware;
using ETOmniverse.TestSupport.Logging;
using FluentAssertions;
using Serilog.Events;
using Xunit;

/// <summary>
/// F-003 AC-2 / AC-4 — GlobalExceptionHandler 整合測：用 LoggingTestWebAppFactory
/// 打 IntegrationTest-only `/test/throw`（Phase 02 既有），驗：
/// (a) 500 + Content-Type application/problem+json
/// (b) ProblemDetails.traceId === response header X-Correlation-Id
/// (c) response 不外洩 stack trace / exception.Message
/// (d) Error 級 log + CorrelationId property
/// LoggingTestWebAppFactory 預設已 UseEnvironment("IntegrationTest")，無需另外設。
/// </summary>
public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task Unhandled_exception_returns_500_ProblemDetails_with_traceId()
    {
        await using var factory = new LoggingTestWebAppFactory();
        var client = factory.CreateClient();

        var res = await client.GetAsync("/test/throw");

        res.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        res.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var corrHeader = res.Headers.GetValues(CorrelationIdMiddleware.HeaderName).First();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("status").GetInt32().Should().Be(500);
        body.GetProperty("traceId").GetString().Should().Be(corrHeader);
        body.GetProperty("code").GetString().Should().Be("UNEXPECTED");
        body.GetProperty("title").GetString().Should().Be("Internal server error");
        body.GetProperty("type").GetString().Should().Be("https://et-omniverse/errors/UNEXPECTED");
    }

    [Fact]
    public async Task Unhandled_exception_response_does_not_leak_stack_trace()
    {
        await using var factory = new LoggingTestWebAppFactory();
        var res = await factory.CreateClient().GetAsync("/test/throw");
        var raw = await res.Content.ReadAsStringAsync();

        raw.Should().NotContain("at ETOmniverse.");
        raw.Should().NotContain("InvalidOperationException");
        raw.Should().NotContain("intentional test exception"); // exception.Message 不外洩
    }

    [Fact]
    public async Task Unhandled_exception_logs_Error_level_with_correlation_id()
    {
        await using var factory = new LoggingTestWebAppFactory();
        _ = await factory.CreateClient().GetAsync("/test/throw");

        factory.Sink.LogEvents
            .Should().Contain(e => e.Level == LogEventLevel.Error,
                "GlobalExceptionHandler 必須以 Error 級記錄 unhandled exception");
        factory.Sink.ShouldHavePropertyOnAllEvents("CorrelationId");
    }
}
