namespace ETOmniverse.Api.Middleware;

using System;
using System.Threading.Tasks;
using ETOmniverse.Common.Http;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = CorrelationIdConstants.HeaderName;
    public const string LogProperty = CorrelationIdConstants.LogProperty;

    private readonly RequestDelegate _next;
    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        var incoming = ctx.Request.Headers[HeaderName].ToString();
        var id = string.IsNullOrWhiteSpace(incoming) ? Guid.NewGuid().ToString("N") : incoming;

        ctx.Response.Headers[HeaderName] = id;
        ctx.Items[LogProperty] = id;

        using (LogContext.PushProperty(LogProperty, id))
        {
            await _next(ctx);
        }
    }
}
