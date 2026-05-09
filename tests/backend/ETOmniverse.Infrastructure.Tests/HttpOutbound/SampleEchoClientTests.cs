namespace ETOmniverse.Infrastructure.Tests.HttpOutbound;

using System.Net;
using ETOmniverse.Domain.Common.Model;
using FluentAssertions;

public sealed class SampleEchoClientTests
{
    [Fact]
    public async Task EchoAsync_PropagatesCorrelationIdAndMapsSuccess()
    {
        var fake = new SequenceHttpMessageHandler()
            .EnqueueStatus(HttpStatusCode.OK, "hello");
        var client = HttpOutboundTestClientFactory.Create(fake, "corr-123");

        var result = await client.EchoAsync("hello");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Message.Should().Be("hello");
        fake.Requests.Single().Headers.GetValues("X-Correlation-Id").Single().Should().Be("corr-123");
        fake.Requests.Single().RequestUri!.Query.Should().Contain("message=hello");
    }

    [Fact]
    public async Task EchoAsync_DoesNotRetryOrdinaryBadRequest()
    {
        var fake = new SequenceHttpMessageHandler()
            .EnqueueStatus(HttpStatusCode.BadRequest);
        var client = HttpOutboundTestClientFactory.Create(fake);

        var result = await client.EchoAsync("bad");

        result.IsSuccess.Should().BeFalse();
        result.Kind.Should().Be(ErrorKind.ExternalDependency);
        result.Code.Should().Be("sample_echo.http_failure");
        fake.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task EchoAsync_RetriesTransientServerError()
    {
        var fake = new SequenceHttpMessageHandler()
            .EnqueueStatus(HttpStatusCode.InternalServerError)
            .EnqueueStatus(HttpStatusCode.InternalServerError)
            .EnqueueStatus(HttpStatusCode.OK, "eventual");
        var client = HttpOutboundTestClientFactory.Create(fake);

        var result = await client.EchoAsync("retry");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Message.Should().Be("eventual");
        fake.CallCount.Should().Be(3);
    }

    [Fact]
    public async Task EchoAsync_MapsTimeoutToExternalDependencyFailure()
    {
        var fake = new SequenceHttpMessageHandler()
            .EnqueueTimeout()
            .EnqueueTimeout()
            .EnqueueTimeout();
        var client = HttpOutboundTestClientFactory.Create(fake);

        var result = await client.EchoAsync("timeout");

        result.IsSuccess.Should().BeFalse();
        result.Kind.Should().Be(ErrorKind.ExternalDependency);
        result.Code.Should().Be("sample_echo.timeout");
        fake.CallCount.Should().Be(3);
    }
}
