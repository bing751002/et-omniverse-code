namespace ETOmniverse.Infrastructure.Http;

using ETOmniverse.Common.Http;
using ETOmniverse.Domain.Common.Ports;
using ETOmniverse.Infrastructure.ExternalServices.SampleEcho;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class HttpClientRegistrationExtensions
{
    public static IServiceCollection AddOutboundHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddTransient<CorrelationIdPropagationHandler>();
        services.AddTransient<OutboundHttpLoggingHandler>();
        services.AddTransient<ResilientHttpClientHandler>();

        services.AddOptions<ExternalServiceOptions>("SampleEcho")
            .Bind(configuration.GetSection($"{ExternalServiceOptions.SectionName}:SampleEcho"))
            .ValidateDataAnnotations()
            .Validate(o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _), "SampleEcho BaseUrl must be an absolute URI.")
            .ValidateOnStart();

        services.AddHttpClient<ISampleEchoPort, SampleEchoClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<ExternalServiceOptions>>()
                    .Get("SampleEcho");
                client.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddHttpMessageHandler<CorrelationIdPropagationHandler>()
            .AddHttpMessageHandler<OutboundHttpLoggingHandler>()
            .AddHttpMessageHandler<ResilientHttpClientHandler>();

        return services;
    }
}
