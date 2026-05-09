namespace ETOmniverse.Domain.Common.Ports;

using ETOmniverse.Domain.Common.Model;

public interface ISampleEchoPort
{
    Task<Result<SampleEchoResponse>> EchoAsync(string message, CancellationToken cancellationToken = default);
}

public sealed record SampleEchoResponse(string Message);
