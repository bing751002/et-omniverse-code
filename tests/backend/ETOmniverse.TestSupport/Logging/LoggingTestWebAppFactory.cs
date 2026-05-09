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

    public LoggingTestWebAppFactory WithSetting(string key, string value)
    {
        _overrides[key] = value;
        return this;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
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
