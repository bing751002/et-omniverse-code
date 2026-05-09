namespace ETOmniverse.Api.Features.Test.Auth;

using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// F-006 fixture endpoints — 僅在 IntegrationTest env 註冊。
/// 提供 [Authorize] 與 [Authorize(Roles="Admin")] 兩個最小 fixture，給 06-03 integration test
/// 與未來 v1.1+ 業務 phase 範本參考。
/// Phase 07 (D-22) 會把 path 收斂到 /api/test/auth/*，本 plan 暫用 /test/auth/* 對齊現有
/// /test/throw / /test/echo 慣例。
/// </summary>
public static class TestAuthEndpoints
{
    public static IEndpointRouteBuilder MapTestAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // [Authorize] — 任何 authenticated user 可進
        app.MapGet("/test/auth/whoami", (ClaimsPrincipal user) =>
            Results.Ok(new { name = user.Identity?.Name, authenticated = user.Identity?.IsAuthenticated ?? false }))
           .RequireAuthorization()
           .WithTags("Test/Auth")
           .WithName("TestAuthWhoAmI");

        // [Authorize(Roles = "Admin")] — 必須有 Admin role
        app.MapGet("/test/auth/admin", (ClaimsPrincipal user) =>
            Results.Ok(new { name = user.Identity?.Name, role = "Admin" }))
           .RequireAuthorization(policy => policy.RequireRole("Admin"))
           .WithTags("Test/Auth")
           .WithName("TestAuthAdmin");

        return app;
    }
}
