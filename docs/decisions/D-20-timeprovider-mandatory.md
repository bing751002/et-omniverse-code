---
id: D-20
title: TimeProvider mandatory for all time-dependent code; ban DateTime.Now / DateTime.UtcNow
status: proposed
date: 2026-05-09
owner: jimmyliao
supersedes:
superseded-by:
related-spec: [F-007]
---

# D-20：TimeProvider mandatory for all time-dependent code

## Context

.NET 8+ 內建 `TimeProvider` 抽象（`Microsoft.Extensions.TimeProvider.Testing` 套件提供 `FakeTimeProvider`）。et-omniverse 後端用 .NET 10，原生支援。

幾乎所有業務功能都會碰時間：
- 排播 schedule（Quartz.NET trigger time）
- Audit timestamp（CreatedAt / UpdatedAt / DeletedAt）
- JWT / session expiry（v1.1+）
- Retry / backoff 計算
- Cache TTL
- Notification 排程

如果這些用 `DateTime.UtcNow` 直寫，**所有時間敏感的測試都會 flaky**（race condition、跨日 boundary、timezone）。Retro-fit `TimeProvider` 到已成型 codebase 的成本是「一開始就強制」的 5x 以上。

F-002 已建立 CI 禁區掃描慣例（`scripts/check-no-console-write.py` + pre-commit hook），加 `check-no-datetime-now.py` 是 30 分鐘工作。

## Decision

**禁止 production code 內出現 `DateTime.Now` / `DateTime.UtcNow` / `DateTimeOffset.Now` / `DateTimeOffset.UtcNow`**，一律走注入的 `TimeProvider`：

- DI 註冊：`builder.Services.AddSingleton(TimeProvider.System)`（prod）
- Domain / Application / Infrastructure 任何需要「現在時間」的地方接 `TimeProvider` constructor 注入，呼叫 `_timeProvider.GetUtcNow()`
- 測試注入 `Microsoft.Extensions.TimeProvider.Testing.FakeTimeProvider`，可 `Advance(TimeSpan)` 控時間
- CI guard：新增 `scripts/check-no-datetime-now.py`，掃 `src/backend/**/*.cs`（排除 `Tests/`、`Migrations/` 自動生成 code），抓到任何 `DateTime.Now` / `DateTime.UtcNow` / `DateTimeOffset.Now` / `DateTimeOffset.UtcNow` 直接 fail
- pre-commit hook 串接（沿用 F-002 模式）

**例外（可豁免，需 rationale-bypass）**：
- Migration 內的 `defaultValueSql` 寫 `GETUTCDATE()`（DB 端時間，跟 .NET 無關）
- Logging context 內顯示用（log enricher 自己抓系統時間，性質是 metadata 不是業務邏輯）

## Consequences

### Positive
- 所有時間敏感 unit / integration / E2E test 可控時、可重現 → flaky 從根源消失
- Quartz schedule、JWT expiry、audit、retry backoff 全可單元化測試
- 新人加 feature 時 IDE / CI 直接擋下 `DateTime.UtcNow` → 不用 code review 才抓

### Negative
- 每個碰時間的 class 多一個 constructor 參數（typically 2-3 lines per class）
- Migration / log enricher 例外要明確標註（極少數場景）

### Neutral
- 完全是 .NET 8+ 框架推薦做法，不是自造抽象
- 跟 F-002 ICurrentUser、F-003 IExceptionHandler 同款 port-injection 風格

## Alternatives considered

- **A：自寫 `IClock` interface（Noda Time 風格）**。沒選，因為 .NET 8+ 已有 `TimeProvider` 是 first-class abstraction，自造會跟標準 library / 第三方套件對接時多一層 adapter。
- **B：不強制，靠 code review 抓**。沒選，因為 50 個 entity / 12 個模組的 codebase，code review 必然漏；且新人不知道規則。
- **C：只在 Domain 層強制，Infrastructure 可以用 `DateTime.UtcNow`**。沒選，因為 Infrastructure 也要測（Repository test 含 audit timestamp、Quartz job 排程）。

## References

- spec: F-007（Testability foundation — TimeProvider 落地細節）
- .NET 文件: <https://learn.microsoft.com/en-us/dotnet/standard/datetime/timeprovider-overview>
- 相關 phase 既有實作: F-002 `scripts/check-no-console-write.py` CI guard pattern
