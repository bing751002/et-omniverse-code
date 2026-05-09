namespace ETOmniverse.TestSupport.Authentication;

using System;
using System.Net.Http;
using ETOmniverse.Api.Authentication.Test;
using ETOmniverse.TestSupport.Logging;

/// <summary>
/// F-006 AC-8 helper — 把 Test scheme header 注入封裝成一行呼叫，避免每個 integration test 自己組 headers。
/// 用 extension 形式（不是 LoggingTestWebAppFactory method）以維持 single responsibility — factory 只管 host build / sink，auth 細節獨立。
/// </summary>
public static class AuthenticatedTestClientExtensions
{
    /// <summary>
    /// 建一個預先注入 Test scheme headers 的 HttpClient。
    /// </summary>
    /// <param name="factory">既有 LoggingTestWebAppFactory（IntegrationTest env）。</param>
    /// <param name="user">X-Test-User 值（必填，不可 null/empty）。</param>
    /// <param name="roles">可選 role 集合；空則不送 X-Test-Roles header。</param>
    public static HttpClient CreateAuthenticatedClient(
        this LoggingTestWebAppFactory factory,
        string user,
        string[]? roles = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        if (string.IsNullOrWhiteSpace(user))
        {
            throw new ArgumentException("user must not be null or empty", nameof(user));
        }

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthenticationDefaults.UserHeaderName, user);
        if (roles is { Length: > 0 })
        {
            client.DefaultRequestHeaders.Add(
                TestAuthenticationDefaults.RoleHeaderName,
                string.Join(',', roles));
        }
        return client;
    }
}
