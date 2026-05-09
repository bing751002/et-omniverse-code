namespace ETOmniverse.Infrastructure.Persistence;

using Microsoft.Extensions.Diagnostics.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly EtOmniverseDbContext _dbContext;

    public DatabaseHealthCheck(EtOmniverseDbContext dbContext) => _dbContext = dbContext;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("MSSQL connection is available.")
                : HealthCheckResult.Unhealthy("MSSQL connection is unavailable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MSSQL connection check failed.", ex);
        }
    }
}
