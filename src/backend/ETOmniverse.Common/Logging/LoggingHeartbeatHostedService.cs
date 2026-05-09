namespace ETOmniverse.Common.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// 驗收用 BackgroundService — 證明 IBackgroundCorrelationScope helper 在背景路徑工作。
/// 預設 disabled — production 不跑（per CONTEXT D-04）。
/// 啟用後每 N 秒寫一行 heartbeat log，每次自建新 CorrelationId。
/// 用途：integration test 驗收 helper API 形狀，避免 helper 沒消費者就設計錯。
/// **不**作為 liveness 證據（liveness 用 healthcheck）。
/// </summary>
public sealed class LoggingHeartbeatHostedService : BackgroundService
{
    private readonly IBackgroundCorrelationScope _scope;
    private readonly ILogger<LoggingHeartbeatHostedService> _logger;
    private readonly LoggingOptions _opts;

    public LoggingHeartbeatHostedService(
        IBackgroundCorrelationScope scope,
        ILogger<LoggingHeartbeatHostedService> logger,
        IOptions<LoggingOptions> opts)
    {
        _scope = scope;
        _logger = logger;
        _opts = opts.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 預設 disabled — 未啟用時直接返回，不跑任何 tick（per CONTEXT D-04）
        if (!_opts.Heartbeat.Enabled) return;

        var interval = TimeSpan.FromSeconds(Math.Max(1, _opts.Heartbeat.IntervalSeconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            using (_scope.Begin())
            {
                _logger.LogInformation("heartbeat tick");
            }
            try { await Task.Delay(interval, stoppingToken); }
            catch (TaskCanceledException) { break; }
        }
    }
}
