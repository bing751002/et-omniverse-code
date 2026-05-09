namespace ETOmniverse.Infrastructure.Tests.HttpOutbound;

using System.Net;
using System.Net.Http.Json;

internal sealed class SequenceHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>> _steps = new();

    public List<HttpRequestMessage> Requests { get; } = new();

    public int CallCount => Requests.Count;

    public SequenceHttpMessageHandler EnqueueStatus(HttpStatusCode statusCode, string message = "ok")
    {
        _steps.Enqueue((_, _) => Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(new { message })
        }));
        return this;
    }

    public SequenceHttpMessageHandler EnqueueTimeout()
    {
        _steps.Enqueue((_, _) => throw new TaskCanceledException("timeout"));
        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request);
        if (_steps.Count == 0)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { message = "default" })
            });
        }

        return _steps.Dequeue().Invoke(request, cancellationToken);
    }
}
