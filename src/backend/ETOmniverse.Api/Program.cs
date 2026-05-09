using ETOmniverse.Api.Authentication.Test;
using ETOmniverse.Api.Features.Common.Health;
using ETOmniverse.Api.Features.Common.Ping;
using ETOmniverse.Api.Features.Test;
using ETOmniverse.Api.Middleware;
using ETOmniverse.Common.Logging;
using ETOmniverse.Infrastructure.DependencyInjection;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Serilog;

BootstrapLogger.Initialize();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog(SerilogSetup.Configure);

    // F-003 AC-6: OpenAPI metadata（title / version / description），document name "v1"
    builder.Services.AddOpenApi("v1", options =>
    {
        options.AddDocumentTransformer((document, context, ct) =>
        {
            document.Info.Title = "ET-Omniverse API";
            document.Info.Version = "v1";
            document.Info.Description = "ET-Omniverse inbound HTTP API（F-003 inbound base）";
            return Task.CompletedTask;
        });
    });
    // F-007 AC-A1 / D-20: TimeProvider 全棧強制 — 所有業務 code 走 DI 取得 TimeProvider
    builder.Services.AddSingleton(TimeProvider.System);
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

    // F-003 AC-5: CORS policy — Dev allow-all / Prod 白名單 / 未設定 fail-closed
    // 注意：AllowedOrigins 在 AddPolicy lambda 內 resolve（lambda 在 CORS service build 時才呼叫，
    // WAF 透過 ConfigureAppConfiguration 注入的 in-memory source 此時已 merge 完成；
    // 若在 service registration 階段讀 builder.Configuration，WAF 覆蓋尚未套用會抓不到 allowlist）
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Default", policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            }
            else
            {
                var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                if (corsAllowedOrigins.Length > 0)
                {
                    policy.WithOrigins(corsAllowedOrigins).AllowAnyMethod().AllowAnyHeader();
                }
                // else: 不加 rule → fail-closed（cross-origin 不會收到 Access-Control-Allow-Origin header）
            }
        });
    });

    // F-006 AC-7: Test authentication scheme 僅在 IntegrationTest 環境註冊。
    // Non-IntegrationTest env：完全不註冊 Authentication（v1.0 還沒有真實 auth scheme，業務 endpoint 也尚未掛 [Authorize]）。
    if (builder.Environment.IsEnvironment("IntegrationTest"))
    {
        builder.Services
            .AddAuthentication(TestAuthenticationDefaults.AuthenticationScheme)
            .AddTestAuthentication();
        builder.Services.AddAuthorization();
    }

    var app = builder.Build();

    // F-006 AC-7: Production / Staging / Development 啟動時若 Test auth scheme 已註冊 → hard-fail。
    // 防止 config / refactor 意外把 test bypass 帶到 prod。
    if (!app.Environment.IsEnvironment("IntegrationTest"))
    {
        var schemeProvider = app.Services.GetService<IAuthenticationSchemeProvider>();
        if (schemeProvider is not null)
        {
            var testScheme = await schemeProvider.GetSchemeAsync(TestAuthenticationDefaults.AuthenticationScheme);
            if (testScheme is not null)
            {
                throw new InvalidOperationException("Test authentication scheme MUST NOT be registered outside IntegrationTest environment.");
            }
        }
    }

    // F-003 AC-6: OpenAPI policy — config 為主，未設則 fallback IsDevelopment（Production 預設 false fail-closed）
    var openApiEnabled = builder.Configuration.GetValue<bool?>("OpenApi:Enabled") ?? app.Environment.IsDevelopment();
    if (openApiEnabled)
    {
        app.MapOpenApi();
    }

    // F-002: CorrelationIdMiddleware 必須早於 RequestLoggingMiddleware（spec 硬規則）
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    // F-003 AC-2/AC-4: ExceptionHandler 必須在 CorrelationIdMiddleware 之後（traceId 才能讀到）；
    // 放在 RequestLoggingMiddleware 之後讓 RequestLoggingMiddleware 仍能記 Error 級 summary log
    app.UseExceptionHandler();

    // F-003 AC-5: 必須在 ExceptionHandler 之後、endpoint 之前
    app.UseCors("Default");

    // F-006: Authentication / Authorization pipeline — 僅 IntegrationTest 環境啟用（無註冊則略過）
    if (app.Environment.IsEnvironment("IntegrationTest"))
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }

    app.MapETOmniverseHealthEndpoints();

    // F-003 AC-7: Common Ping sample（GET /api/common/ping、POST /api/common/ping/echo）
    app.MapPingEndpoints();

    // F-003 AC-7: ping/fail 僅 IntegrationTest 環境註冊（D-22 白名單，沿用既有 namespace）
    if (app.Environment.IsEnvironment("IntegrationTest"))
    {
        app.MapPingFailEndpoint();
    }

    // F-007 AC-C2 / D-22: 集中註冊 test-only endpoints（/api/test/*）
    // 雙線防護：caller if（此處）+ extension 第一行 hard-fail throw
    if (app.Environment.IsEnvironment("IntegrationTest"))
    {
        app.MapTestOnlyEndpoints();
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
