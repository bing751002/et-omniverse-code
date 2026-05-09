namespace ETOmniverse.Common.Logging;

using ETOmniverse.Common.Logging.Enrichers;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

public static class SerilogSetup
{
    public static void Configure(HostBuilderContext ctx, IServiceProvider sp, LoggerConfiguration cfg)
    {
        cfg.MinimumLevel.Information()
           .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
           .MinimumLevel.Override("System", LogEventLevel.Warning)
           .Enrich.FromLogContext()
           .Enrich.WithMachineName()
           .Enrich.WithProperty("EnvironmentName", ctx.HostingEnvironment.EnvironmentName)
           .Enrich.With(new AppNameEnricher(ctx.Configuration))
           .Enrich.With(new AppVersionEnricher())
           .ReadFrom.Configuration(ctx.Configuration)   // 允許 appsettings 覆寫 minimum level
           .WriteTo.Console(new RenderedCompactJsonFormatter(), standardErrorFromLevel: LogEventLevel.Error);

        // Serilog 自己壞掉時 last resort（per spec In scope: Serilog SelfLog → stderr）
        Serilog.Debugging.SelfLog.Enable(Console.Error);
    }
}
