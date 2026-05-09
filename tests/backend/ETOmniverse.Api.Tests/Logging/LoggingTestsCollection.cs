namespace ETOmniverse.Api.Tests.Logging;

using Xunit;

/// <summary>
/// Logging integration test collection — 序列化執行，避免 WebApplicationFactory + static Serilog logger 的平行 race condition。
/// BootstrapLogger 是 static，多個 WAF 平行起動時可能互相覆蓋 Logger instance。
/// </summary>
[CollectionDefinition("LoggingTests", DisableParallelization = true)]
public class LoggingTestsCollection { }
