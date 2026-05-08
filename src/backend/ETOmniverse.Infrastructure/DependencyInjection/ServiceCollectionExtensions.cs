using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ETOmniverse.Domain.Common.Ports;
using ETOmniverse.Infrastructure.Time;

namespace ETOmniverse.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddETOmniverseInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    _ = configuration;

    services.AddSingleton<IClock, SystemClock>();

    return services;
  }
}
