namespace ETOmniverse.Domain.Tests.Common.Model;

using ETOmniverse.Domain.Common.Model;
using FluentAssertions;
using Xunit;

public class ResultTests
{
    [Fact]
    public void Success_generic_carries_value()
    {
        var r = Result<int>.Success(42);

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(42);
        r.Kind.Should().BeNull();
        r.Code.Should().BeNull();
        r.Message.Should().BeNull();
    }

    [Fact]
    public void Failure_generic_carries_error_metadata()
    {
        var r = Result<int>.Failure("PING.MSG_TOO_LONG", "message too long", ErrorKind.Validation);

        r.IsSuccess.Should().BeFalse();
        r.Value.Should().Be(default(int));
        r.Kind.Should().Be(ErrorKind.Validation);
        r.Code.Should().Be("PING.MSG_TOO_LONG");
        r.Message.Should().Be("message too long");
    }

    [Fact]
    public void NonGeneric_Success_uses_Unit()
    {
        var r = Result.Success();

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(Unit.Value);
        r.Kind.Should().BeNull();
    }

    [Fact]
    public void NonGeneric_Failure_carries_kind()
    {
        var r = Result.Failure("X.NOT_FOUND", "missing", ErrorKind.NotFound);

        r.IsSuccess.Should().BeFalse();
        r.Kind.Should().Be(ErrorKind.NotFound);
        r.Code.Should().Be("X.NOT_FOUND");
        r.Message.Should().Be("missing");
    }
}
