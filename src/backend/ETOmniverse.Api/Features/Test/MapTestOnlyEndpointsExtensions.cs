namespace ETOmniverse.Api.Features.Test;

using System;
using System.IO;
using ETOmniverse.Api.Features.Test.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

/// <summary>
/// F-007 / D-22: Centralised registration for test-only endpoints under <c>/api/test/*</c>.
/// First line guards against accidental Production deployment; caller is also expected
/// to wrap the call in <c>if (app.Environment.IsEnvironment("IntegrationTest"))</c> for
/// defence in depth (caller if + extension first-line throw — 雙線防護 per 07-CONTEXT).
///
/// 例外白名單：<c>/api/common/ping/fail</c>（F-003 ping sample）不走本 extension，
/// 沿用既有 namespace per D-22。
/// </summary>
public static class MapTestOnlyEndpointsExtensions
{
    public const string HardFailMessage =
        "Test-only endpoints MUST NOT be mapped outside IntegrationTest environment.";

    public static WebApplication MapTestOnlyEndpoints(this WebApplication app)
    {
        if (!app.Environment.IsEnvironment("IntegrationTest"))
        {
            throw new InvalidOperationException(HardFailMessage);
        }

        // Migrated from Program.cs inline block (F-002 AC-3 B2) — /test/throw → /api/test/throw
        app.MapGet("/api/test/throw", (HttpContext _) =>
        {
            throw new InvalidOperationException(
                "intentional test exception for AC-3 5xx level verification");
        });

        // /test/echo → /api/test/echo
        app.MapPost("/api/test/echo", async (HttpContext ctx) =>
        {
            // 讀完 body（讓 RequestLoggingMiddleware 已做完 buffering capture），回 200
            using var reader = new StreamReader(ctx.Request.Body);
            _ = await reader.ReadToEndAsync();
            return Results.Ok();
        });

        app.MapGet("/api/test/echo", () => Results.Ok());

        // F-006 (Phase 06) test auth fixture endpoints — registered via shared extension on
        // IEndpointRouteBuilder（path 已在 TestAuthEndpoints.cs 同步遷移到 /api/test/auth/*）
        app.MapTestAuthEndpoints();

        return app;
    }
}
