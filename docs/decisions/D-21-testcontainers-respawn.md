---
id: D-21
title: Testcontainers MSSQL + Respawn for integration / E2E test data lifecycle
status: accepted
date: 2026-05-09
owner: jimmyliao
supersedes:
superseded-by:
related-spec: [F-005, F-007]
---

# D-21：Testcontainers MSSQL + Respawn for test data lifecycle

## Context

F-005 將落地 MSSQL DbContext + baseline migration（per ROADMAP），F-005 spec 已點明 Testcontainers MSSQL fixture（in scope）。但「fixture 怎麼用」「test 之間如何 reset data」這層紀律必須在 F-005 之前定，否則 E2E suite 一旦超過 50 個 test 就會卡在「速度 vs 隔離」的 trade-off。

三條典型路線：

| 策略 | 速度 | 隔離 | 適合 | 缺點 |
|------|------|------|------|------|
| Per-test transaction rollback | 最快（μs） | 完美（同 process） | unit / integration test | 跨 process 不適用（E2E 走 HTTP） |
| Respawn / Checkpoint truncate | 中（10-50 ms） | 完美 | E2E、跨 class | 需 setup 一次規則 |
| 完整 migrate 每次 | 最慢（5-30 sec） | 完美 | 不建議 | E2E 跑 30 分鐘變 5 小時 |

現在不選，未來 retro-fit 50 張表 schema + 100 個 test 的成本爆炸。

## Decision

**Integration test（同 process / WAF + Testcontainers MSSQL）**：
- 每個 test class 共用一個 `DatabaseFixture`（Testcontainers MSSQL container，class 結束 dispose）
- 每個 test method **走 transaction rollback**（test 內 begin tx → 操作 → assert → rollback）
- 由 `[Collection("Database")]` xUnit collection fixture 統籌

**E2E test（跨 process / Playwright / 真實 HTTP call）**：
- 整個 suite 共用一個 long-lived Testcontainers MSSQL container
- 每個 test class 之間用 **Respawn**（`Respawn` NuGet 套件）截斷所有 user table（保留 schema、保留 migration history table）
- 每個 test method 不再隔離（用 unique data 區隔，e.g. test method 內生 unique GUID prefix）
- 啟動時跑一次 migration，不再重跑

**共用 abstraction（F-007 落地）**：
- `tests/backend/ETOmniverse.TestSupport/Database/MsSqlContainerFixture.cs` — 提供 connection string
- `tests/backend/ETOmniverse.TestSupport/Database/RespawnDatabaseReset.cs` — 提供 `ResetAsync()`
- `tests/backend/ETOmniverse.TestSupport/Database/TransactionalTestBase.cs` — 提供 transaction-per-test 的 base class

## Consequences

### Positive
- Integration test 速度可達 ~50-100 test/秒（transaction rollback 比 truncate 快 10x+）
- E2E test 速度可達 ~10-30 test/分鐘（Respawn 比 migrate 快 50x+）
- 全 schema reset 紀律一致 → 新人寫 test 不用每次選 reset 策略
- E2E flaky 主因「test 之間殘留 data」從根源消失

### Negative
- 多兩個 NuGet 依賴：`Testcontainers.MsSql`（已 in F-005 spec）、`Respawn`（新加）
- 第一次跑 E2E 要拉 MSSQL Docker image（~1.5 GB），CI cache 要設定
- TransactionalTestBase 對「需要在 test 內呼叫 SaveChanges 後實際 commit」的 case 不適用（極少數場景）— 該類 test 走 Respawn 路徑

### Neutral
- Respawn 是 .NET 社群 de facto 標準（Jimmy Bogard 維護），不是冷門套件
- 跟 F-005 已選 Testcontainers 路線完全相容，只是補上 reset 機制這層

## Alternatives considered

- **A：完全 in-memory DB（SQLite / EF InMemory）跑 integration test，E2E 才用真 MSSQL**。沒選，因為 SQLite ≠ MSSQL（FK cascade、JSON column、temporal table 行為不同），EF InMemory 連 query translation 都不一樣 → 測過是錯覺、上 prod 才壞。
- **B：每個 test 自己拉一個 MSSQL container**。沒選，啟動 ~30 秒/container × 100 test = 50 分鐘，CI 爆炸。
- **C：共用一個 dev DB（不用 Testcontainers）**。沒選，CI 並行跑 → race condition；本地跑會污染開發 DB。

## References

- spec: F-005（落地 DbContext + Testcontainers fixture）、F-007（Respawn + TestSupport abstractions）
- Respawn: <https://github.com/jbogard/Respawn>
- Testcontainers .NET: <https://dotnet.testcontainers.org/>
- 相關 ADR: D-17（Phase 1 DB = MSSQL）
