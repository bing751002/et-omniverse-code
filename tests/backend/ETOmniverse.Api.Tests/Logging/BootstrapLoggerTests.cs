namespace ETOmniverse.Api.Tests.Logging;

using System;
using System.IO;
using ETOmniverse.Common.Logging;
using FluentAssertions;
using Xunit;

public class BootstrapLoggerTests
{
    [Fact]
    public void Initialize_then_Fatal_writes_to_stderr()
    {
        var originalErr = Console.Error;
        using var sw = new StringWriter();
        Console.SetError(sw);

        try
        {
            BootstrapLogger.Initialize();
            BootstrapLogger.Logger.Fatal("startup failed: {Reason}", "invalid connection string");
            // 注意：只 flush BootstrapLogger 自己的 Logger instance，不呼叫 Serilog.Log.CloseAndFlush()
            // 以避免關閉其他測試共用的 global Serilog.Log.Logger
            (BootstrapLogger.Logger as IDisposable)?.Dispose();
        }
        finally
        {
            Console.SetError(originalErr);
        }

        sw.ToString().Should().Contain("startup failed");
    }
}
