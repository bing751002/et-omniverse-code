using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ETOmniverse.Domain.Common.Ports;
using ETOmniverse.Infrastructure.Identity;
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
    services.AddSingleton<ICurrentUser, AnonymousCurrentUser>();
    services.AddSingleton<ETOmniverse.Common.Logging.IBackgroundCorrelationScope,
                          ETOmniverse.Common.Logging.BackgroundCorrelationScope>();

    return services;
  }
}
