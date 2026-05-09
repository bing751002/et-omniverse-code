namespace ETOmniverse.Api.Authentication.Test;

using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

/// <summary>
/// F-006 AC-1 — Test authentication handler。讀 <c>X-Test-User</c> + 可選
/// <c>X-Test-Roles</c> header 構造 <see cref="ClaimsPrincipal"/>，僅供
/// IntegrationTest 環境使用。註冊 / env guard 統一收在 Program.cs，本
/// handler 不做 env 檢查（per CONTEXT locked decision）。
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(
        IOptionsMonitor<TestAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userHeaderName = Options.UserHeaderName;
        if (!Request.Headers.TryGetValue(userHeaderName, out var userValues)
            || StringValues.IsNullOrEmpty(userValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var user = userValues[0];
        if (string.IsNullOrWhiteSpace(user))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user),
            new(ClaimTypes.Name, user),
        };

        var roleHeaderName = Options.RoleHeaderName;
        if (Request.Headers.TryGetValue(roleHeaderName, out var roleValues)
            && !StringValues.IsNullOrEmpty(roleValues))
        {
            foreach (var raw in roleValues)
            {
                if (raw is null)
                {
                    continue;
                }

                var tokens = raw.Split(',');
                foreach (var token in tokens)
                {
                    var role = token.Trim();
                    if (string.IsNullOrEmpty(role))
                    {
                        continue;
                    }

                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
        }

        var identity = new ClaimsIdentity(claims, TestAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestAuthenticationDefaults.AuthenticationScheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
