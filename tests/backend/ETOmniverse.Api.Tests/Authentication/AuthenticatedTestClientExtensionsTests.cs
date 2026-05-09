namespace ETOmniverse.Api.Tests.Authentication;

using System;
using System.Net;
using System.Threading.Tasks;
using ETOmniverse.Api.Authentication.Test;
using ETOmniverse.TestSupport.Authentication;
using ETOmniverse.TestSupport.Logging;
using FluentAssertions;
using Xunit;

[Collection("LoggingTests")]
public class AuthenticatedTestClientExtensionsTests
{
    [Fact]
    public void CreateAuthenticatedClient_WithUserOnly_SetsUserHeaderOnly()
    {
        using var factory = new LoggingTestWebAppFactory();
        using var client = factory.CreateAuthenticatedClient("alice");

        client.DefaultRequestHeaders.GetValues(TestAuthenticationDefaults.UserHeaderName)
              .Should().ContainSingle().Which.Should().Be("alice");
        client.DefaultRequestHeaders.Contains(TestAuthenticationDefaults.RoleHeaderName)
              .Should().BeFalse();
    }

    [Fact]
    public void CreateAuthenticatedClient_WithRoles_SetsBothHeaders()
    {
        using var factory = new LoggingTestWebAppFactory();
        using var client = factory.CreateAuthenticatedClient("alice", new[] { "Admin", "Editor" });

        client.DefaultRequestHeaders.GetValues(TestAuthenticationDefaults.UserHeaderName)
              .Should().ContainSingle().Which.Should().Be("alice");
        client.DefaultRequestHeaders.GetValues(TestAuthenticationDefaults.RoleHeaderName)
              .Should().ContainSingle().Which.Should().Be("Admin,Editor");
    }

    [Fact]
    public async Task CreateAuthenticatedClient_EndToEnd_HitsAdminEndpoint()
    {
        using var factory = new LoggingTestWebAppFactory();
        using var client = factory.CreateAuthenticatedClient("alice", new[] { "Admin" });

        var resp = await client.GetAsync("/api/test/auth/admin");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateAuthenticatedClient_NullOrEmptyUser_Throws(string? user)
    {
        using var factory = new LoggingTestWebAppFactory();
        var act = () => factory.CreateAuthenticatedClient(user!);
        act.Should().Throw<ArgumentException>().WithMessage("*user*");
    }
}
