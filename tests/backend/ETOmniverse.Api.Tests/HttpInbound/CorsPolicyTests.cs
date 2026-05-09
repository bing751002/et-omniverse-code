namespace ETOmniverse.Api.Tests.HttpInbound;

using System.Net.Http;
using System.Threading.Tasks;
using ETOmniverse.TestSupport.Logging;
using FluentAssertions;
using Xunit;

/// <summary>
/// F-003 AC-5 — CORS policy：Dev allow-all / Prod 無 allowlist fail-closed / Prod 有 allowlist 白名單通過。
///
/// [Collection("LoggingTests")] — 沿用 03-03 convention（Program.cs `finally Log.CloseAndFlush()`
/// 全域 logger 關閉的併發干擾），避免與其他 WAF 整合 test class 並行 race。
/// </summary>
[Collection("LoggingTests")]
public class CorsPolicyTests
{
    private static LoggingTestWebAppFactory CreateFactory(string env, params (string Key, string Value)[] settings)
    {
        var f = new LoggingTestWebAppFactory { Environment = env };
        foreach (var (k, v) in settings)
        {
            f.WithSetting(k, v);
        }
        return f;
    }

    private static async Task<HttpResponseMessage> Preflight(HttpClient client, string origin, string path = "/api/common/ping")
    {
        var req = new HttpRequestMessage(HttpMethod.Options, path);
        req.Headers.Add("Origin", origin);
        req.Headers.Add("Access-Control-Request-Method", "GET");
        return await client.SendAsync(req);
    }

    [Fact]
    public async Task Development_environment_allows_any_origin()
    {
        await using var factory = CreateFactory("Development");
        var res = await Preflight(factory.CreateClient(), "https://random.example");

        res.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue(
            "Development 應 allow-all，preflight 必含 Access-Control-Allow-Origin header");
    }

    [Fact]
    public async Task Production_without_allowlist_blocks_cross_origin()
    {
        await using var factory = CreateFactory("Production");
        var res = await Preflight(factory.CreateClient(), "https://random.example");

        res.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse(
            "Production 無 allowlist 應 fail-closed，不返回 Access-Control-Allow-Origin");
    }

    [Fact]
    public async Task Production_with_allowlist_allows_listed_origin()
    {
        await using var factory = CreateFactory(
            "Production",
            ("Cors:AllowedOrigins:0", "https://allowed.example"));
        var res = await Preflight(factory.CreateClient(), "https://allowed.example");

        res.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue();
        res.Headers.GetValues("Access-Control-Allow-Origin").Should()
            .Contain(v => v.Contains("https://allowed.example"));
    }

    [Fact]
    public async Task Production_with_allowlist_blocks_unlisted_origin()
    {
        await using var factory = CreateFactory(
            "Production",
            ("Cors:AllowedOrigins:0", "https://allowed.example"));
        var res = await Preflight(factory.CreateClient(), "https://other.example");

        res.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }
}
