using ETOmniverse.Domain.Common.Ports;

namespace ETOmniverse.Infrastructure.Time;

public sealed class SystemClock : IClock
{
  public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
