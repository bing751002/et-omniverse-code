namespace ETOmniverse.Domain.Common.Ports;

public interface IClock
{
  DateTimeOffset UtcNow { get; }
}
