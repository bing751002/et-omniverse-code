---
id: F-004
title: HTTP outbound base
module:
status: draft
owner: jimmyliao
created: 2026-05-09
updated: 2026-05-09
supersedes:
superseded-by:
related-adr: []
related-interview: []
phase:
---

# F-004：HTTP outbound base

## 業務背景

ET-Omniverse Phase 1 會呼叫多個外部服務：kie.ai、Gemini、大數據受眾、派報自動化、SMTP。這些 outbound 呼叫需要一致的 typed client、timeout / retry、CorrelationId 傳遞、latency / status logging、secret / endpoint 設定來源。F-004 在任何業務外部 client 之前先釘住共用 outbound HTTP pattern。

對應 7-step：支援 Step 2 受眾、Step 3 AI VCR、Step 5/6 派報與通知，但本 spec 不實作任何業務外部服務。

## 用戶故事

1. As a backend developer wrapping an external service, I want a typed client convention and base handlers, so that every service propagates CorrelationId and logs latency/status consistently.
2. As an operator investigating external failures, I want each outbound call to log target service, method, status, duration, correlation id, and failure category, so that I can distinguish partner failure from ET application failure.
3. As a security reviewer, I want endpoints and API keys loaded only from config / user-secrets / Ops files, so that no external secret is committed to the repo.

## 範圍

### In scope

- **IHttpClientFactory + typed client 規約**
  - 每個外部 service 一個 typed client
  - 每個 typed client 對應一個 Domain port，Domain 不 reference HTTP / SDK
  - Infrastructure 實作 port，Api composition root 註冊
  - typed client 命名：`<ServiceName>Client`；options 命名：`<ServiceName>Options`

- **Resilience policy**
  - timeout / retry 預設由 config 控制
  - retry 使用 exponential backoff + jitter
  - 預設只 retry transient failure：timeout、5xx、408、429
  - 4xx 非 408/429 不 retry
  - Day 1 不啟用 circuit breaker；只保留 options shape 給 P1.x / 真實外部服務 phase 視情況開啟
  - config defaults：
    - `TimeoutSeconds`: 10
    - `Retry:MaxAttempts`: 3
    - `Retry:BaseDelayMs`: 200
    - `CircuitBreaker:Enabled`: false

- **DelegatingHandler base**
  - CorrelationId propagation：讀 F-002 current correlation id，寫到 outbound `X-Correlation-Id`
  - latency/status logging：每次 outbound call 一行 structured log
  - log 欄位：serviceName / method / host / pathTemplate / statusCode / durationMs / correlationId / retryAttempt / outcome
  - secret-safe：不記 full URL query、authorization header、request/response body
  - 4xx/5xx 轉成 `Result<T>` / typed failure，不讓業務 use case 直接處理 raw HttpRequestException
  - handler 順序固定：
    1. CorrelationId propagation
    2. outbound logging
    3. resilience policy
    4. typed client send

- **Config source discipline**
  - endpoint / timeout / retry / API key 全部從 `appsettings.*.Ops.json`、env、user-secrets 取得
  - repo 內只允許 non-secret placeholder / schema
  - 新增 config key 必須同步 INFRA 或 spec 寫用途、預設值、是否 secret
  - options validation fail 時 API startup fail fast，不讓缺 endpoint / secret 的 client 到 runtime 才炸
  - config shape：
    ```json
    {
      "ExternalServices": {
        "SampleEcho": {
          "BaseUrl": "http://localhost",
          "TimeoutSeconds": 10,
          "Retry": {
            "MaxAttempts": 3,
            "BaseDelayMs": 200
          },
          "CircuitBreaker": {
            "Enabled": false
          }
        }
      }
    }
    ```

- **Sample typed client**
  - 建 sample outbound client 只驗證 handler pipeline，不碰 kie.ai / Gemini / 大數據 / 派報
  - 優先用 test-only fake HTTP server 或 SampleEchoClient
  - 不把 SMTP 當第一個 sample，避免 SMTP healthcheck 語意不清
  - sample 不進 production route；只透過 integration test 直接呼叫 typed client / port
  - fake server 回應固定案例：200 success、500 transient、400 non-retry、timeout

### Out of scope

- kie.ai / Gemini / 大數據 / 派報 / SMTP 真實 client
- Service mesh / sidecar
- 外部 API mock server 常駐環境；測試用 in-memory / WireMock.Net 類型 fixture 即可
- Distributed tracing / W3C TraceContext
- API key rotation 流程
- Business retry 策略特殊化（例如 AI VCR 長任務輪詢）
- Background job / Quartz 呼叫外部服務的 orchestration pattern（只提供 HTTP client foundation）

## 驗收條件

- [ ] **AC-1 typed client convention**：sample client 透過 `IHttpClientFactory` 註冊，且有對應 Domain port；Domain 不 reference HTTP package — 對應測試：architecture / unit
- [ ] **AC-2 CorrelationId propagation**：inbound request 帶 `X-Correlation-Id` 時，sample outbound request 帶同值 header — 對應測試：integration
- [ ] **AC-3 timeout / retry**：transient 5xx 或 timeout 依 config retry；4xx 非 408/429 不 retry；Day 1 circuit breaker disabled — 對應測試：integration
- [ ] **AC-4 outbound log**：每次 outbound call 產生一行 structured log，含 serviceName / statusCode / durationMs / correlationId / retryAttempt / outcome，且不含 secret / query / body — 對應測試：integration
- [ ] **AC-5 failure mapping**：4xx/5xx / timeout 被包成 `Result<T>` typed failure，不讓 use case 看到 raw HttpRequestException — 對應測試：unit / integration
- [ ] **AC-6 config discipline**：repo 內沒有真實 endpoint secret / API key；options binding 有 validation；缺必要設定時 startup fail fast — 對應測試：unit / governance grep
- [ ] **AC-7 sample 不碰業務服務**：sample client 不連 kie.ai / Gemini / 大數據 / 派報 / SMTP — 對應測試：code review
- [ ] **AC-8 build clean**：`dotnet build` / `dotnet test` 通過 — 對應測試：build smoke

## 實作連結（完工後填）

- Correlation handler：`<src/backend/ETOmniverse.Common/Http/CorrelationIdPropagationHandler.cs>`
- Outbound logging handler：`<src/backend/ETOmniverse.Common/Http/OutboundHttpLoggingHandler.cs>`
- Resilience registration：`<src/backend/ETOmniverse.Infrastructure/Http/HttpClientRegistrationExtensions.cs>`
- Options model：`<src/backend/ETOmniverse.Infrastructure/Http/ExternalServiceOptions.cs>`
- Sample port：`<src/backend/ETOmniverse.Domain/Common/Ports/ISampleEchoPort.cs>`
- Sample client：`<src/backend/ETOmniverse.Infrastructure/ExternalServices/SampleEcho/>`
- Tests：`<tests/backend/ETOmniverse.Infrastructure.Tests/HttpOutbound/>`
- 主要 PR：#TBD

## 依賴決策（NuGet）

| 套件 | 用途 | 為什麼必要 | 替代方案評估 |
|---|---|---|---|
| `Microsoft.Extensions.Http` | `IHttpClientFactory` typed client | .NET 官方 HTTP client factory pattern | 手動 new HttpClient 易造成 socket / lifetime 問題 |
| `Microsoft.Extensions.Http.Resilience` | timeout / retry / backoff / optional breaker options | .NET 官方 resilience pipeline，與 `IHttpClientFactory` 整合 | raw Polly 可行但較容易讓每個 client 自行組 policy；若實作時發現 .NET 10 相容性問題，再由 plan 明確記錄 deviation |

**不引入**：service mesh、常駐 mock server、外部服務 SDK。

## Open questions

- [x] Q-F004-001: Resilience package 最終採 `Microsoft.Extensions.Http.Resilience` 還是 raw Polly extensions？— **Resolved 2026-05-09**：優先採 `Microsoft.Extensions.Http.Resilience`。理由：它是 .NET 官方 IHttpClientFactory resilience pipeline；raw Polly 只作為相容性 deviation fallback，不作為預設設計。
- [x] Q-F004-002: typed failure 是否沿用 F-003/F-005 後擴充的 `Result<T>`，或先定義 outbound-specific failure model？— **Resolved 2026-05-09**：沿用 F-003 的 `Result<T>` + `ErrorKind.ExternalDependency`。外部服務細節放在 error code / safe message，不建立第二套 outbound failure abstraction。

## 變更記錄

| 日期 | 變更 | PR |
|---|---|---|
| 2026-05-09 | 初版 (status: draft) | #TBD |
