namespace ETOmniverse.Infrastructure.Tests.HttpOutbound;

using System.ComponentModel.DataAnnotations;
using ETOmniverse.Infrastructure.Http;
using FluentAssertions;

public sealed class ExternalServiceOptionsTests
{
    [Fact]
    public void Defaults_MatchF004Contract()
    {
        var options = new ExternalServiceOptions { BaseUrl = "http://localhost" };

        options.TimeoutSeconds.Should().Be(10);
        options.Retry.MaxAttempts.Should().Be(3);
        options.Retry.BaseDelayMs.Should().Be(200);
        options.CircuitBreaker.Enabled.Should().BeFalse();
    }

    [Fact]
    public void BaseUrl_IsRequired()
    {
        var options = new ExternalServiceOptions();
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(options, new ValidationContext(options), results, true)
            .Should().BeFalse();
    }
}
