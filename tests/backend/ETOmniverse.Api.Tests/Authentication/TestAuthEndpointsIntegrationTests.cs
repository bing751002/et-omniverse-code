namespace ETOmniverse.Api.Tests.Authentication;

using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ETOmniverse.Api.Authentication.Test;
using ETOmniverse.TestSupport.Logging;
using FluentAssertions;
using Xunit;

/// <summary>
/// F-006 AC-3 / AC-4 / AC-5 / AC-6 / AC-9 — IntegrationTest env 下 [Authorize] /
/// [Authorize(Roles="Admin")] fixture endpoint 四個結果情境 + X-Correlation-Id 留存。
///
/// [Collection("LoggingTests")]：Phase 02 既有 gotcha — Program.cs `finally
/// Log.CloseAndFlush()` 在 factory dispose 時會關掉全域 Serilog logger，與並行測試
/// 共享 sink 會 race。沿用同 collection 序列化。
/// </summary>
[Collection("LoggingTests")]
public class TestAuthEndpointsIntegrationTests
{
    // AC-3: authenticated user 進 [Authorize] → 200 + body 含 user name
    [Fact]
    public async Task Whoami_WithTestUserHeader_Returns200WithName()
    {
        await using var factory = new LoggingTestWebAppFactory();
        var client = factory.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/test/auth/whoami");
        req.Headers.Add(TestAuthenticationDefaults.UserHeaderName, "alice");

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("name").GetString().Should().Be("alice");
        doc.RootElement.GetProperty("authenticated").GetBoolean().Should().BeTrue();
    }

    // AC-4: 無 auth header → 401 ProblemDetails + X-Correlation-Id 留存
    [Fact]
    public async Task Whoami_NoAuthHeader_Returns401ProblemDetailsWithCorrelationId()
    {
        await using var factory = new LoggingTestWebAppFactory();
        var client = factory.CreateClient();

        var resp = await client.GetAsync("/api/test/auth/whoami");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        resp.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        resp.Headers.Should().ContainKey("X-Correlation-Id");
        resp.Headers.GetValues("X-Correlation-Id").Should().NotBeEmpty();
    }

    // AC-5: authenticated 但無 Admin role → 403 ProblemDetails + X-Correlation-Id 留存
    [Fact]
    public async Task Admin_WithUserNoRoles_Returns403ProblemDetailsWithCorrelationId()
    {
        await using var factory = new LoggingTestWebAppFactory();
        var client = factory.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/test/auth/admin");
        req.Headers.Add(TestAuthenticationDefaults.UserHeaderName, "alice");

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        resp.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        resp.Headers.Should().ContainKey("X-Correlation-Id");
    }

    // AC-6: authenticated + Admin role → 200
    [Fact]
    public async Task Admin_WithUserAndAdminRole_Returns200()
    {
        await using var factory = new LoggingTestWebAppFactory();
        var client = factory.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/test/auth/admin");
        req.Headers.Add(TestAuthenticationDefaults.UserHeaderName, "alice");
        req.Headers.Add(TestAuthenticationDefaults.RoleHeaderName, "Admin,Editor");

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("name").GetString().Should().Be("alice");
    }

    // AC-9 強化：client 送的 X-Correlation-Id 在 401 response 必須原樣 echo（F-002/F-003 既有契約）
    [Fact]
    public async Task Whoami_WithClientCorrelationId_EchoesItInProblemDetailsResponse()
    {
        await using var factory = new LoggingTestWebAppFactory();
        var client = factory.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/test/auth/whoami");
        const string corr = "fixed-corr-id-06-03";
        req.Headers.Add("X-Correlation-Id", corr);

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        resp.Headers.GetValues("X-Correlation-Id").Should().ContainSingle().Which.Should().Be(corr);
    }
}
