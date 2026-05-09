namespace ETOmniverse.Common.Http;

using System.Diagnostics;
using Microsoft.Extensions.Logging;

public sealed class OutboundHttpLoggingHandler : DelegatingHandler
{
    private readonly ILogger<OutboundHttpLoggingHandler> _logger;

    public OutboundHttpLoggingHandler(ILogger<OutboundHttpLoggingHandler> logger) =>
        _logger = logger;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            sw.Stop();

            Log(request, response.StatusCode, sw.ElapsedMilliseconds, null);
            return response;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            sw.Stop();
            Log(request, null, sw.ElapsedMilliseconds, ex);
            throw;
        }
    }

    private void Log(HttpRequestMessage request, System.Net.HttpStatusCode? statusCode, long durationMs, Exception? exception)
    {
        var serviceName = request.Options.TryGetValue(OutboundHttpLogProperties.ServiceNameKey, out var service)
            ? service
            : "unknown";
        var pathTemplate = request.Options.TryGetValue(OutboundHttpLogProperties.PathTemplateKey, out var template)
            ? template
            : request.RequestUri?.AbsolutePath ?? "";
        var retryAttempt = request.Options.TryGetValue(OutboundHttpLogProperties.RetryAttemptKey, out var attempt)
            ? attempt
            : 0;
        var outcome = exception is not null
            ? "exception"
            : statusCode is >= System.Net.HttpStatusCode.BadRequest ? "failure" : "success";

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["serviceName"] = serviceName,
            ["method"] = request.Method.Method,
            ["host"] = request.RequestUri?.Host ?? "",
            ["pathTemplate"] = pathTemplate,
            ["statusCode"] = statusCode is null ? null : (int)statusCode.Value,
            ["durationMs"] = durationMs,
            ["retryAttempt"] = retryAttempt,
            ["outcome"] = outcome
        });

        if (exception is null)
        {
            _logger.LogInformation(
                "OUTBOUND {ServiceName} {Method} {Host}{PathTemplate} -> {StatusCode} ({DurationMs} ms) retry={RetryAttempt} outcome={Outcome}",
                serviceName,
                request.Method.Method,
                request.RequestUri?.Host ?? "",
                pathTemplate,
                statusCode is null ? 0 : (int)statusCode.Value,
                durationMs,
                retryAttempt,
                outcome);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "OUTBOUND {ServiceName} {Method} {Host}{PathTemplate} -> exception ({DurationMs} ms) retry={RetryAttempt} outcome={Outcome}",
                serviceName,
                request.Method.Method,
                request.RequestUri?.Host ?? "",
                pathTemplate,
                durationMs,
                retryAttempt,
                outcome);
        }
    }
}
