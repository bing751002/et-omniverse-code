namespace ETOmniverse.Api.Features.Common.Health;

public static class HealthEndpointExtensions
{
  public static IEndpointRouteBuilder MapETOmniverseHealthEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapGet("/", () => Results.Redirect("/health/live"));
    app.MapGet("/health/live", () => Results.Ok(new { status = "live", service = "ETOmniverse.Api" }));
    app.MapHealthChecks("/health/ready");

    return app;
  }
}
