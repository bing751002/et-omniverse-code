namespace ETOmniverse.Infrastructure.Tests.HttpOutbound;

using ETOmniverse.Common.Http;
using ETOmniverse.Infrastructure.ExternalServices.SampleEcho;
using ETOmniverse.Infrastructure.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

internal static class HttpOutboundTestClientFactory
{
    public static SampleEchoClient Create(
        SequenceHttpMessageHandler primaryHandler,
        string? correlationId = "corr-test",
        ExternalServiceOptions? options = null)
    {
        var httpContextAccessor = new HttpContextAccessor();
        if (correlationId is not null)
        {
            httpContextAccessor.HttpContext = new DefaultHttpContext();
            httpContextAccessor.HttpContext.Items[CorrelationIdConstants.LogProperty] = correlationId;
        }

        var resilience = new ResilientHttpClientHandler(
            new StaticOptionsMonitor<ExternalServiceOptions>(options ?? new ExternalServiceOptions
            {
                BaseUrl = "http://sample.test",
                TimeoutSeconds = 1,
                Retry = new RetryOptions { MaxAttempts = 3, BaseDelayMs = 1 }
            }))
        {
            InnerHandler = primaryHandler
        };

        var logging = new OutboundHttpLoggingHandler(NullLogger<OutboundHttpLoggingHandler>.Instance)
        {
            InnerHandler = resilience
        };

        var correlation = new CorrelationIdPropagationHandler(httpContextAccessor)
        {
            InnerHandler = logging
        };

        return new SampleEchoClient(new HttpClient(correlation)
        {
            BaseAddress = new Uri("http://sample.test")
        });
    }
}
