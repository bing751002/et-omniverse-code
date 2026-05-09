namespace ETOmniverse.Common.Tests.Logging;

using Xunit;

/// <summary>
/// Logging unit test collection — 序列化執行，避免 static Serilog Log.Logger 平行 race condition。
/// </summary>
[CollectionDefinition("LoggingUnitTests", DisableParallelization = true)]
public class LoggingUnitTestsCollection { }
