namespace ETOmniverse.Api.Tests.Authentication;

using System;
using ETOmniverse.Api.Authentication.Test;
using ETOmniverse.TestSupport.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// F-006 AC-7 integration test — Test authentication scheme 防誤用唯一防線：
/// non-IntegrationTest env 啟動時若偵測到 Test scheme 已註冊 → app.Build() 後
/// hard-fail throws InvalidOperationException（spec literal message）。
/// </summary>
[Collection("LoggingTests")]
public class ProductionStartupGuardTests
{
    [Fact]
    public void Production_With_Test_Scheme_Registered_Throws_On_Startup()
    {
        using var factory = new LoggingTestWebAppFactory { Environment = "Production" };

        var customized = factory.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {
                services
                    .AddAuthentication(TestAuthenticationDefaults.AuthenticationScheme)
                    .AddTestAuthentication();
            });
        });

        // CreateClient 會觸發 host build → hard-fail guard 在 app.Build() 之後 throw
        var act = () => customized.CreateClient();

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Test authentication scheme MUST NOT be registered outside IntegrationTest environment.");
    }

    [Fact]
    public void IntegrationTest_With_Test_Scheme_Does_Not_Throw()
    {
        using var factory = new LoggingTestWebAppFactory();   // Environment = "IntegrationTest"
        // 啟動不應 throw — Test scheme 由 Program.cs 在 IntegrationTest env 自動註冊
        var act = () => factory.CreateClient();
        act.Should().NotThrow();
    }
}
