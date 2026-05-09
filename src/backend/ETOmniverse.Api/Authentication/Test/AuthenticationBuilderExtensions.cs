namespace ETOmniverse.Api.Authentication.Test;

using System;
using Microsoft.AspNetCore.Authentication;

/// <summary>
/// F-006 AC-2 — DI 註冊擴充。讓 caller 用
/// <c>builder.Services.AddAuthentication("Test").AddTestAuthentication()</c>
/// 一行註冊 Test scheme。本 extension 不做 env 檢查（per CONTEXT locked
/// decision — guard 統一收在 Program.cs）。
/// </summary>
public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddTestAuthentication(this AuthenticationBuilder builder)
        => builder.AddTestAuthentication(configureOptions: null);

    public static AuthenticationBuilder AddTestAuthentication(
        this AuthenticationBuilder builder,
        Action<TestAuthenticationSchemeOptions>? configureOptions)
    {
        return builder.AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>(
            TestAuthenticationDefaults.AuthenticationScheme,
            configureOptions ?? (_ => { }));
    }
}
