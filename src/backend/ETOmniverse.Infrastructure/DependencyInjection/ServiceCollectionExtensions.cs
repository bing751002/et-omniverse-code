using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ETOmniverse.Domain.Common.Ports;
using ETOmniverse.Infrastructure.Http;
using ETOmniverse.Infrastructure.Identity;
using ETOmniverse.Infrastructure.Persistence;

namespace ETOmniverse.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddETOmniverseInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    _ = configuration;

    services.AddSingleton(TimeProvider.System);
    services.AddSingleton<ICurrentUser, AnonymousCurrentUser>();
    services.AddSingleton<ETOmniverse.Common.Logging.IBackgroundCorrelationScope,
                          ETOmniverse.Common.Logging.BackgroundCorrelationScope>();
    services.AddOutboundHttpClients(configuration);
    services.AddPersistence(configuration);

    return services;
  }

  private static IServiceCollection AddPersistence(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("Default");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
      throw new InvalidOperationException("ConnectionStrings:Default is required for persistence registration.");
    }

    services.AddDbContext<EtOmniverseDbContext>(options =>
      EtOmniverseDbContextFactory.ConfigureSqlServer(options, connectionString));
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped(typeof(IRepository<>), typeof(RepositoryBase<>));
    services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("mssql");

    return services;
  }
}
