namespace ETOmniverse.Common.Http;

using Microsoft.AspNetCore.Http;

public sealed class CorrelationIdPropagationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdPropagationHandler(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains(CorrelationIdConstants.HeaderName))
        {
            var correlationId = GetCurrentCorrelationId();
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                request.Headers.TryAddWithoutValidation(CorrelationIdConstants.HeaderName, correlationId);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }

    private string? GetCurrentCorrelationId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Items.TryGetValue(CorrelationIdConstants.LogProperty, out var value) == true)
        {
            return value?.ToString();
        }

        return context?.TraceIdentifier;
    }
}
