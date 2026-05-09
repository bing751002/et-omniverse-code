---
id: D-19
title: Test-mode authentication bypass via env-guarded TestAuthenticationHandler
status: proposed
date: 2026-05-09
owner: jimmyliao
supersedes:
superseded-by:
related-spec: [F-006]
---

# D-19：Test-mode authentication bypass via env-guarded TestAuthenticationHandler

## Context

F-003 把真實 auth（JWT / session / RBAC）推到 v1.1+ 業務 milestone（per D-14 / F-003 Out of scope）。但 v1.0 backend foundation 結束後即將進入業務 endpoint 開發（v1.1），每個業務 endpoint 都會掛 `[Authorize]`。

若沒有可預期的「測試模式 auth」機制，未來 E2E / integration test 兩條路：
1. 全部走真 login flow → 慢、flaky、跟 auth phase 重度耦合 → E2E suite 變成 auth regression test
2. 每個測試各自自己 build `ClaimsPrincipal` → 共識散亂、新人寫測試先卡 1 天

F-003 已經建立 IsEnvironment("IntegrationTest") guard 慣例（`/api/common/ping/fail` 採用），auth bypass 沿用同模式可零學習成本。

## Decision

落地 **TestAuthenticationHandler** 機制，**僅在 `app.Environment.IsEnvironment("IntegrationTest")` 註冊**，由 spec F-006 規範細節：

- 新增 `AuthenticationScheme = "Test"`，對應 `TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationSchemeOptions>`
- Handler 讀 request header `X-Test-User`（或 spec 定的 header 名），值為 user id / username
- 視 header 內容 build `ClaimsPrincipal`（ClaimTypes.NameIdentifier、ClaimTypes.Name、Roles）
- Production / Staging 啟動時若偵測到此 scheme 註冊 → **startup-time hard-fail throw**（防 config 錯誤洩漏）
- 真實 auth scheme（v1.1+ F-XXX 才落地）可與 Test scheme 並存，AuthenticationOptions.DefaultScheme 走真實，Test 為「次選 scheme」（test 顯式指定）

## Consequences

### Positive
- E2E test 寫入：`request.Headers.Add("X-Test-User", "alice"); request.Headers.Add("Authorization", "Test")`，3 行內進入 authenticated 狀態
- 跟未來真實 auth phase **零耦合**：F-006 落地後，Auth phase 可獨立進行，不會牽動既有 E2E test
- 沿用 F-003 IsEnvironment guard 慣例，紀律一致

### Negative
- 多一個 AuthenticationHandler 實作 + 對應 startup guard，~80 LOC
- 真實 auth phase 上線時要 review TestAuthenticationHandler 還適不適用（可能某些 claim 結構需要對齊）

### Neutral
- 與 F-002 ICurrentUser stub 並存：F-002 stub 是「沒 auth 時的 anonymous fallback」，TestAuthenticationHandler 是「test mode 假裝有 auth」，職責互不重疊
- 跟 v1.1+ 業務 milestone 的 RBAC（D-18 功能面 RBAC + 事業群 scope）相容：Test handler 可以 inject 任意 role / scope claim

## Alternatives considered

- **A：每個 test 自己 build TestServer with custom AuthenticationHandler**。沒選，因為散亂、無共識，每個 dev 寫法不同。
- **B：用 ASP.NET Core 內建的 `AddJwtBearer` 加上 test-only HMAC secret 簽 fake JWT**。沒選，因為要 wire JWT 套件 + 加 secret 管理，遠重於 D-19；且未來真實 auth 若不走 JWT（cookie session）則 test JWT 是廢路徑。
- **C：完全不做 test bypass，E2E 一律走真 login**。沒選，因為 E2E suite 會變成 auth phase regression test，且每個 test setup 要 5+ 步驟，速度慢且 flaky。

## References

- spec: F-006（落地 Auth bypass 機制細節）
- 相關 ADR: D-14（auth 推到 Phase 2）、D-18（功能面 RBAC + 事業群 scope）
- 相關 phase 既有實作: F-003 `IsEnvironment("IntegrationTest")` guard pattern（`src/backend/ETOmniverse.Api/Features/Common/Ping/PingEndpoints.cs`）
