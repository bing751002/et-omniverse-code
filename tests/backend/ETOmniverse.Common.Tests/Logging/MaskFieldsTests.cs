namespace ETOmniverse.Common.Tests.Logging;

using ETOmniverse.Common.Logging;
using FluentAssertions;
using Xunit;

public class MaskFieldsTests
{
    [Theory]
    [InlineData("password")]
    [InlineData("Password")]
    [InlineData("PASSWORD")]
    [InlineData("token")]
    [InlineData("apiKey")]
    [InlineData("secret")]
    [InlineData("authorization")]
    [InlineData("cookie")]
    [InlineData("x-api-key")]
    [InlineData("X-Api-Key")]
    public void Baseline_contains_field_case_insensitive(string field)
        => MaskFields.Baseline.Contains(field).Should().BeTrue();

    [Fact]
    public void Baseline_has_exactly_seven_fields()
        => MaskFields.Baseline.Should().HaveCount(7);

    [Fact]
    public void GetEffectiveSet_with_null_returns_baseline_only()
        => MaskFields.GetEffectiveSet(null).Should().HaveCount(7);

    [Fact]
    public void GetEffectiveSet_adds_new_fields()
        => MaskFields.GetEffectiveSet(new[] { "customSecret" }).Should().HaveCount(8);

    [Fact]
    public void GetEffectiveSet_does_not_double_count_case_variant_of_baseline()
        => MaskFields.GetEffectiveSet(new[] { "Password" }).Should().HaveCount(7);

    [Fact]
    public void GetEffectiveSet_ignores_empty_or_whitespace()
        => MaskFields.GetEffectiveSet(new[] { "", "  ", "real" }).Should().HaveCount(8);
}
