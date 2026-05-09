namespace ETOmniverse.Infrastructure.ExternalServices.SampleEcho;

using System.Net;
using System.Net.Http.Json;
using ETOmniverse.Common.Http;
using ETOmniverse.Domain.Common.Model;
using ETOmniverse.Domain.Common.Ports;

public sealed class SampleEchoClient : ISampleEchoPort
{
    private const string ServiceName = "SampleEcho";
    private const string PathTemplate = "/echo";
    private readonly HttpClient _httpClient;

    public SampleEchoClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<Result<SampleEchoResponse>> EchoAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"echo?message={Uri.EscapeDataString(message)}");
        request.Options.Set(OutboundHttpLogProperties.ServiceNameKey, ServiceName);
        request.Options.Set(OutboundHttpLogProperties.PathTemplateKey, PathTemplate);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var payload = await response.Content.ReadFromJsonAsync<SampleEchoPayload>(cancellationToken);
                return Result<SampleEchoResponse>.Success(new SampleEchoResponse(payload?.Message ?? ""));
            }

            return Result<SampleEchoResponse>.Failure(
                "sample_echo.http_failure",
                $"SampleEcho returned HTTP {(int)response.StatusCode}.",
                ErrorKind.ExternalDependency);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<SampleEchoResponse>.Failure(
                "sample_echo.timeout",
                "SampleEcho request timed out.",
                ErrorKind.ExternalDependency);
        }
        catch (HttpRequestException)
        {
            return Result<SampleEchoResponse>.Failure(
                "sample_echo.transport_failure",
                "SampleEcho transport failure.",
                ErrorKind.ExternalDependency);
        }
    }

    private sealed record SampleEchoPayload(string Message);
}
