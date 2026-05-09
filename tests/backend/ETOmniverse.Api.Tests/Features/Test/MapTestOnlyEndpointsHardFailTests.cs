namespace ETOmniverse.Api.Tests.Features.Test;

using System;
using ETOmniverse.Api.Features.Test;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Xunit;

/// <summary>
/// F-007 AC-C1 / AC-C3 / D-22: <see cref="MapTestOnlyEndpointsExtensions.MapTestOnlyEndpoints"/>
/// 在非 IntegrationTest env 下第一行必須 throw 含 D-22 字面 message 的 InvalidOperationException。
/// 此測直接 build 一個 minimal <see cref="WebApplication"/>，繞過 Program.cs 的 caller if，
/// 驗證 extension 第一行硬防線（雙線防護的第二線）。
/// </summary>
public sealed class MapTestOnlyEndpointsHardFailTests
{
    private const string ExpectedMessage =
        "Test-only endpoints MUST NOT be mapped outside IntegrationTest environment.";

    [Fact]
    public void MapTestOnlyEndpoints_InProductionEnvironment_ThrowsInvalidOperation()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Production",
        });
        var app = builder.Build();

        var act = () => app.MapTestOnlyEndpoints();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage(ExpectedMessage);
    }

    [Fact]
    public void MapTestOnlyEndpoints_InDevelopmentEnvironment_ThrowsInvalidOperation()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development",
        });
        var app = builder.Build();

        var act = () => app.MapTestOnlyEndpoints();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage(ExpectedMessage);
    }

    [Fact]
    public void MapTestOnlyEndpoints_InIntegrationTestEnvironment_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "IntegrationTest",
        });
        var app = builder.Build();

        var act = () => app.MapTestOnlyEndpoints();

        act.Should().NotThrow();
    }
}
