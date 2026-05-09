namespace ETOmniverse.Domain.Common.Ports;

/// <summary>
/// Day 1: AnonymousCurrentUser 永遠 anonymous。
/// Identity 模組（D14/D18）落地時切真實作。
/// 本 port 不接 LogContext — F-002 不掛 UserId enricher（per spec In scope 段）。
/// </summary>
public interface ICurrentUser
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
    string DisplayName { get; }
}
