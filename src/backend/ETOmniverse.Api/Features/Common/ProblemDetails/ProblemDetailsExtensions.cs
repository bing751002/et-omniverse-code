namespace ETOmniverse.Api.Features.Common.ProblemDetails;

using System.Collections.Generic;
using ETOmniverse.Api.Middleware;
using ETOmniverse.Domain.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MvcProblem = Microsoft.AspNetCore.Mvc.ProblemDetails;

/// <summary>
/// Shared ProblemDetails factory（F-003 AC-1 / AC-2 / AC-4）— ResultHttpExtensions
/// 與 GlobalExceptionHandler 共用同一個 mapping 點：ErrorKind → HTTP status / title /
/// type URL / traceId 打包。Domain 端 Result.Failure(code, msg, kind) 透過此處轉成
/// RFC 7807 ProblemDetails。traceId 來源為 CorrelationIdMiddleware.LogProperty
/// （per CONTEXT canonical_refs — 不用 HttpContext.TraceIdentifier，後者是 connection-level id）。
/// </summary>
public static class ProblemDetailsExtensions
{
    public const string DefaultTypeBaseUrl = "https://et-omniverse/errors";
    public const string ContentType = "application/problem+json";

    /// <summary>ErrorKind → HTTP status mapping（API 層唯一 mapping 點）。</summary>
    public static int ToHttpStatus(this ErrorKind kind) => kind switch
    {
        ErrorKind.Validation         => StatusCodes.Status400BadRequest,
        ErrorKind.Unauthorized       => StatusCodes.Status401Unauthorized,
        ErrorKind.Forbidden          => StatusCodes.Status403Forbidden,
        ErrorKind.NotFound           => StatusCodes.Status404NotFound,
        ErrorKind.Conflict           => StatusCodes.Status409Conflict,
        ErrorKind.ExternalDependency => StatusCodes.Status502BadGateway,
        ErrorKind.Unexpected         => StatusCodes.Status500InternalServerError,
        _                            => StatusCodes.Status500InternalServerError
    };

    /// <summary>ErrorKind → 短 title（ProblemDetails.Title）。</summary>
    public static string ToTitle(this ErrorKind kind) => kind switch
    {
        ErrorKind.Validation         => "Validation failed",
        ErrorKind.Unauthorized       => "Unauthorized",
        ErrorKind.Forbidden          => "Forbidden",
        ErrorKind.NotFound           => "Not found",
        ErrorKind.Conflict           => "Conflict",
        ErrorKind.ExternalDependency => "Bad gateway",
        ErrorKind.Unexpected         => "Internal server error",
        _                            => "Internal server error"
    };

    /// <summary>
    /// 從 HttpContext + ErrorKind + code + message 打包 ProblemDetails。
    /// type = {Errors:TypeBaseUrl}/{code}（缺 config 時 fallback DefaultTypeBaseUrl）。
    /// traceId = HttpContext.Items[CorrelationIdMiddleware.LogProperty]。
    /// </summary>
    public static MvcProblem ToProblemDetails(
        this HttpContext ctx,
        ErrorKind kind,
        string code,
        string message,
        IDictionary<string, string[]>? errors = null)
    {
        var cfg = ctx.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
        var baseUrl = cfg?["Errors:TypeBaseUrl"] ?? DefaultTypeBaseUrl;
        var traceId = ctx.Items[CorrelationIdMiddleware.LogProperty]?.ToString() ?? "";

        var pd = new MvcProblem
        {
            Status = kind.ToHttpStatus(),
            Type = $"{baseUrl}/{code}",
            Title = kind.ToTitle(),
            Detail = message,
            Instance = ctx.Request.Path
        };
        pd.Extensions["traceId"] = traceId;
        pd.Extensions["code"] = code;
        if (errors is not null && errors.Count > 0)
        {
            pd.Extensions["errors"] = errors;
        }
        return pd;
    }
}
