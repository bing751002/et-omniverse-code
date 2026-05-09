namespace ETOmniverse.Api.Tests.HttpInbound;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ETOmniverse.TestSupport.Logging;
using FluentAssertions;
using Xunit;

/// <summary>
/// F-003 AC-6 — OpenAPI policy：Dev 預設 expose / Prod 預設不 expose / OpenApi:Enabled=true 強制 expose；
/// 文件含 title + version metadata。
///
/// [Collection("LoggingTests")] — 沿用 03-03 convention（避免 WAF 並行 race）。
/// </summary>
[Collection("LoggingTests")]
public class OpenApiPolicyTests
{
    private const string OpenApiPath = "/openapi/v1.json";

    private static LoggingTestWebAppFactory CreateFactory(string env, params (string Key, string Value)[] settings)
    {
        var f = new LoggingTestWebAppFactory { Environment = env };
        foreach (var (k, v) in settings)
        {
            f.WithSetting(k, v);
        }
        return f;
    }

    [Fact]
    public async Task Development_default_exposes_openapi_endpoint()
    {
        await using var factory = CreateFactory(
            "Development",
            ("OpenApi:Enabled", "true"));
        var res = await factory.CreateClient().GetAsync(OpenApiPath);
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Production_default_does_not_expose_openapi_endpoint()
    {
        await using var factory = CreateFactory("Production");
        var res = await factory.CreateClient().GetAsync(OpenApiPath);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Production_with_OpenApi_Enabled_true_exposes_openapi_endpoint()
    {
        await using var factory = CreateFactory(
            "Production",
            ("OpenApi:Enabled", "true"));
        var res = await factory.CreateClient().GetAsync(OpenApiPath);
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OpenApi_document_contains_title_and_version()
    {
        await using var factory = CreateFactory(
            "Development",
            ("OpenApi:Enabled", "true"));
        var doc = await factory.CreateClient().GetFromJsonAsync<JsonElement>(OpenApiPath);

        doc.GetProperty("info").GetProperty("title").GetString()
            .Should().NotBeNullOrEmpty().And.Contain("ET-Omniverse");
        doc.GetProperty("info").GetProperty("version").GetString()
            .Should().NotBeNullOrEmpty();
    }
}
