namespace ETOmniverse.Api.Features.Common.Ping;

/// <summary>
/// F-003 AC-7 — POST /api/common/ping/echo 的 request DTO。
/// 規則 by EchoRequestValidator：Message 1-50 chars。
/// </summary>
public sealed record EchoRequest(string Message);
