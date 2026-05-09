namespace ETOmniverse.Api.Features.Common.Ping;

using ETOmniverse.Api.Features.Common.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// F-003 AC-7 / AC-8 — Common Ping sample。Endpoint body 只做 binding /
/// validation / response mapping，不寫業務邏輯。後續業務模組 reference template。
/// </summary>
public static class PingEndpoints
{
    public static IEndpointRouteBuilder MapPingEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/common/ping → 200 { "message": "pong" }
        app.MapGet("/api/common/ping", () => Results.Ok(new { message = "pong" }))
           .WithTags("Common/Ping")
           .WithName("Ping");

        // POST /api/common/ping/echo → echo back valid message;
        // invalid → 400 ProblemDetails (via ValidationEndpointFilter)
        app.MapPost("/api/common/ping/echo", (EchoRequest req) => Results.Ok(new { message = req.Message }))
           .AddEndpointFilter<ValidationEndpointFilter<EchoRequest>>()
           .WithTags("Common/Ping")
           .WithName("PingEcho");

        return app;
    }

    /// <summary>
    /// F-003 AC-7 — 只在 IntegrationTest 環境註冊（per CONTEXT D-D4），
    /// 用來驗 GlobalExceptionHandler 5xx 路徑（AC-2）。其他環境呼叫得 404。
    /// </summary>
    public static IEndpointRouteBuilder MapPingFailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/common/ping/fail", (HttpContext _) =>
        {
            throw new System.InvalidOperationException("intentional ping/fail for AC-2 5xx integration test");
        })
           .WithTags("Common/Ping")
           .WithName("PingFail");

        return app;
    }
}
