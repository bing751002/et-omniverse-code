namespace ETOmniverse.Common.Logging;

/// <summary>
/// Strongly-typed binding for the "Logging" config section
/// (separate from ASP.NET 內建的 Logging:LogLevel).
/// 故意不暴露 Mask.Fields 的 setter — baseline 不可被 appsettings 覆蓋。
/// </summary>
public sealed class LoggingOptions
{
    public const string SectionName = "Logging";

    public RequestBodyOptions RequestBody { get; init; } = new();
    public MaskOptions Mask { get; init; } = new();
    public HeartbeatOptions Heartbeat { get; init; } = new();

    public sealed class RequestBodyOptions
    {
        public bool Enabled { get; init; } = false;
        public int MaxBytes { get; init; } = 32 * 1024;   // 32 KB cap per spec
    }

    public sealed class MaskOptions
    {
        public string[] AdditionalFields { get; init; } = Array.Empty<string>();
    }

    public sealed class HeartbeatOptions
    {
        public bool Enabled { get; init; } = false;
        public int IntervalSeconds { get; init; } = 60;
    }
}
