namespace ETOmniverse.Api.Tests.Authentication;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ETOmniverse.Api.Authentication.Test;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

/// <summary>
/// F-006 AC-1 unit-level 驗收 — 對 <see cref="TestAuthenticationHandler"/>
/// 直接 invoke（純 in-memory，不走 WebApplicationFactory）。
/// </summary>
public class TestAuthenticationHandlerTests
{
    private static async Task<TestAuthenticationHandler> CreateHandlerAsync(
        HttpContext context,
        TestAuthenticationSchemeOptions? options = null)
    {
        var opts = options ?? new TestAuthenticationSchemeOptions();
        var monitor = new TestOptionsMonitor<TestAuthenticationSchemeOptions>(opts);
        var handler = new TestAuthenticationHandler(monitor, NullLoggerFactory.Instance, UrlEncoder.Default);
        var scheme = new AuthenticationScheme(
            TestAuthenticationDefaults.AuthenticationScheme,
            displayName: null,
            handlerType: typeof(TestAuthenticationHandler));
        await handler.InitializeAsync(scheme, context);
        return handler;
    }

    private static HttpContext BuildContext(IDictionary<string, string?> headers)
    {
        var ctx = new DefaultHttpContext();
        foreach (var (key, value) in headers)
        {
            ctx.Request.Headers[key] = value;
        }
        return ctx;
    }

    [Fact]
    public async Task HandleAuthenticate_NoHeader_ReturnsNoResult()
    {
        var ctx = BuildContext(new Dictionary<string, string?>());
        var handler = await CreateHandlerAsync(ctx);

        var result = await handler.AuthenticateAsync();

        result.None.Should().BeTrue();
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAuthenticate_UserOnly_ReturnsSuccessWithName()
    {
        var ctx = BuildContext(new Dictionary<string, string?>
        {
            ["X-Test-User"] = "alice",
        });
        var handler = await CreateHandlerAsync(ctx);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.Should().BeTrue();
        result.Principal.Should().NotBeNull();
        result.Principal!.Identity!.Name.Should().Be("alice");
        result.Principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("alice");
        result.Principal.FindAll(ClaimTypes.Role).Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAuthenticate_UserAndRoles_ReturnsSuccessWithRoles()
    {
        var ctx = BuildContext(new Dictionary<string, string?>
        {
            ["X-Test-User"] = "alice",
            ["X-Test-Roles"] = "Admin,Editor",
        });
        var handler = await CreateHandlerAsync(ctx);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.Should().BeTrue();
        var roles = result.Principal!.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        roles.Should().BeEquivalentTo(new[] { "Admin", "Editor" });
    }

    [Fact]
    public async Task HandleAuthenticate_RolesWithWhitespaceAndEmpty_TrimsAndSkips()
    {
        var ctx = BuildContext(new Dictionary<string, string?>
        {
            ["X-Test-User"] = "alice",
            ["X-Test-Roles"] = " Admin , ,Editor ",
        });
        var handler = await CreateHandlerAsync(ctx);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.Should().BeTrue();
        var roles = result.Principal!.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        roles.Should().BeEquivalentTo(new[] { "Admin", "Editor" });
    }

    [Fact]
    public async Task HandleAuthenticate_EmptyRolesHeader_ReturnsSuccessWithoutRoles()
    {
        var ctx = BuildContext(new Dictionary<string, string?>
        {
            ["X-Test-User"] = "alice",
            ["X-Test-Roles"] = string.Empty,
        });
        var handler = await CreateHandlerAsync(ctx);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.Should().BeTrue();
        result.Principal!.FindAll(ClaimTypes.Role).Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAuthenticate_AuthenticationType_IsTest()
    {
        var ctx = BuildContext(new Dictionary<string, string?>
        {
            ["X-Test-User"] = "alice",
        });
        var handler = await CreateHandlerAsync(ctx);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.Should().BeTrue();
        var identity = (ClaimsIdentity)result.Principal!.Identity!;
        identity.AuthenticationType.Should().Be("Test");
    }

    /// <summary>
    /// 最簡 <see cref="IOptionsMonitor{TOptions}"/> 實作 — handler unit test 專用，
    /// CurrentValue 回固定值，OnChange 回 noop disposable。
    /// </summary>
    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        private readonly T _value;

        public TestOptionsMonitor(T value)
        {
            _value = value;
        }

        public T CurrentValue => _value;

        public T Get(string? name) => _value;

        public IDisposable? OnChange(Action<T, string?> listener) => new NoopDisposable();

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
