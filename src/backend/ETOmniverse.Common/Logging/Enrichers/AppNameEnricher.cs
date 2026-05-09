namespace ETOmniverse.Common.Logging.Enrichers;

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Events;

public sealed class AppNameEnricher : ILogEventEnricher
{
    private readonly string _appName;

    public AppNameEnricher(IConfiguration cfg)
    {
        _appName = cfg["Log:ExtraInfo:AppName"]
                   ?? Assembly.GetEntryAssembly()?.GetName().Name
                   ?? "ETOmniverse";
    }

    public void Enrich(LogEvent evt, ILogEventPropertyFactory pf)
        => evt.AddPropertyIfAbsent(pf.CreateProperty("AppName", _appName));
}
