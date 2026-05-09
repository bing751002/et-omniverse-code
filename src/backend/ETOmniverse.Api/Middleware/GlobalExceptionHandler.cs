namespace ETOmniverse.Api.Middleware;

using System;
using System.Threading;
using System.Threading.Tasks;
using ETOmniverse.Api.Features.Common.ProblemDetails;
using ETOmniverse.Domain.Common.Model;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// F-003 AC-2：未 catch exception → 500 ProblemDetails + Error 級 log。
/// 走 .NET 8+ 內建 IExceptionHandler 路線（per CONTEXT D-B1），由 Program.cs
/// 的 AddExceptionHandler&lt;GlobalExceptionHandler&gt;() 註冊。
/// 紀律：exception 整個傳給 logger（stack trace 落 log 含 CorrelationId via LogContext），
/// 但 ProblemDetails Detail 是 hardcoded 通用訊息，不外洩 exception.Message / StackTrace。
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private const string UnexpectedCode = "UNEXPECTED";
    private const string SafeDetail = "An unexpected error occurred.";

    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var pd = httpContext.ToProblemDetails(
            ErrorKind.Unexpected,
            UnexpectedCode,
            SafeDetail);

        httpContext.Response.StatusCode = pd.Status ?? StatusCodes.Status500InternalServerError;

        // ASP.NET ExceptionHandlerMiddleware 接管時會 Response.Clear()，X-Correlation-Id
        // 會被刷掉；本 handler 從 ctx.Items 重新寫回，確保 AC-4 response header 與
        // ProblemDetails.traceId 永遠對齊
        var corrId = httpContext.Items[CorrelationIdMiddleware.LogProperty]?.ToString();
        if (!string.IsNullOrEmpty(corrId))
        {
            httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName] = corrId;
        }

        // 注意：WriteAsJsonAsync 會用 contentType 參數覆蓋 Response.ContentType；
        // 顯式傳 application/problem+json 避免被 default application/json 蓋掉
        await httpContext.Response.WriteAsJsonAsync(
            pd,
            options: null,
            contentType: ProblemDetailsExtensions.ContentType,
            cancellationToken: cancellationToken);
        return true;
    }
}
