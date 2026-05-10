namespace ETOmniverse.Domain.Common.Ports;

/// <summary>
/// Request-scoped current user port. HTTP hosts populate it from the authenticated principal.
/// 本 port 不接 LogContext — F-002 不掛 UserId enricher（per spec In scope 段）。
/// </summary>
public interface ICurrentUser
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
    string DisplayName { get; }
}
