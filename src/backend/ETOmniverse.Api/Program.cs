using ETOmniverse.Api.Features.Common.Health;
using ETOmniverse.Common.Logging;
using ETOmniverse.Infrastructure.DependencyInjection;
using Serilog;

BootstrapLogger.Initialize();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog(SerilogSetup.Configure);

    builder.Services.AddOpenApi();
    builder.Services.AddHealthChecks();
    builder.Services.AddETOmniverseInfrastructure(builder.Configuration);

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.MapETOmniverseHealthEndpoints();

    app.Run();
}
catch (Exception ex)
{
    BootstrapLogger.Logger.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
