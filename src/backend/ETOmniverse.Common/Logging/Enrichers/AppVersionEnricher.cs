namespace ETOmniverse.Common.Logging.Enrichers;

using System.Reflection;
using Serilog.Core;
using Serilog.Events;

public sealed class AppVersionEnricher : ILogEventEnricher
{
    private readonly string _version;

    public AppVersionEnricher()
    {
        _version = Assembly.GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
            ?? Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
            ?? "0.0.0";
    }

    public void Enrich(LogEvent evt, ILogEventPropertyFactory pf)
        => evt.AddPropertyIfAbsent(pf.CreateProperty("AppVersion", _version));
}
