namespace ETOmniverse.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public sealed class EtOmniverseDbContextFactory : IDesignTimeDbContextFactory<EtOmniverseDbContext>
{
    public EtOmniverseDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var apiPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "ETOmniverse.Api"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(apiPath) ? apiPath : Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.Ops.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Server=localhost;Database=ETOmniverse;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<EtOmniverseDbContext>();
        ConfigureSqlServer(options, connectionString);
        return new EtOmniverseDbContext(options.Options);
    }

    public static void ConfigureSqlServer(
        DbContextOptionsBuilder options,
        string connectionString)
    {
        options.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsAssembly(typeof(EtOmniverseDbContext).Assembly.FullName);
            sql.EnableRetryOnFailure();
        });
        options.UseSnakeCaseNamingConvention();
    }
}
