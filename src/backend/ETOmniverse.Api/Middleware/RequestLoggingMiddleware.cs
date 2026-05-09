namespace ETOmniverse.Api.Middleware;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ETOmniverse.Common.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using Serilog.Events;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly LoggingOptions _opts;
    private readonly IReadOnlySet<string> _maskSet;

    public RequestLoggingMiddleware(RequestDelegate next, IOptions<LoggingOptions> opts)
    {
        _next = next;
        _opts = opts.Value;
        _maskSet = MaskFields.GetEffectiveSet(_opts.Mask.AdditionalFields);
    }

    public async Task Invoke(HttpContext ctx)
    {
        // /health 排除（per spec In scope）
        if (ctx.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
        {
            await _next(ctx);
            return;
        }

        var sw = Stopwatch.StartNew();
        string? capturedBody = null;
        bool truncated = false;
        long bodySize = 0;
        string? contentType = ctx.Request.ContentType;

        if (_opts.RequestBody.Enabled && ctx.Request.ContentLength is long len && len > 0)
        {
            ctx.Request.EnableBuffering();
            bodySize = len;

            if (len > _opts.RequestBody.MaxBytes)
            {
                truncated = true;
            }
            else if (contentType is not null && contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = new StreamReader(ctx.Request.Body, leaveOpen: true);
                var raw = await reader.ReadToEndAsync();
                ctx.Request.Body.Position = 0;
                capturedBody = MaskJsonBody(raw);
            }
            // 非 JSON：不讀內容、只記 size + contentType（structured property）
        }

        int statusCode = 0;
        Exception? caught = null;
        try
        {
            await _next(ctx);
            statusCode = ctx.Response.StatusCode;
        }
        catch (Exception ex)
        {
            // unhandled exception → status 視為 500，仍要記 Error 級別 summary log
            caught = ex;
            statusCode = 500;
        }
        finally
        {
            sw.Stop();
            var level = statusCode >= 500 ? LogEventLevel.Error
                       : statusCode >= 400 ? LogEventLevel.Warning
                       : LogEventLevel.Information;

            var maskedPath = MaskQueryString(ctx.Request.Path + ctx.Request.QueryString);

            // B4: 全部用 structured property push 進 LogContext，message template 只有核心 5 欄。
            // 這樣 LogEvent.Properties 會有 BodyTruncated / BodySize / ContentType / RequestBody 各自 first-class entry。
            using var _bt = LogContext.PushProperty("BodyTruncated", truncated);
            using var _bs = LogContext.PushProperty("BodySize", bodySize);
            using var _ct = LogContext.PushProperty("ContentType", contentType ?? "");
            using var _rb = capturedBody is not null
                ? LogContext.PushProperty("RequestBody", capturedBody)
                : (IDisposable)NullDisposable.Instance;

            Log.Write(level, caught,
                "HTTP {Method} {Path} -> {StatusCode} ({DurationMs} ms) corr={CorrelationId}",
                ctx.Request.Method,
                maskedPath,
                statusCode,
                sw.ElapsedMilliseconds,
                ctx.Items[CorrelationIdMiddleware.LogProperty] ?? "");
        }

        // 還原成原本行為：unhandled exception 仍要往上拋，讓 ASP.NET pipeline 寫 500 response
        if (caught is not null)
            ExceptionDispatchInfo.Capture(caught).Throw();
    }

    private string MaskJsonBody(string raw)
    {
        try
        {
            var node = JsonNode.Parse(raw);
            MaskNode(node);
            return node?.ToJsonString() ?? raw;
        }
        catch (JsonException) { return "<invalid json>"; }
    }

    private void MaskNode(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            var keys = obj.Select(p => p.Key).ToList();
            foreach (var key in keys)
            {
                if (_maskSet.Contains(key))
                    obj[key] = MaskFields.MaskedValue;
                else
                    MaskNode(obj[key]);
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
                MaskNode(item);
        }
    }

    private string MaskQueryString(string pathAndQuery)
    {
        // 對每個 mask field 做 query string 替換：?token=xxx → ?token=***
        var result = pathAndQuery;
        foreach (var f in _maskSet)
        {
            var pattern = new Regex($@"([?&]){Regex.Escape(f)}=[^&]*", RegexOptions.IgnoreCase);
            result = pattern.Replace(result, $"$1{f}={MaskFields.MaskedValue}");
        }
        return result;
    }

    private sealed class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();
        public void Dispose() { }
    }
}
