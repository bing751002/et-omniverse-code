namespace ETOmniverse.Api.Tests.Middleware;

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ETOmniverse.TestSupport.Logging;
using FluentAssertions;
using Xunit;

[Collection("LoggingTests")]
public class MaskingTests
{
    [Fact]
    public async Task Body_capture_default_disabled_log_does_not_contain_RequestBody_property()
    {
        await using var f = new LoggingTestWebAppFactory();
        _ = await f.CreateClient().PostAsJsonAsync("/api/test/echo", new { password = "hunter2" });
        var summary = f.Sink.LogEvents.First(e => e.MessageTemplate.Text.StartsWith("HTTP "));
        summary.Properties.Should().NotContainKey("RequestBody");
    }

    [Fact]
    public async Task Body_capture_enabled_password_field_is_masked_in_RequestBody_property()
    {
        await using var f = new LoggingTestWebAppFactory()
            .WithSetting("Logging:RequestBody:Enabled", "true");
        _ = await f.CreateClient().PostAsJsonAsync("/api/test/echo", new { password = "hunter2" });
        var summary = f.Sink.LogEvents.First(e => e.MessageTemplate.Text.StartsWith("HTTP "));
        summary.Properties.Should().ContainKey("RequestBody");
        var body = summary.Properties["RequestBody"].ToString();
        body.Should().NotContain("hunter2");
        body.Should().Contain("***");
    }

    [Fact]
    public async Task AdditionalFields_customSecret_is_masked()
    {
        await using var f = new LoggingTestWebAppFactory()
            .WithSetting("Logging:RequestBody:Enabled", "true")
            .WithSetting("Logging:Mask:AdditionalFields:0", "customSecret");
        _ = await f.CreateClient().PostAsJsonAsync("/api/test/echo", new { customSecret = "leak-me" });
        var summary = f.Sink.LogEvents.First(e => e.MessageTemplate.Text.StartsWith("HTTP "));
        summary.Properties["RequestBody"].ToString().Should().NotContain("leak-me");
    }

    [Fact]
    public async Task QueryString_token_is_masked_in_Path_property_even_without_body_capture()
    {
        await using var f = new LoggingTestWebAppFactory();
        _ = await f.CreateClient().GetAsync("/api/test/echo?token=should-not-appear");
        var summary = f.Sink.LogEvents.First(e => e.MessageTemplate.Text.StartsWith("HTTP "));
        var path = summary.Properties["Path"].ToString();
        path.Should().NotContain("should-not-appear");
        path.Should().Contain("token=***");
    }

    [Fact]
    public async Task Body_over_32KB_records_BodyTruncated_property_true_with_BodySize()
    {
        await using var f = new LoggingTestWebAppFactory()
            .WithSetting("Logging:RequestBody:Enabled", "true");
        var huge = new string('a', 33 * 1024);   // > 32 KB
        var content = new StringContent($"{{\"big\":\"{huge}\"}}", System.Text.Encoding.UTF8, "application/json");
        _ = await f.CreateClient().PostAsync("/api/test/echo", content);

        var summary = f.Sink.LogEvents.First(e => e.MessageTemplate.Text.StartsWith("HTTP "));
        // B4: structured property 斷言（不是 RenderMessage 字串搜尋）
        summary.Properties.Should().ContainKey("BodyTruncated");
        summary.Properties["BodyTruncated"].ToString().Should().Be("True");
        summary.Properties.Should().ContainKey("BodySize");
        summary.Properties.Should().NotContainKey("RequestBody");
    }

    [Fact]
    public async Task Multipart_body_records_ContentType_and_BodySize_properties_no_RequestBody()
    {
        await using var f = new LoggingTestWebAppFactory()
            .WithSetting("Logging:RequestBody:Enabled", "true");
        var multipart = new MultipartFormDataContent();
        multipart.Add(new StringContent("secret-value"), "field1");
        _ = await f.CreateClient().PostAsync("/api/test/echo", multipart);

        var summary = f.Sink.LogEvents.First(e => e.MessageTemplate.Text.StartsWith("HTTP "));
        summary.Properties.Should().ContainKey("ContentType");
        summary.Properties["ContentType"].ToString().Should().Contain("multipart");
        summary.Properties.Should().ContainKey("BodySize");
        summary.Properties.Should().NotContainKey("RequestBody");
    }
}
