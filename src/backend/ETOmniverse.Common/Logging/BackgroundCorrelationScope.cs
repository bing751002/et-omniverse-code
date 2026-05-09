namespace ETOmniverse.Common.Logging;

using System;
using Serilog.Context;

public sealed class BackgroundCorrelationScope : IBackgroundCorrelationScope
{
    public IDisposable Begin() => Begin(Guid.NewGuid().ToString("N"));
    public IDisposable Begin(string id) =>
        LogContext.PushProperty("CorrelationId", id);
}
