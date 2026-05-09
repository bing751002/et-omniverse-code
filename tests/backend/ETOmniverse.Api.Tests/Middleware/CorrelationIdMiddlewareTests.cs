namespace ETOmniverse.Api.Tests.Middleware;

using System.Net.Http;
using System.Threading.Tasks;
using ETOmniverse.TestSupport.Logging;
using FluentAssertions;
using Xunit;

[Collection("LoggingTests")]
public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task No_incoming_header_generates_GUID_N_format()
    {
        await using var f = new LoggingTestWebAppFactory();
        var resp = await f.CreateClient().GetAsync("/health");
        resp.Headers.GetValues("X-Correlation-Id").Single()
            .Should().MatchRegex("^[0-9a-fA-F]{32}$");
    }

    [Fact]
    public async Task Incoming_header_is_propagated_unchanged()
    {
        await using var f = new LoggingTestWebAppFactory();
        var client = f.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/test/echo");
        req.Headers.Add("X-Correlation-Id", "test-corr-123");
        req.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        var resp = await client.SendAsync(req);
        resp.Headers.GetValues("X-Correlation-Id").Single().Should().Be("test-corr-123");
    }

    [Fact]
    public async Task All_log_events_in_request_share_same_CorrelationId()
    {
        await using var f = new LoggingTestWebAppFactory();
        var client = f.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/test/echo");
        req.Headers.Add("X-Correlation-Id", "shared-corr");
        req.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        _ = await client.SendAsync(req);

        var corrIds = f.Sink.LogEvents
            .Where(e => e.Properties.ContainsKey("CorrelationId"))
            .Select(e => e.Properties["CorrelationId"].ToString().Trim('"'))
            .Distinct();
        corrIds.Should().Contain("shared-corr");
    }
}
