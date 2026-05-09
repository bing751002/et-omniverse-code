namespace ETOmniverse.Api.Tests.Logging;

using System;
using System.Linq;
using System.Threading.Tasks;
using ETOmniverse.TestSupport.Logging;
using FluentAssertions;
using Xunit;

[Collection("LoggingTests")]
public class HeartbeatHostedServiceTests
{
    [Fact]
    public async Task Heartbeat_disabled_by_default_no_tick_logs()
    {
        await using var f = new LoggingTestWebAppFactory();
        _ = f.CreateClient();   // 觸發 host 啟動
        await Task.Delay(TimeSpan.FromSeconds(2));
        f.Sink.LogEvents.Any(e => e.RenderMessage().Contains("heartbeat tick"))
            .Should().BeFalse("heartbeat is disabled by default per CONTEXT D-04");
    }

    [Fact]
    public async Task Heartbeat_enabled_emits_at_least_two_ticks_with_distinct_correlation_ids()
    {
        await using var f = new LoggingTestWebAppFactory()
            .WithSetting("Logging:Heartbeat:Enabled", "true")
            .WithSetting("Logging:Heartbeat:IntervalSeconds", "1");
        _ = f.CreateClient();
        await Task.Delay(TimeSpan.FromMilliseconds(2500));   // 等 ≥ 2 個 tick

        var ticks = f.Sink.LogEvents
            .Where(e => e.RenderMessage().Contains("heartbeat tick"))
            .ToList();
        ticks.Should().HaveCountGreaterOrEqualTo(2);

        var corrIds = ticks
            .Select(e => e.Properties["CorrelationId"].ToString().Trim('"'))
            .Distinct()
            .ToList();
        corrIds.Count.Should().BeGreaterOrEqualTo(2,
            "AC-7: 啟用後 heartbeat log 帶每次都不同的 CorrelationId");
    }
}
