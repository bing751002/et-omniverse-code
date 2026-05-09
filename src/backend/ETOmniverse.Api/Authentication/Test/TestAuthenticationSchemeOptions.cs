namespace ETOmniverse.Api.Authentication.Test;

using Microsoft.AspNetCore.Authentication;

/// <summary>
/// F-006 AC-1 — Test authentication scheme options。
/// 預設讀 <see cref="TestAuthenticationDefaults.UserHeaderName"/> /
/// <see cref="TestAuthenticationDefaults.RoleHeaderName"/>，可由 caller 透過
/// <c>AddTestAuthentication(opts =&gt; opts.UserHeaderName = "...")</c> 覆寫。
/// </summary>
public class TestAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public string UserHeaderName { get; set; } = TestAuthenticationDefaults.UserHeaderName;

    public string RoleHeaderName { get; set; } = TestAuthenticationDefaults.RoleHeaderName;
}
