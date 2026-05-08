using ETOmniverse.Api.Features.Common.Health;
using ETOmniverse.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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

public partial class Program;
