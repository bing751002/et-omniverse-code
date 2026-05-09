namespace ETOmniverse.Api.Authentication.Test;

/// <summary>
/// F-006 AC-1 — Test authentication scheme 常數集中。Header 名稱字串
/// 集中於此供 Handler / Options / TestSupport 共用，避免散落導致誤用。
/// </summary>
public static class TestAuthenticationDefaults
{
    public const string AuthenticationScheme = "Test";

    public const string UserHeaderName = "X-Test-User";

    public const string RoleHeaderName = "X-Test-Roles";
}
