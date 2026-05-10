namespace ETOmniverse.Infrastructure.Identity;

using System.Security.Claims;
using ETOmniverse.Domain.Common.Ports;
using Microsoft.AspNetCore.Http;

public sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public string DisplayName =>
        _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "anonymous";
}
