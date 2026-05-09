namespace ETOmniverse.Infrastructure.Identity;

using ETOmniverse.Domain.Common.Ports;

public sealed class AnonymousCurrentUser : ICurrentUser
{
    public string? UserId => null;
    public bool IsAuthenticated => false;
    public string DisplayName => "anonymous";
}
