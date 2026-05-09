namespace ETOmniverse.Common.Tests.Logging;

using ETOmniverse.Common.Logging;
using FluentAssertions;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using System.Collections.Concurrent;
using Xunit;

[Collection("LoggingUnitTests")]
public class BackgroundCorrelationScopeTests
{
    /// <summary>
    /// Helper — 建立只有一個 InMemorySink 的 logger，不加 enricher 之外的 sink。
    /// 使用 ConcurrentQueue 模擬 sink 確保不依賴 InMemorySink 版本 API。
    /// </summary>
    private sealed class CaptureSink : ILogEventSink
    {
        private readonly ConcurrentQueue<LogEvent> _events = new();
        public IEnumerable<LogEvent> Events => _events;
        public void Emit(LogEvent logEvent) => _events.Enqueue(logEvent);
    }

    [Fact]
    public void Begin_pushes_new_CorrelationId_then_restores_on_dispose()
    {
        var sink = new CaptureSink();
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Sink(sink)
            .CreateLogger();

        var scope = new BackgroundCorrelationScope();

        logger.Information("before");
        using (scope.Begin())
        {
            logger.Information("inside");
        }
        logger.Information("after");

        var events = sink.Events.ToList();
        events.Should().HaveCount(3);
        events[0].Properties.ContainsKey("CorrelationId").Should().BeFalse();
        events[1].Properties.ContainsKey("CorrelationId").Should().BeTrue();
        events[2].Properties.ContainsKey("CorrelationId").Should().BeFalse();
    }

    [Fact]
    public void Begin_each_call_generates_different_id()
    {
        var sink = new CaptureSink();
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Sink(sink)
            .CreateLogger();

        var scope = new BackgroundCorrelationScope();
        using (scope.Begin()) logger.Information("first");
        using (scope.Begin()) logger.Information("second");

        var ids = sink.Events
            .Select(e => e.Properties["CorrelationId"].ToString())
            .Distinct();
        ids.Should().HaveCount(2);
    }
}
