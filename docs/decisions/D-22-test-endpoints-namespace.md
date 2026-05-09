---
id: D-22
title: Test-only endpoints under /api/test/* namespace with startup hard-fail in Production
status: proposed
date: 2026-05-09
owner: jimmyliao
supersedes:
superseded-by:
related-spec: [F-007]
---

# D-22：Test-only endpoints namespace `/api/test/*` with Production startup hard-fail

## Context

E2E test 必然需要側邊 endpoint：
- Seed data（建立特定 user / batch / 商品狀態）
- Reset state（截斷 table、清 cache、reset Quartz schedule）
- Fault injection（強制讓某 outbound HTTP 失敗、模擬 timeout）
- Skip step（直接把 batch 推到 Step N，不跑前面 step）
- Read internal state（撈 in-memory cache 內容、撈 Quartz job queue）

F-002 / F-003 已經出現兩種 test endpoint 命名空間：
- `/test/throw`、`/test/echo`（F-002 RequestLoggingMiddleware 整合測用）
- `/api/common/ping/fail`（F-003 GlobalExceptionHandler 整合測用）

這是已經發生的小不一致。如果不收斂，等業務模組進來時會 sprawl 成 `/test/*`、`/api/common/*/fail`、`/api/<module>/test/*`、`/internal/*` 散處 50 個地方，且 prod 防護分散每個 endpoint 自己 IsEnvironment guard，必然有人忘記加。

## Decision

**所有 test-only endpoint 統一落 `/api/test/*` namespace，由集中註冊機制統籌**：

- Endpoint 全集中在 `src/backend/ETOmniverse.Api/Features/Test/`（namespace）
- 註冊由單一 extension method `WebApplication.MapTestOnlyEndpoints()` 統籌
- 此 extension 內部 **第一行就 throw 若不在 IntegrationTest env**：
  ```csharp
  public static WebApplication MapTestOnlyEndpoints(this WebApplication app)
  {
      if (!app.Environment.IsEnvironment("IntegrationTest"))
      {
          throw new InvalidOperationException(
              "Test-only endpoints MUST NOT be mapped outside IntegrationTest environment.");
      }
      // ... map /api/test/* endpoints
      return app;
  }
  ```
- Program.cs 內呼叫該 extension，IsEnvironment 判斷在 extension 內部不在 caller — 避免 caller 忘記加 guard
- 任何業務 phase 加 test endpoint **必須**在 spec 補一條 AC（spec 可追蹤）

**遷移既有 endpoint（F-007 落地時順手做）**：
- `/test/throw` → `/api/test/throw`
- `/test/echo` → `/api/test/echo`
- `/api/common/ping/fail` 保留（這是 F-003 ping sample 的一部分，不是 test infrastructure，性質是「ping 系列的 fault sample」）

每加一個 test endpoint：
1. 加進 `Features/Test/` 對應 file
2. 在 `MapTestOnlyEndpoints` extension 內註冊
3. spec 內補 AC 說明用途
4. CI guard（F-007 落地時加）：scripts/check-test-endpoints.py 掃 `Features/Test/` 與 `MapTestOnlyEndpoints` 一致

## Consequences

### Positive
- Production 無論如何不可能洩漏 test endpoint：startup hard-fail 比每個 endpoint 自己 guard 更安全（fail-fast、一個 fail 全 fail）
- 所有 test endpoint 集中可 review，新人 onboarding 一個 file 看完
- 每個 test endpoint 有 spec AC → 不會 sprawl 到沒人記得為什麼存在

### Negative
- 既有 `/test/throw`、`/test/echo` 要遷移（小成本，ping test 改路徑）
- 業務 phase 加 test endpoint 多一步「補 spec AC」紀律

### Neutral
- 跟 D-19 TestAuthenticationHandler 同款 startup-time guard 哲學（防 config 錯誤洩漏）
- F-003 留下的不一致（`/test/*` vs `/api/common/ping/fail`）此 ADR 一併處理 — `/api/common/ping/fail` 因為是 ping sample 的 fault demo，留原 namespace

## Alternatives considered

- **A：靠每個 endpoint 自己 IsEnvironment guard**。沒選，散亂、必然漏、prod 風險集中。
- **B：所有 test endpoint 加 `[Authorize(Roles = "Test")]`**。沒選，依賴 auth 機制（F-006），耦合過深；且 prod 還是會吃到 401 而不是 404，從外部仍可推斷有此 endpoint 存在。
- **C：把 test endpoint 拆獨立 csproj `ETOmniverse.Api.TestEndpoints`，prod 不 reference**。沒選，多一個 csproj + 編譯路徑，邊際收益不抵成本；startup hard-fail 已經達到等效安全。

## References

- spec: F-007（落地 MapTestOnlyEndpoints + 既有 /test/* 遷移）
- 相關 ADR: D-19（同款 startup hard-fail 防洩漏哲學）
- 相關 phase 既有實作:
  - `src/backend/ETOmniverse.Api/Program.cs` 既有 `/test/throw`、`/test/echo` block
  - `src/backend/ETOmniverse.Api/Features/Common/Ping/PingEndpoints.cs` `/api/common/ping/fail`
