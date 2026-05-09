namespace ETOmniverse.Domain.Tests.Common.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using ETOmniverse.Domain.Common.Model;
using FluentAssertions;
using Xunit;

public class ErrorKindTests
{
    [Fact]
    public void Has_exactly_seven_values()
    {
        Enum.GetValues<ErrorKind>().Length.Should().Be(7);
    }

    [Fact]
    public void Names_are_stable()
    {
        var actual = Enum.GetNames<ErrorKind>().ToHashSet();
        var expected = new HashSet<string>
        {
            "Validation", "NotFound", "Conflict", "Unauthorized",
            "Forbidden", "ExternalDependency", "Unexpected"
        };

        actual.Should().BeEquivalentTo(expected);
    }
}
