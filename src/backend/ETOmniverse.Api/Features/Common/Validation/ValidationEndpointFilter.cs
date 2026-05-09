namespace ETOmniverse.Api.Features.Common.Validation;

using System.Linq;
using System.Threading.Tasks;
using ETOmniverse.Api.Features.Common.ProblemDetails;
using ETOmniverse.Domain.Common.Model;
using FluentValidation;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Per-endpoint validation filter（per CONTEXT D-C1）。
/// 用法：app.MapPost(...).AddEndpointFilter&lt;ValidationEndpointFilter&lt;EchoRequest&gt;&gt;()
/// IValidator&lt;TRequest&gt; 未註冊 → noop（pass-through，避免「忘了寫 validator」誤殺，per CONTEXT Discretion）。
/// 失敗時透過共用 <see cref="ProblemDetailsExtensions.ToProblemDetails"/> 打包 RFC 7807，
/// errorCode 直送 "VALIDATION"（per D-B3 — Day 1 不建 ErrorCatalog 集中表）。
/// </summary>
public sealed class ValidationEndpointFilter<TRequest> : IEndpointFilter
    where TRequest : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var validator = ctx.HttpContext.RequestServices.GetService(typeof(IValidator<TRequest>)) as IValidator<TRequest>;
        if (validator is null) return await next(ctx);   // noop pass-through

        var request = ctx.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null) return await next(ctx);     // 無 TRequest 參數 → noop（保護性）

        var result = await validator.ValidateAsync(request);
        if (result.IsValid) return await next(ctx);

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var pd = ctx.HttpContext.ToProblemDetails(
            ErrorKind.Validation,
            "VALIDATION",
            "One or more validation errors occurred.",
            errors);

        // 用 Results.Json 而非 Results.Problem(pd) — 後者會走內建 ProblemDetailsService
        // 並用 Activity.TraceId 覆蓋我們設好的 traceId extension。Results.Json 直接 serialize
        // 不重組 ProblemDetails，且回傳 IResult 由框架負責寫入（避免 Response.WriteAsJsonAsync
        // 後再 return null 觸發「response already started」雙寫）。
        // ContentType 顯式傳 application/problem+json（避免 default application/json）。
        return Results.Json(
            pd,
            options: null,
            contentType: ProblemDetailsExtensions.ContentType,
            statusCode: pd.Status ?? StatusCodes.Status400BadRequest);
    }
}
