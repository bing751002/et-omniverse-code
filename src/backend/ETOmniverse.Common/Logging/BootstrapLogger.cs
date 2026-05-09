namespace ETOmniverse.Common.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

/// <summary>
/// DI container 建立前可用的 console fallback logger（per F-002 AC-6）。
/// 啟動異常被吞 = production 黑洞，這支保證 stderr 必有最後一筆。
/// </summary>
public static class BootstrapLogger
{
    public static ILogger Logger { get; private set; } = Serilog.Core.Logger.None;

    public static void Initialize()
    {
        Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(new RenderedCompactJsonFormatter(), standardErrorFromLevel: LogEventLevel.Error)
            .CreateLogger();
    }
}
