namespace ETOmniverse.Api.Features.Common.ProblemDetails;

using ETOmniverse.Domain.Common.Model;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Result&lt;T&gt; / Result → IResult 統一 mapping（F-003 AC-1 API 半邊）。
/// Endpoint 寫法：return r.ToHttpResult(httpContext);
/// 成功路徑：Result&lt;T&gt;.Success(value) → 200 OK with body / null value → 204；
/// Result.Success() → 204 NoContent。失敗路徑：透過 ProblemDetailsExtensions
/// 打包成 RFC 7807 ProblemDetails（Content-Type application/problem+json）。
/// </summary>
public static class ResultHttpExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> r, HttpContext ctx)
    {
        if (r.IsSuccess)
        {
            return r.Value is null ? Results.NoContent() : Results.Ok(r.Value);
        }

        var pd = ctx.ToProblemDetails(
            r.Kind ?? ErrorKind.Unexpected,
            r.Code ?? "UNEXPECTED",
            r.Message ?? string.Empty);
        return Results.Problem(pd);
    }

    public static IResult ToHttpResult(this Result r, HttpContext ctx)
    {
        if (r.IsSuccess)
        {
            return Results.NoContent();
        }

        var pd = ctx.ToProblemDetails(
            r.Kind ?? ErrorKind.Unexpected,
            r.Code ?? "UNEXPECTED",
            r.Message ?? string.Empty);
        return Results.Problem(pd);
    }
}
