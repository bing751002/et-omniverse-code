namespace ETOmniverse.Infrastructure.Http;

using ETOmniverse.Common.Http;
using Microsoft.Extensions.Options;

public sealed class ResilientHttpClientHandler : DelegatingHandler
{
    private readonly IOptionsMonitor<ExternalServiceOptions> _optionsMonitor;

    public ResilientHttpClientHandler(IOptionsMonitor<ExternalServiceOptions> optionsMonitor) =>
        _optionsMonitor = optionsMonitor;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var options = GetOptions(request);
        Exception? lastException = null;
        var attempts = Math.Max(1, options.Retry.MaxAttempts);

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));

            var cloned = attempt == 1 ? request : await CloneAsync(request, cancellationToken);
            cloned.Options.Set(OutboundHttpLogProperties.RetryAttemptKey, attempt - 1);

            try
            {
                var response = await base.SendAsync(cloned, timeoutCts.Token);
                request.Options.Set(OutboundHttpLogProperties.RetryAttemptKey, attempt - 1);

                if (!IsRetryable(response.StatusCode) || attempt == attempts)
                {
                    return response;
                }

                response.Dispose();
            }
            catch (Exception ex) when (IsRetryableException(ex, cancellationToken) && attempt < attempts)
            {
                lastException = ex;
                request.Options.Set(OutboundHttpLogProperties.RetryAttemptKey, attempt);
            }

            await Task.Delay(GetDelay(options, attempt), cancellationToken);
        }

        throw lastException ?? new HttpRequestException("Outbound request failed after retry attempts.");
    }

    private static bool IsRetryable(System.Net.HttpStatusCode statusCode)
    {
        var code = (int)statusCode;
        return code >= 500 || code is 408 or 429;
    }

    private static bool IsRetryableException(Exception ex, CancellationToken callerToken) =>
        !callerToken.IsCancellationRequested &&
        (ex is TaskCanceledException or TimeoutException or HttpRequestException);

    private ExternalServiceOptions GetOptions(HttpRequestMessage request)
    {
        var serviceName = request.Options.TryGetValue(OutboundHttpLogProperties.ServiceNameKey, out var service)
            ? service
            : "SampleEcho";
        return _optionsMonitor.Get(serviceName);
    }

    private static TimeSpan GetDelay(ExternalServiceOptions options, int attempt)
    {
        var baseDelay = Math.Max(1, options.Retry.BaseDelayMs);
        var exponential = baseDelay * Math.Pow(2, Math.Max(0, attempt - 1));
        var jitter = Random.Shared.Next(0, baseDelay);
        return TimeSpan.FromMilliseconds(exponential + jitter);
    }

    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            clone.Content = new ByteArrayContent(bytes);
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}
