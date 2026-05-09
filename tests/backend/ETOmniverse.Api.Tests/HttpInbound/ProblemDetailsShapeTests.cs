namespace ETOmniverse.Api.Tests.HttpInbound;

using System.Collections.Generic;
using ETOmniverse.Api.Features.Common.ProblemDetails;
using ETOmniverse.Api.Middleware;
using ETOmniverse.Domain.Common.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// F-003 AC-1 / AC-4 — ErrorKind→HTTP status 7 值 mapping、ProblemDetails shape
/// （traceId / code / type / Instance）機械驗。
/// </summary>
public class ProblemDetailsShapeTests
{
    [Theory]
    [InlineData(ErrorKind.Validation, 400)]
    [InlineData(ErrorKind.Unauthorized, 401)]
    [InlineData(ErrorKind.Forbidden, 403)]
    [InlineData(ErrorKind.NotFound, 404)]
    [InlineData(ErrorKind.Conflict, 409)]
    [InlineData(ErrorKind.ExternalDependency, 502)]
    [InlineData(ErrorKind.Unexpected, 500)]
    public void ErrorKind_maps_to_expected_status(ErrorKind kind, int expected)
        => kind.ToHttpStatus().Should().Be(expected);

    [Fact]
    public void ToProblemDetails_sets_traceId_from_CorrelationIdMiddleware_LogProperty()
    {
        var ctx = NewCtx(corrId: "abc-123");
        var pd = ctx.ToProblemDetails(ErrorKind.NotFound, "X.NF", "missing");

        pd.Status.Should().Be(404);
        pd.Title.Should().Be("Not found");
        pd.Detail.Should().Be("missing");
        pd.Instance.Should().Be("/x");
        pd.Extensions.Should().ContainKey("traceId");
        pd.Extensions["traceId"].Should().Be("abc-123");
        pd.Extensions["code"].Should().Be("X.NF");
    }

    [Fact]
    public void ToProblemDetails_type_uses_Errors_TypeBaseUrl_from_config()
    {
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Errors:TypeBaseUrl"] = "https://custom.example/err"
            })
            .Build();

        var ctx = NewCtx(cfg);
        var pd = ctx.ToProblemDetails(ErrorKind.Validation, "X.VAL", "bad");
        pd.Type.Should().Be("https://custom.example/err/X.VAL");
    }

    [Fact]
    public void ToProblemDetails_type_falls_back_to_default_when_config_missing()
    {
        var ctx = NewCtx();
        var pd = ctx.ToProblemDetails(ErrorKind.Conflict, "X.CONF", "dup");
        pd.Type.Should().Be("https://et-omniverse/errors/X.CONF");
    }

    [Fact]
    public void ToProblemDetails_includes_errors_extension_when_provided()
    {
        var ctx = NewCtx();
        var errors = new Dictionary<string, string[]>
        {
            ["message"] = new[] { "required" }
        };
        var pd = ctx.ToProblemDetails(ErrorKind.Validation, "X.VAL", "bad", errors);
        pd.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public void ToProblemDetails_omits_errors_extension_when_null()
    {
        var ctx = NewCtx();
        var pd = ctx.ToProblemDetails(ErrorKind.NotFound, "X.NF", "missing");
        pd.Extensions.Should().NotContainKey("errors");
    }

    private static HttpContext NewCtx(IConfiguration? cfg = null, string corrId = "default-corr")
    {
        var services = new ServiceCollection();
        services.AddSingleton(cfg ?? new ConfigurationBuilder().Build());
        var ctx = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        ctx.Items[CorrelationIdMiddleware.LogProperty] = corrId;
        ctx.Request.Path = "/x";
        return ctx;
    }
}
