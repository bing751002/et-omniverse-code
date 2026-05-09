using ETOmniverse.Api.Features.Common.Health;
using ETOmniverse.Api.Features.Common.Ping;
using ETOmniverse.Api.Middleware;
using ETOmniverse.Common.Logging;
using ETOmniverse.Infrastructure.DependencyInjection;
using FluentValidation;
using Serilog;

BootstrapLogger.Initialize();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog(SerilogSetup.Configure);

    builder.Services.AddOpenApi();
    builder.Services.AddHealthChecks();
    builder.Services.AddETOmniverseInfrastructure(builder.Configuration);

    // F-002 AC-2/AC-3/AC-4: strongly-typed options binding for request logging
    builder.Services.Configure<LoggingOptions>(
        builder.Configuration.GetSection(LoggingOptions.SectionName));

    // F-002 AC-7: 驗收用 heartbeat — 永遠 register，service 自己讀 Enabled 旗標決定是否跑
    // 不在 builder 階段判斷是因為 WAF integration test 需要在 CreateHost 階段才覆蓋 appsettings
    builder.Services.AddHostedService<ETOmniverse.Common.Logging.LoggingHeartbeatHostedService>();

    // F-003 AC-2: GlobalExceptionHandler — 走內建 IExceptionHandler 路線（per CONTEXT D-B1）
    builder.Services.AddExceptionHandler<ETOmniverse.Api.Middleware.GlobalExceptionHandler>();
    builder.Services.AddProblemDetails(); // 啟用 RFC 7807 ProblemDetails 預設機制

    // F-003 AC-3: FluentValidation assembly scanning（per CONTEXT D-C2）
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // F-002: CorrelationIdMiddleware 必須早於 RequestLoggingMiddleware（spec 硬規則）
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    // F-003 AC-2/AC-4: ExceptionHandler 必須在 CorrelationIdMiddleware 之後（traceId 才能讀到）；
    // 放在 RequestLoggingMiddleware 之後讓 RequestLoggingMiddleware 仍能記 Error 級 summary log
    app.UseExceptionHandler();

    app.MapETOmniverseHealthEndpoints();

    // F-003 AC-7: Common Ping sample（GET /api/common/ping、POST /api/common/ping/echo）
    app.MapPingEndpoints();

    // F-003 AC-7: ping/fail 僅 IntegrationTest 環境註冊（沿用 Phase 02 環境 guard 模式）
    if (app.Environment.IsEnvironment("IntegrationTest"))
    {
        app.MapPingFailEndpoint();
    }

    // F-002 AC-3 B2: IntegrationTest 環境限定的測試 endpoint（5xx 機械驗證用）
    if (app.Environment.IsEnvironment("IntegrationTest"))
    {
        app.MapGet("/test/throw", (HttpContext _) =>
        {
            throw new InvalidOperationException("intentional test exception for AC-3 5xx level verification");
        });

        app.MapPost("/test/echo", async (HttpContext ctx) =>
        {
            // 讀完 body（讓 RequestLoggingMiddleware 已做完 buffering capture），回 200
            using var reader = new StreamReader(ctx.Request.Body);
            _ = await reader.ReadToEndAsync();
            return Results.Ok();
        });

        app.MapGet("/test/echo", () => Results.Ok());
    }

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
