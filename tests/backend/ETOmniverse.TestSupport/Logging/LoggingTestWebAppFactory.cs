namespace ETOmniverse.TestSupport.Logging;

using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.InMemory;

public class LoggingTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> _overrides = new();
    public InMemorySink Sink { get; } = new InMemorySink();

    /// <summary>
    /// 環境覆蓋：預設 IntegrationTest（讓 /test/throw 等 hook 生效）。
    /// 03-03-PLAN 新增 — PingEndpointsTests 需在 Development 與 IntegrationTest 兩 env 各驗一次。
    /// </summary>
    public string Environment { get; set; } = "IntegrationTest";

    public LoggingTestWebAppFactory WithSetting(string key, string value)
    {
        _overrides[key] = value;
        return this;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // 預設啟用 IntegrationTest 環境，讓 Program.cs 中的測試 endpoint 生效；
        // 透過 Environment property 可在 PingEndpointsTests 等場景切換 Development。
        builder.UseEnvironment(Environment);

        builder.ConfigureAppConfiguration(cfg =>
        {
            cfg.AddInMemoryCollection(_overrides);
        });

        builder.UseSerilog((ctx, sp, lc) =>
        {
            ETOmniverse.Common.Logging.SerilogSetup.Configure(ctx, sp, lc);
            lc.WriteTo.Sink(Sink);
        });

        return base.CreateHost(builder);
    }
}
