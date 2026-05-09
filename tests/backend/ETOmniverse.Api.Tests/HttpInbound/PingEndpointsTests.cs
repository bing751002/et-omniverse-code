namespace ETOmniverse.Api.Tests.HttpInbound;

using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ETOmniverse.Api.Middleware;
using ETOmniverse.TestSupport.Logging;
using FluentAssertions;
using Xunit;

/// <summary>
/// F-003 AC-7 — Common Ping sample 三 endpoint 跑通完整 inbound pipeline；
/// AC-3 — invalid request → 400 ProblemDetails 含 errors + traceId；
/// AC-2 路徑也順帶驗（ping/fail → 500 ProblemDetails）。
///
/// [Collection("LoggingTests")] — 03-03 deviation：本 test class 與
/// GlobalExceptionHandlerTests 並行執行時，Program.cs `finally Log.CloseAndFlush()`
/// 在某 factory dispose 時會關閉全域 Serilog logger，導致同時執行的另一 factory
/// Sink 收不到 events（pre-existing static-logger 設計 — 已由 Logging/ 既有測試用
/// 同 collection 序列化）。本 class 加入同 collection 避免 cross-class race。
/// </summary>
[Collection("LoggingTests")]
public class PingEndpointsTests
{
    private static LoggingTestWebAppFactory CreateFactory(string env = "IntegrationTest")
    {
        return new LoggingTestWebAppFactory { Environment = env };
    }

    [Fact]
    public async Task Get_ping_returns_200_with_message_pong()
    {
        await using var factory = CreateFactory();
        var res = await factory.CreateClient().GetAsync("/api/common/ping");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("message").GetString().Should().Be("pong");
    }

    [Fact]
    public async Task Post_echo_with_valid_message_returns_200_with_echoed_message()
    {
        await using var factory = CreateFactory();
        var res = await factory.CreateClient().PostAsJsonAsync(
            "/api/common/ping/echo", new { message = "hello" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("message").GetString().Should().Be("hello");
    }

    [Fact]
    public async Task Post_echo_with_empty_message_returns_400_ProblemDetails_with_errors()
    {
        await using var factory = CreateFactory();
        var res = await factory.CreateClient().PostAsJsonAsync(
            "/api/common/ping/echo", new { message = "" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        res.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var corrHeader = res.Headers.GetValues(CorrelationIdMiddleware.HeaderName).First();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("status").GetInt32().Should().Be(400);
        body.GetProperty("traceId").GetString().Should().Be(corrHeader);
        body.GetProperty("code").GetString().Should().Be("VALIDATION");
        body.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Object);
        // errors 必須含 Message 屬性
        body.GetProperty("errors").TryGetProperty("Message", out var fieldErrors).Should().BeTrue();
        fieldErrors.EnumerateArray().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Post_echo_with_too_long_message_returns_400_ProblemDetails()
    {
        await using var factory = CreateFactory();
        var longMsg = new string('x', 51);
        var res = await factory.CreateClient().PostAsJsonAsync(
            "/api/common/ping/echo", new { message = longMsg });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("code").GetString().Should().Be("VALIDATION");
    }

    [Fact]
    public async Task Get_ping_fail_in_IntegrationTest_returns_500_ProblemDetails()
    {
        await using var factory = CreateFactory(env: "IntegrationTest");
        var res = await factory.CreateClient().GetAsync("/api/common/ping/fail");

        res.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        res.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("status").GetInt32().Should().Be(500);
        body.GetProperty("code").GetString().Should().Be("UNEXPECTED");
    }

    [Fact]
    public async Task Get_ping_fail_in_Development_returns_404()
    {
        await using var factory = CreateFactory(env: "Development");
        var res = await factory.CreateClient().GetAsync("/api/common/ping/fail");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
