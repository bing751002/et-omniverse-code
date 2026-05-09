---
id: F-006
title: Test-mode authentication scheme for E2E / integration test
module:
status: implementing
owner: jimmyliao
created: 2026-05-09
updated: 2026-05-09
supersedes:
superseded-by:
related-adr: [D-19]
related-interview: []
phase:
---

# F-006：Test-mode authentication scheme

## 業務背景

F-003 已 ship 但真實 auth（JWT / cookie session / RBAC）推到 v1.1+ 業務 milestone（per D-14、F-003 Out of scope）。在 v1.1 開始之前，業務 phase 一旦開工就會掛 `[Authorize]` 在每個業務 endpoint 上，E2E / integration test 必須能進入 authenticated 狀態才能跑。

per **D-19** 決議走 env-guarded TestAuthenticationHandler，本 spec 落地細節。

對應 7-step：與業務 step 無關，是所有 authorize-required endpoint 的測試前提。

## 用戶故事

1. As a backend developer writing integration test for an `[Authorize]`-protected endpoint, I want to send `X-Test-User: alice` header to enter authenticated state, so that test setup stays at 3 lines and not 30.
2. As a frontend / E2E developer using Playwright, I want a deterministic way to act-as a specific user without going through real login UI, so that E2E suite is decoupled from auth phase regression.
3. As a security reviewer, I want production deployments to refuse start if test auth scheme is registered, so that misconfiguration cannot leak this bypass to internet.

## 範圍

### In scope
- `TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationSchemeOptions>` 實作
- `TestAuthenticationSchemeOptions` 載 header name（預設 `X-Test-User`）+ 可選 role header（預設 `X-Test-Roles`）
- DI 註冊 helper：`AddTestAuthentication()` extension on `AuthenticationBuilder`
- 註冊時機 guard：**僅在 `app.Environment.IsEnvironment("IntegrationTest")` 註冊**
- Production / Staging 環境啟動時若偵測到 Test scheme 註冊 → throw `InvalidOperationException`（startup-time hard-fail）
- ClaimsPrincipal build 規則：
  - `ClaimTypes.NameIdentifier` = header value
  - `ClaimTypes.Name` = header value
  - `ClaimTypes.Role` = `X-Test-Roles` header（comma-separated）
- 共存：與真實 auth scheme（v1.1+ F-XXX 落地）並存，AuthenticationOptions 預設 scheme 走真實，Test 為次選
- TestSupport：`AuthenticatedTestClient` factory method on `LoggingTestWebAppFactory`，封裝 Test header 注入
- Integration test 範例：寫至少 3 個（authenticated 200、無 header 401、wrong role 403）

### Out of scope
- 真實 auth 實作（JWT / cookie / OAuth 整合）— v1.1+ F-XXX
- User store / RBAC 表 — v1.1+，per D-14 / D-18
- Frontend login flow 整合 — v1.1+
- 多 tenant / OrgUnit scope 的 claim 結構 — D-18 已決定走「事業群 scope」，此 spec 預留 ClaimType `et_omni:scope` 可注入但不規範語意
- E2E 對 Test handler 的使用範例（Playwright）— 留給未來 E2E phase

## 驗收條件

- [ ] **AC-1** `TestAuthenticationHandler` 落 `<src/backend/ETOmniverse.Api/Authentication/Test/>` namespace — 對應測試：unit
- [ ] **AC-2** `AddTestAuthentication()` extension 落 `<src/backend/ETOmniverse.Api/Authentication/Test/AuthenticationBuilderExtensions.cs>` — 對應測試：unit
- [ ] **AC-3** Integration test 送 `X-Test-User: alice` 進入 `[Authorize]` endpoint 回 200，且 `User.Identity.Name == "alice"` — 對應測試：integration
- [ ] **AC-4** Integration test 不送 header 進入 `[Authorize]` endpoint 回 401 ProblemDetails (per F-003 contract) — 對應測試：integration
- [ ] **AC-5** Integration test 送 `X-Test-User: alice` 但 endpoint 標 `[Authorize(Roles = "Admin")]`，未送 `X-Test-Roles` → 回 403 ProblemDetails — 對應測試：integration
- [ ] **AC-6** Integration test 送 `X-Test-User: alice` + `X-Test-Roles: Admin,Editor` → 進入 `[Authorize(Roles = "Admin")]` endpoint 回 200 — 對應測試：integration
- [ ] **AC-7** Production 環境（`ASPNETCORE_ENVIRONMENT=Production`）啟動 `app.Run()` 前若 Test scheme 已註冊 → throw `InvalidOperationException` 含 message "Test authentication scheme MUST NOT be registered outside IntegrationTest environment." — 對應測試：integration（用 WebApplicationFactory 強制 env=Production 測 throws）
- [ ] **AC-8** `LoggingTestWebAppFactory.CreateAuthenticatedClient(string user, string[] roles)` helper 存在，回 `HttpClient` 預設帶 Test header — 對應測試：unit / integration
- [ ] **AC-9** ProblemDetails 401 / 403 response 仍帶 `X-Correlation-Id`（per F-003 AC-4 — 跟 logging foundation 對齊不破壞） — 對應測試：integration
- [ ] **AC-10** dotnet build 0 warning / 0 error；docs/specs/F-006 status flip 完整 D-08 4-step（draft → approved → implementing → implemented）

## 實作連結（完工後填）

- Handler：[`src/backend/ETOmniverse.Api/Authentication/Test/TestAuthenticationHandler.cs`](../../src/backend/ETOmniverse.Api/Authentication/Test/TestAuthenticationHandler.cs)
- Options：[`src/backend/ETOmniverse.Api/Authentication/Test/TestAuthenticationSchemeOptions.cs`](../../src/backend/ETOmniverse.Api/Authentication/Test/TestAuthenticationSchemeOptions.cs)
- Defaults：[`src/backend/ETOmniverse.Api/Authentication/Test/TestAuthenticationDefaults.cs`](../../src/backend/ETOmniverse.Api/Authentication/Test/TestAuthenticationDefaults.cs)
- Extension：[`src/backend/ETOmniverse.Api/Authentication/Test/AuthenticationBuilderExtensions.cs`](../../src/backend/ETOmniverse.Api/Authentication/Test/AuthenticationBuilderExtensions.cs)
- Startup guard：[`src/backend/ETOmniverse.Api/Program.cs`](../../src/backend/ETOmniverse.Api/Program.cs)（搜尋 `MUST NOT be registered outside IntegrationTest environment`）
- Test fixture endpoints：[`src/backend/ETOmniverse.Api/Features/Test/Auth/TestAuthEndpoints.cs`](../../src/backend/ETOmniverse.Api/Features/Test/Auth/TestAuthEndpoints.cs)
- Test helper：[`tests/backend/ETOmniverse.TestSupport/Authentication/AuthenticatedTestClientExtensions.cs`](../../tests/backend/ETOmniverse.TestSupport/Authentication/AuthenticatedTestClientExtensions.cs)
- 主要 PR：（待 PR 編號落地後補）

## Open questions

- [ ] Q-006-001: Test handler header name是否要用 `Authorization: Test <user>` 而非自訂 `X-Test-User`？前者比較貼近真實 Bearer 流程，後者明顯不會誤用到 prod。**傾向 `X-Test-User`**（D-22 startup hard-fail 已防誤用，header 越明確越安全）
- [ ] Q-006-002: 真實 auth phase 上線時，Test handler 的 ClaimType 結構是否要 align？預留 ClaimType constants file 讓兩者共用是否值得？

## 變更記錄

| 日期 | 變更 | PR |
|---|---|---|
| 2026-05-09 | 初版 draft（基於 D-19） | TBD |
