---
id: F-007
title: Testability foundation — TimeProvider mandate, DB lifecycle, test endpoint discipline
module:
status: implemented
owner: jimmyliao
created: 2026-05-09
updated: 2026-05-09
supersedes:
superseded-by:
related-adr: [D-20, D-21, D-22]
related-interview: []
phase:
---

# F-007：Testability foundation

## 業務背景

v1.0 backend foundation（F-002 logging / F-003 HTTP inbound / F-004 HTTP outbound / F-005 persistence）將完成基礎建設。在進入業務 phase（v1.1+ 12 模組）之前，必須一次落地三項橫切性測試基礎，否則每個業務 phase 都要回頭補：

1. **TimeProvider 全棧強制**（per D-20）— 排播 schedule、audit timestamp、retry backoff、cache TTL 全部時間敏感，沒這個一切時間相關 test 必 flaky
2. **Test data lifecycle**（per D-21）— Testcontainers MSSQL 由 F-005 提供，但 Respawn + transactional test base 由本 spec 落地
3. **Test-only endpoints namespace**（per D-22）— 收斂既有 F-002 `/test/*` 與 F-003 `/api/common/ping/fail`，定義集中註冊機制 + Production startup hard-fail

對應 7-step：與業務 step 無關，是所有業務 phase 開工前的測試基建前提。

## 用戶故事

1. As a backend developer writing a feature that depends on time, I want `TimeProvider` injected, so that test can `Advance(TimeSpan.FromHours(2))` without sleeping or flaking.
2. As a backend developer writing integration test, I want `TransactionalTestBase` so that each test method auto-rollbacks, achieving 50-100 test/sec speed.
3. As an E2E test author, I want `RespawnDatabaseReset.ResetAsync()` between test classes, so that data leakage between tests is impossible.
4. As a backend developer adding a test endpoint, I want one centralised place (`MapTestOnlyEndpoints`) to register, so that I cannot accidentally leak it to production.
5. As a security reviewer, I want production startup to hard-fail if any test endpoint is mapped, so that misconfiguration is fail-fast and not silent.

## 範圍

### In scope

#### A. TimeProvider 強制（per D-20）
- DI 註冊 prod：`builder.Services.AddSingleton(TimeProvider.System)`
- TestSupport 提供 `FakeTimeProviderFixture` wrapper 整合 `Microsoft.Extensions.TimeProvider.Testing.FakeTimeProvider`
- CI guard：`scripts/check-no-datetime-now.py` 掃 backend C# 禁區（patterns: `DateTime\.Now`、`DateTime\.UtcNow`、`DateTimeOffset\.Now`、`DateTimeOffset\.UtcNow`）
- pre-commit hook 串接（沿用 F-002 `check-no-console-write` 模式）
- CONVENTIONS.md 補一條硬規則
- 例外白名單：`Migrations/`（auto-generated）、Logging enricher metadata
- F-002 / F-003 既有 production code 若有違規 → 此 spec 一併修正

#### B. DB lifecycle abstractions（per D-21）
- `tests/backend/ETOmniverse.TestSupport/Database/MsSqlContainerFixture.cs` — 共用 Testcontainers 容器（IClassFixture / xUnit collection fixture）
- `tests/backend/ETOmniverse.TestSupport/Database/DockerFactAttribute.cs` / `DockerTheoryAttribute.cs` — Docker 不可用時在 xUnit discovery 層回報真正 skipped，不在 test body 內 `return` 假綠
- `tests/backend/ETOmniverse.TestSupport/Database/RespawnDatabaseReset.cs` — `ResetAsync()` 用 Respawn 截斷 user table（保留 `__EFMigrationsHistory`）
- `tests/backend/ETOmniverse.TestSupport/Database/TransactionalTestBase.cs` — base class 提供 `BeginTransactionAsync` / `RollbackAsync`，xUnit `IAsyncLifetime` 整合
- `[Collection("Database")]` xUnit collection definition
- 範例 integration test 證明 transaction rollback 速度（≥ 50 test/sec on dev machine）
- F-005 一旦提供 DbContext 即可整合（依賴 F-005，但 F-007 不重做 F-005 工作）

#### C. Test-only endpoint discipline（per D-22）
- `src/backend/ETOmniverse.Api/Features/Test/` namespace 建立
- `WebApplication.MapTestOnlyEndpoints()` extension 落 `Features/Test/MapTestOnlyEndpointsExtensions.cs`
- 內部第一行 IsEnvironment("IntegrationTest") guard，否則 throw
- Program.cs 改 call extension（不在 caller 自己 guard）
- 既有 `/test/throw`、`/test/echo` 遷移到 `/api/test/throw`、`/api/test/echo`（F-002 test 同步更新 path）
- `/api/common/ping/fail` **不遷移**（per D-22 — ping sample 的 fault demo 不是 test infrastructure）
- CI guard：`scripts/check-test-endpoints.py` 掃 `Features/Test/` 註冊與 `MapTestOnlyEndpoints` 一致
- pre-commit hook 串接

### Out of scope
- F-005 本身的 DbContext / migration / repository 實作（由 F-005 spec 處理）
- E2E test framework 選型（Playwright vs Cypress）— 等真要寫 frontend E2E 再決定
- F-006 Test authentication scheme（獨立 spec，但 F-007 transactional test base 會用到 F-006 helper 寫範例）— **F-007 不阻擋 F-006，可平行**
- TimeProvider 在 frontend Vue 端的對應（frontend 用 `vi.useFakeTimers()`，跟 backend 無關）
- 真實 production deployment / monitoring 設定

## 驗收條件

### A. TimeProvider
- [x] **AC-A1** `builder.Services.AddSingleton(TimeProvider.System)` 註冊在 Program.cs — 對應測試：integration（DI resolve `TimeProvider` 拿到 instance）
- [x] **AC-A2** `scripts/check-no-datetime-now.py` 掃 backend C# 檔（排除 `Migrations/`、`bin/`、`obj/`）抓到任何 `DateTime.Now` / `DateTime.UtcNow` / `DateTimeOffset.Now` / `DateTimeOffset.UtcNow` 即 exit 非零 — 對應測試：unit（script 餵 fixture file 測 detection）
- [x] **AC-A3** pre-commit hook 串 `check-no-datetime-now.py`（per F-002 模式） — 對應測試：integration（hook script 餵違規 file 測 block）
- [x] **AC-A4** F-002 / F-003 既有 src code 若違反 AC-A2，本 spec 一併修正並通過 — 對應測試：CI（dotnet build + test 全綠）
- [x] **AC-A5** TestSupport 提供 `FakeTimeProviderFixture`（包 `Microsoft.Extensions.TimeProvider.Testing.FakeTimeProvider`），整合 xUnit collection fixture — 對應測試：unit
- [x] **AC-A6** CONVENTIONS.md 增一節「Time handling — TimeProvider mandate」描述規則與例外 — 對應測試：grep CONVENTIONS.md 含必要 keyword

### B. DB lifecycle
- [x] **AC-B1** `MsSqlContainerFixture` 落 `tests/backend/ETOmniverse.TestSupport/Database/`，xUnit collection fixture，提供 connection string property — 對應測試：integration（兩個 test class 共用同 fixture，container 只啟動一次）
- [x] **AC-B2** `RespawnDatabaseReset.ResetAsync()` 截斷所有 user table，保留 `__EFMigrationsHistory` — 對應測試：integration（先 insert data → ResetAsync → query empty + migration table 仍在）
- [x] **AC-B3** `TransactionalTestBase` 提供 transaction-per-test，test 結束自動 rollback — 對應測試：integration（test method insert data → next test method 看不到）
- [x] **AC-B4** `[Collection("Database")]` xUnit collection definition 落 `tests/backend/ETOmniverse.TestSupport/Database/DatabaseCollection.cs` — 對應測試：grep
- [x] **AC-B5** Respawn + Testcontainers package reference 加進 `tests/backend/ETOmniverse.TestSupport.csproj`（依賴版本由執行 phase planner 視 F-005 既選版本決定） — 對應測試：grep
- [x] **AC-B6** Smoke test 範例：跑 50 個 transactional test 在開發機 < 5 sec — 對應測試：integration（含 timing assertion 寬鬆 timeout）

### C. Test endpoints
- [x] **AC-C1** `Features/Test/MapTestOnlyEndpointsExtensions.cs` 內部第一行 `if (!app.Environment.IsEnvironment("IntegrationTest")) throw new InvalidOperationException(...)` — 對應測試：unit
- [x] **AC-C2** Program.cs 改 call `app.MapTestOnlyEndpoints()`（不再 inline `if IsEnvironment` block） — 對應測試：grep + integration
- [x] **AC-C3** Production env startup 若 `MapTestOnlyEndpoints` 被 call → throw — 對應測試：integration（強制 env=Production 跑 WAF 測 throws）
- [x] **AC-C4** 既有 `/test/throw` `/test/echo` 遷移到 `/api/test/throw` `/api/test/echo`，F-002 既有 test 同步更新 path → 仍全綠 — 對應測試：integration
- [x] **AC-C5** `/api/common/ping/fail` 不變（per D-22 — F-003 ping sample 留原 namespace） — 對應測試：grep + integration
- [x] **AC-C6** `scripts/check-test-endpoints.py` 掃 `Features/Test/` 註冊一致性 — 對應測試：unit
- [x] **AC-C7** pre-commit hook 串 `check-test-endpoints.py` — 對應測試：integration

### D. 收尾
- [x] **AC-D1** dotnet build solution: 0 warning / 0 error（`-warnaserror` clean）
- [x] **AC-D2** dotnet test solution: 全綠（含 F-007 新增 test + F-002~F-006 既有 test 無回歸）
- [x] **AC-D3** F-007 spec status flip 完整 D-08 4-step（draft → approved → implementing → implemented）

## 實作連結（完工後填）

- TimeProvider DI：`src/backend/ETOmniverse.Api/Program.cs`
- TimeProvider CI guard：`scripts/check-no-datetime-now.py`
- FakeTimeProviderFixture：`tests/backend/ETOmniverse.TestSupport/Time/FakeTimeProviderFixture.cs`
- MsSqlContainerFixture：`tests/backend/ETOmniverse.TestSupport/Database/MsSqlContainerFixture.cs`
- RespawnDatabaseReset：`tests/backend/ETOmniverse.TestSupport/Database/RespawnDatabaseReset.cs`
- TransactionalTestBase：`tests/backend/ETOmniverse.TestSupport/Database/TransactionalTestBase.cs`
- DatabaseCollection：`tests/backend/ETOmniverse.TestSupport/Database/DatabaseCollection.cs`
- MapTestOnlyEndpointsExtensions：`src/backend/ETOmniverse.Api/Features/Test/MapTestOnlyEndpointsExtensions.cs`
- Test endpoint CI guard：`scripts/check-test-endpoints.py`
- 主要 PR：#XXX

## Open questions

- [ ] Q-007-001: F-007 跟 F-005 的時序 — F-007 B 區塊（DB lifecycle）依賴 F-005 DbContext，是否 B 區塊獨立成 F-007.1 在 F-005 之後？**傾向同 spec 但 phase plan 安排 F-005 → F-007 順序**
- [ ] Q-007-002: TimeProvider AC-A4「F-002 / F-003 既有 code 若違反一併修正」— 修正範圍若大需要新 ADR 嗎？**傾向不需要，這是 D-20 落地的副作用**
- [ ] Q-007-003: scripts/check-no-datetime-now.py 是否要 allow inline rationale-bypass comment（per D-18）？**傾向是，沿用 F-002 既有模式**

## 變更記錄

| 日期 | 變更 | PR |
|---|---|---|
| 2026-05-09 | 初版 draft（基於 D-20 / D-21 / D-22） | TBD |
