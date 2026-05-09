namespace ETOmniverse.Api.Tests.Authentication;

using System.Threading.Tasks;
using ETOmniverse.Api.Authentication.Test;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// F-006 AC-2 unit-level 驗收 — DI 註冊路徑，scheme name + handler type
/// 經 IAuthenticationSchemeProvider 可正確被解析。
/// </summary>
public class AuthenticationBuilderExtensionsTests
{
    [Fact]
    public async Task AddTestAuthentication_RegistersTestSchemeWithHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication(TestAuthenticationDefaults.AuthenticationScheme)
                .AddTestAuthentication();

        await using var provider = services.BuildServiceProvider();
        var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();
        var scheme = await schemeProvider.GetSchemeAsync(TestAuthenticationDefaults.AuthenticationScheme);

        scheme.Should().NotBeNull();
        scheme!.Name.Should().Be("Test");
        scheme.HandlerType.Should().Be(typeof(TestAuthenticationHandler));
    }
}
