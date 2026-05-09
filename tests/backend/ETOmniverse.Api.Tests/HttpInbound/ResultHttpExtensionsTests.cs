namespace ETOmniverse.Api.Tests.HttpInbound;

using ETOmniverse.Api.Features.Common.ProblemDetails;
using ETOmniverse.Api.Middleware;
using ETOmniverse.Domain.Common.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// F-003 AC-1（API 半邊）— Result&lt;T&gt; / Result → IResult 統一 mapping 機械驗。
/// 純 unit（不 spin web app）— 用 DefaultHttpContext + minimal IConfiguration。
/// </summary>
public class ResultHttpExtensionsTests
{
    private static HttpContext NewCtx(string corrId = "test-corr-id")
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        var ctx = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        ctx.Items[CorrelationIdMiddleware.LogProperty] = corrId;
        ctx.Request.Path = "/test";
        return ctx;
    }

    [Fact]
    public void Generic_Success_with_value_returns_Ok_200()
    {
        var r = Result<int>.Success(42);
        var http = r.ToHttpResult(NewCtx());
        http.Should().BeOfType<Ok<int>>();
    }

    [Fact]
    public void Generic_Success_with_null_value_returns_NoContent_204()
    {
        var r = Result<string?>.Success(null);
        var http = r.ToHttpResult(NewCtx());
        http.Should().BeOfType<NoContent>();
    }

    [Fact]
    public void NonGeneric_Success_returns_NoContent_204()
    {
        var http = Result.Success().ToHttpResult(NewCtx());
        http.Should().BeOfType<NoContent>();
    }

    [Fact]
    public void Generic_Failure_returns_Problem_with_status_from_kind()
    {
        var r = Result<int>.Failure("PING.INVALID", "msg", ErrorKind.Validation);
        var http = r.ToHttpResult(NewCtx());
        http.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void NonGeneric_Failure_returns_Problem_with_status_from_kind()
    {
        var ctx = NewCtx("corr-xyz");
        var r = Result.Failure("X.NOT_FOUND", "missing", ErrorKind.NotFound);
        var http = r.ToHttpResult(ctx);
        http.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
