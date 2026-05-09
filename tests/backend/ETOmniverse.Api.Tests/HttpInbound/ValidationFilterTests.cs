namespace ETOmniverse.Api.Tests.HttpInbound;

using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ETOmniverse.TestSupport.Logging;
using FluentAssertions;
using Xunit;

/// <summary>
/// F-003 AC-3 — ValidationEndpointFilter 行為細節（補 PingEndpointsTests 未涵蓋部分）：
/// errors 是 dictionary、key 為 PropertyName、value 為 string array、code === "VALIDATION"。
///
/// [Collection("LoggingTests")] — 同 PingEndpointsTests 註解，避免與
/// GlobalExceptionHandlerTests 並行時 Program.cs `Log.CloseAndFlush()` 干擾 Sink。
/// </summary>
[Collection("LoggingTests")]
public class ValidationFilterTests
{
    [Fact]
    public async Task Filter_returns_ProblemDetails_with_errors_dictionary_shape()
    {
        await using var factory = new LoggingTestWebAppFactory();
        var res = await factory.CreateClient().PostAsJsonAsync(
            "/api/common/ping/echo", new { message = "" });

        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Object);
        // 每個 key 對應 string[] value
        foreach (var prop in body.GetProperty("errors").EnumerateObject())
        {
            prop.Value.ValueKind.Should().Be(JsonValueKind.Array);
            prop.Value.EnumerateArray().Should().AllSatisfy(
                e => e.ValueKind.Should().Be(JsonValueKind.String));
        }
    }

    [Fact]
    public async Task Filter_uses_VALIDATION_code_in_ProblemDetails()
    {
        await using var factory = new LoggingTestWebAppFactory();
        var res = await factory.CreateClient().PostAsJsonAsync(
            "/api/common/ping/echo", new { message = "" });

        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("code").GetString().Should().Be("VALIDATION");
        body.GetProperty("type").GetString().Should().Contain("/VALIDATION");
    }

    [Fact]
    public async Task Get_ping_passes_without_validator_filter()
    {
        // GET /api/common/ping 不掛 validator filter — 確認 unrelated request 不被 filter 影響
        await using var factory = new LoggingTestWebAppFactory();
        var res = await factory.CreateClient().GetAsync("/api/common/ping");
        res.IsSuccessStatusCode.Should().BeTrue();
    }
}
