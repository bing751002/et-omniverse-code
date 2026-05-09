namespace ETOmniverse.Infrastructure.Http;

using System.ComponentModel.DataAnnotations;

public sealed class ExternalServiceOptions
{
    public const string SectionName = "ExternalServices";

    [Required]
    public string BaseUrl { get; init; } = "";

    [Range(1, 300)]
    public int TimeoutSeconds { get; init; } = 10;

    public RetryOptions Retry { get; init; } = new();

    public CircuitBreakerOptions CircuitBreaker { get; init; } = new();
}

public sealed class RetryOptions
{
    [Range(1, 10)]
    public int MaxAttempts { get; init; } = 3;

    [Range(1, 60_000)]
    public int BaseDelayMs { get; init; } = 200;
}

public sealed class CircuitBreakerOptions
{
    public bool Enabled { get; init; } = false;
}
