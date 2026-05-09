---
id: F-002
title: Backend logging foundation (Serilog + CorrelationId)
module:
status: approved
owner: jimmyliao
created: 2026-05-09
updated: 2026-05-09
supersedes:
superseded-by:
related-adr: []
related-interview: []
phase:
---

# F-002：Backend logging foundation (Serilog + CorrelationId)

## 業務背景

ET-Omniverse 後端目前只有 5 個 csproj 殼 + `IClock` + `Result` + Health endpoint，12 業務模組尚未開工。準備進入多人協作開發前，必須先把**共用基礎建設**釘住，避免每個模組 owner 各自造 log 寫法、各自處理 CorrelationId、外部 client wrapper 各種 latency log 風格。

Logging 是基礎建設依賴鏈的最上游：

```
F-002 Logging foundation ─┬─→ F-003 HTTP inbound base （middleware 要 LogContext）
                          └─→ F-004 HTTP outbound base （DelegatingHandler 要 CorrelationId 傳遞）
                                       ↓
                                  F-005 Persistence foundation（EF interceptor 帶 correlationId log SQL）
```

對應 7-step：與業務流程無關，是支撐所有模組的橫切基礎。

## 用戶故事

1. As a backend developer 開發任一模組的 endpoint / use case / job, I want 直接用 `ILogger<T>` 寫 log 並自動帶 CorrelationId / UserId / AppName / Env, so that 我不需要每個模組重新發明 log 規則，且查問題時能用 CorrelationId 串起整個請求。
2. As an operator / SRE 在 production 收到使用者回報問題, I want 從 response header 拿到的 `X-Correlation-Id` 能在 docker logs 一次撈出整個請求鏈（API + 背景 job + 外部呼叫）, so that 不用拼湊多份 log。
3. As a developer 寫 integration test, I want 用 in-memory log sink assert log 內容（特定 event 是否寫了、property 對不對）, so that log 規則本身可被驗證。
4. As a developer 啟動失敗除錯, I want DI 容器尚未建立前的啟動異常也能落到 log（bootstrap 階段）, so that 不會因為 Serilog 還沒初始化就吞掉啟動錯誤。

## 範圍

### In scope

- **Serilog bootstrap**
  - JSON formatter（`RenderedCompactJsonFormatter` / CLEF 格式，跟未來 Seq / EFK 相容）
  - **Console sink only**（Day 1 唯一 sink — container 標準做法，由 docker logs / 未來 fluent-bit 收）
  - `appsettings.*.json` 控制 minimum level + per-namespace override（`Microsoft.*` / `System.*` 預設 Warning）
  - **`BootstrapLogger`**：DI 建立前可用的 console fallback logger（抄 FaceAI `BootstrapLogger.cs` 概念，避免啟動異常被吞）
  - **Serilog SelfLog** 落到 stderr（Serilog 自己壞掉時的 last resort）

- **CorrelationId middleware**（`src/backend/ETOmniverse.Api/Middleware/CorrelationIdMiddleware.cs`）
  - 讀 incoming `X-Correlation-Id` header；無則生成新 GUID（`N` 格式）
  - 寫回 response `X-Correlation-Id` header
  - Push 進 `LogContext`（property 名 `CorrelationId`），整個 request scope 內所有 log 自動帶
  - 必須**早於** Serilog request logging middleware

- **Request logging middleware**（一行 JSON / 請求）
  - 欄位：`method` / `path` / `statusCode` / `durationMs` / `correlationId`（**不含** `userId` — 見下方 enricher 段）
  - **Body capture 預設 disabled**（`Logging:RequestBody:Enabled=false`）
    - 啟用時需 ops 在 appsettings 顯式打開
    - 啟用後仍**強制套 mask + size cap**（ops 不能繞過）
  - **Secret masking**（永遠生效，不管 body capture 啟不啟用 — 例如 query string 內含 token 時 path log 也要 mask）
    - **Baseline mask fields hardcoded in code**（`Common/Logging/MaskFields.cs`）：`password`、`token`、`apiKey`、`secret`、`authorization`、`cookie`、`x-api-key`
    - **Additive config**：`Logging:Mask:AdditionalFields` 只能**加**，不能**覆蓋**或**清空** baseline
    - 實際 mask set = baseline ∪ additional
    - 大小寫不敏感
  - **Body size cap：32 KB**（超過只記 `bodyTruncated=true` + `bodySize`，**不全讀進記憶體**）— 對應 VCR 上傳大 payload 反模式風險
  - 非 JSON body（multipart 等）：不記內容只記 size
  - 5xx 自動升 `Error` level，4xx 自動升 `Warning`
  - 健康檢查 endpoint（`/health`）排除以避免 log 噪音

- **LogContext enrichers**（自動掛上）
  - `CorrelationId`（middleware push）
  - `MachineName`、`EnvironmentName`、`AppName`（從 `appsettings:Log:ExtraInfo:AppName`）
  - `AppVersion`（從 entry assembly informational version）
  - **`UserId` 不在本 spec 範圍**：F-002 不掛 UserId enricher。理由：Identity 模組落地前，`UserId=anonymous` 會讓每行 log 都帶噪音欄位、訓練 operator 忽略此欄。`UserId` enricher 由 Identity 模組接真實使用者後加，並列為 Identity 自身的 AC（「整個 request 鏈 log 帶真實 userId」）

- **`ICurrentUser` port (stub)**（`src/backend/ETOmniverse.Domain/Common/Ports/ICurrentUser.cs`）
  - 介面定義：`UserId` / `IsAuthenticated` / `DisplayName`
  - Day 1 只有 `AnonymousCurrentUser` 實作（永遠 anonymous）
  - Identity 模組（D14 / D18）落地時切真實作，本 spec **不**處理 auth
  - **本 spec 不**把 `ICurrentUser` 接進 LogContext（見上）— 只保留 port 給未來 HTTP / use case 拿

- **`IBackgroundCorrelationScope` helper**（`src/backend/ETOmniverse.Common/Logging/`）
  - 給未來 Quartz job / `IHostedService` 用：`using (scope.Begin()) { ... }` 自動 push 新 CorrelationId 進 `LogContext`
  - **本 spec 不做 JobLoggingDecorator**（背景任務還沒落地，等對應 phase 接）
  - **驗收用 caller**：附一個 `LoggingHeartbeatHostedService`（`Common/Logging/LoggingHeartbeatHostedService.cs`）
    - **預設 disabled**（`Logging:Heartbeat:Enabled=false`），production 不跑
    - 啟用後每 N 秒（`Logging:Heartbeat:IntervalSeconds`，預設 60）寫一行 heartbeat log，每次自建新 CorrelationId
    - 用途：dev / integration test 啟用以驗 helper API 形狀（避免 helper 沒消費者就設計錯）
    - **不**作為 liveness 證據（liveness 用 healthcheck，不靠 heartbeat log）

- **Test in-memory sink**（`tests/...`）
  - 用 `Serilog.Sinks.InMemory` 提供 `LogAssertionExtensions`（`logs.Should().HaveLogContaining(...)` 之類）
  - F-003 之後的 endpoint test 拿來驗 log 規則本身

- **CONVENTIONS.md 補規則**（同 PR）
  - 新增「Logging」段：
    - 業務 code 唯一入口為 `Microsoft.Extensions.Logging.ILogger<T>`
    - 禁直接呼叫 `Serilog.Log.*`、`Console.WriteLine`、`Debug.WriteLine`
    - 禁自寫 `LogContext.PushProperty(...)` — 走 enricher / middleware / `IBackgroundCorrelationScope`
    - Log message 用 structured template（`logger.LogInformation("User {UserId} did {Action}", id, action)`），禁字串內插

- **INFRA.md 修正**（同 PR）
  - 「Observability Staging」表格 Day 1 行：`Serilog JSON + CorrelationId + console/file sink` → **`Serilog JSON + CorrelationId + console sink only + docker json-file rotation`**
  - 補充段「Day 1 log retention policy」明寫：
    - app 端：只寫 console JSON sink，**不**做 in-app file sink（避免 fluent-bit 上線後雙寫 / 與 host 權限糾纏）
    - compose 端：對 `api` / `web` service 設 `logging.driver: json-file` + `max-size: 50m` + `max-file: 5`，避免 container log 撐爆 host 磁碟
    - **限制聲明**：docker json-file rotation 是 **size-based 不是 time-based**，**不保證**「保留一週」或任何時間級別追查能力。staging / lab 若需固定保留 N 天日誌，**P1.x 必須提前**上 Seq 或其他 log collector，不能假裝 docker rotation 撐得到 P1.6 EFK 才上來
  - 加註：`docker/docker-compose.infra.yml` 的 `seq` service 保留，但 API 端 Day 1 不掛 Seq sink；P1.x 開啟時改 appsettings 即可

- **`docker-compose.yml` 修正**（同 PR）
  - 對 `api` service 加 `logging.driver: json-file` + `options.max-size: 50m` + `options.max-file: "5"`
  - 對 `web` service 同樣設定（雖然 nginx access log 已分開，container log 也要 cap）
  - 不改 `docker-compose.infra.yml`（infra 服務 mssql / redis / seq 各自有 log 行為，不在本 spec 範圍）

### Out of scope

- **File sink**（container 反模式 — 留給 fluent-bit / docker logs；未來若 on-prem VM 真有需要再評估）
- **Seq sink**（INFRA P1.x 才接；本 spec 只留 config schema hook，不啟）
- **EFK / Fluent Bit**（INFRA P1.6+）
- **Email sink**（FaceAI 有，過早；ARCHITECTURE 沒列為需求）
- **Distributed tracing / OpenTelemetry**（ARCHITECTURE 明說不做）
- **業務 metric / 計數器**（業務 metric 走 MSSQL，不混進 log / 時序 DB — CONVENTIONS 已寫陷阱）
- **PII 細部 masking**（先做欄位黑名單，深度 PII 規則等第一個敏感 log 出現再補）
- **Audit log writer**（獨立模組，與此 spec 無關）
- **Quartz / `IHostedService` `JobLoggingDecorator` 整合**（本 spec 只交付 `IBackgroundCorrelationScope` helper，真正 decorator 留給背景任務 phase）
- **Auth middleware**（`ICurrentUser` 只給 stub；Identity 模組接真實作）
- **HTTP request id 與 W3C TraceContext 對齊**（不做 distributed tracing 就不需要對齊）

## 驗收條件

- [ ] **AC-1 console JSON 輸出格式正確**：`dotnet run` 啟動 API，console 第一行 log 為 CLEF JSON 格式，含 `@t` / `@m` / `@l` / `AppName` / `EnvironmentName` 欄位 — 對應測試：integration（`WebApplicationFactory` + `InMemorySink` assert）
- [ ] **AC-2 CorrelationId middleware 行為正確**：
  - [ ] 無 header 時自動生 GUID 並寫 response `X-Correlation-Id` — 對應測試：integration
  - [ ] 帶 header 時透傳同值 — 對應測試：integration
  - [ ] 該 request 內所有 log line 都帶相同 `CorrelationId` property — 對應測試：integration
- [ ] **AC-3 Request log 行為正確**：
  - [ ] 一個 request 對應一行 summary log，含 method / path / statusCode / durationMs / correlationId — 對應測試：integration
  - [ ] log line **不含** `userId` 欄位（F-002 不掛 UserId enricher）— 對應測試：integration
  - [ ] 5xx → `Error` level；4xx → `Warning`；2xx/3xx → `Information` — 對應測試：integration
  - [ ] `/health` 不產生 request log — 對應測試：integration
- [ ] **AC-4 Secret masking + body capture**：
  - [ ] body capture 預設 disabled — log 不含 request body 內容（只含 metadata）— 對應測試：integration
  - [ ] 啟用 body capture 後，POST JSON body 含 `password` 欄位時，log 中該欄位被 mask（驗 baseline list）— 對應測試：integration
  - [ ] `Logging:Mask:AdditionalFields=customSecret` 設定下，`customSecret` 同樣被 mask（驗 additive）— 對應測試：integration
  - [ ] `Logging:Mask:Fields` 之類試圖**覆蓋** baseline 的設定 key 不存在 / 被忽略 — 對應測試：unit（mask config binding 測試）
  - [ ] query string 內 `?token=xxx` 在 path log 中被 mask（不論 body capture 是否啟用）— 對應測試：integration
  - [ ] POST body > 32KB 時，log 含 `bodyTruncated=true` 且不含完整內容 — 對應測試：integration
  - [ ] 非 JSON body（如 multipart）只記 size 不記內容 — 對應測試：integration
- [ ] **AC-5 Enricher 自動掛上**：任一 log line 都帶 `MachineName` / `EnvironmentName` / `AppName` / `AppVersion` 且**不含** `UserId` — 對應測試：integration
- [ ] **AC-6 BootstrapLogger 工作**：強制觸發啟動失敗（壞掉的 connection string）時，error 仍透過 BootstrapLogger 落到 stderr，不靜默 — 對應測試：manual smoke + 一個整合測試（用 invalid config 起 host）
- [ ] **AC-7 `IBackgroundCorrelationScope` helper + heartbeat caller**：
  - [ ] `using (scope.Begin())` 區塊內 log 帶**新生成的** CorrelationId（與外層 HTTP request 無關）— 對應測試：unit
  - [ ] 區塊結束後 `LogContext` 還原 — 對應測試：unit
  - [ ] `LoggingHeartbeatHostedService` 預設 disabled（`Logging:Heartbeat:Enabled` 不設或為 false 時不啟動）— 對應測試：integration（驗 service 沒 register / 沒 tick）
  - [ ] 啟用後 heartbeat log 帶**每次都不同**的 CorrelationId（驗 helper 在 background 路徑工作）— 對應測試：integration（啟用 heartbeat + interval=1s + InMemorySink 收 2 個 tick → assert 兩個 correlationId 不同）
- [ ] **AC-8 業務 code 禁區**：`grep` 整個 `src/backend/` 不存在 `Console.WriteLine` / `Serilog.Log.` / `Debug.WriteLine`（除 `Common/Logging/BootstrapLogger.cs` 自身外）— 對應測試：CI script（一支 `scripts/check-no-console-write.py`）
- [ ] **AC-9 CONVENTIONS.md 補完**：「Logging」段新增、`grep` 找得到「禁直接呼叫 `Serilog.Log.*`」字樣 — 對應測試：manual review
- [ ] **AC-10 INFRA.md + docker-compose.yml 修正**：
  - [ ] INFRA.md「Observability Staging」表格 Day 1 行只有 console sink + json-file rotation — 對應測試：manual review
  - [ ] INFRA.md 補「Day 1 log retention policy」段，含 size-based rotation 限制聲明 — 對應測試：manual review
  - [ ] `docker-compose.yml` `api` / `web` service 設了 `logging.driver: json-file` + `max-size: 50m` + `max-file: 5` — 對應測試：manual review + `docker compose config` 解析驗證
- [ ] **AC-11 build clean**：`dotnet build` warnings-as-errors 通過、`dotnet test` 全部 pass — 對應測試：build smoke

## 實作連結（完工後填）

> 路徑以 `<...>` 標記表示尚未存在。

- Serilog setup：`<src/backend/ETOmniverse.Common/Logging/SerilogSetup.cs>`
- BootstrapLogger：`<src/backend/ETOmniverse.Common/Logging/BootstrapLogger.cs>`
- Enrichers：`<src/backend/ETOmniverse.Common/Logging/Enrichers/>`
- CorrelationId middleware：`<src/backend/ETOmniverse.Api/Middleware/CorrelationIdMiddleware.cs>`
- Request logging middleware：`<src/backend/ETOmniverse.Api/Middleware/RequestLoggingMiddleware.cs>`
- ICurrentUser stub：`<src/backend/ETOmniverse.Domain/Common/Ports/ICurrentUser.cs>` + `<src/backend/ETOmniverse.Infrastructure/Identity/AnonymousCurrentUser.cs>`
- Background correlation scope：`<src/backend/ETOmniverse.Common/Logging/IBackgroundCorrelationScope.cs>`
- Test sink helper：`<tests/ETOmniverse.TestSupport/Logging/InMemoryLogAssertions.cs>`
- CONVENTIONS.md 補丁：`docs/CONVENTIONS.md`（新增 Logging 段）
- INFRA.md 補丁：`docs/INFRA.md`（Observability Staging 表格 Day 1 行）
- CI 禁區掃描：`<scripts/check-no-console-write.py>`
- 主要 PR：#TBD

## 參考來源

從現有專案抄/不抄的點位（避免讀者跑去全抄）：

- **抄結構**：`D:\FaceAI\src\backend\FaceAi.Common\Log\SerilogSetting.cs`
  - `RunInternal` 開關式設定（Console / Seq / Email / File 各自獨立 enable）
  - `BootstrapLogger` 概念（line 41）
  - `RenderedCompactJsonFormatter` 選型（line 92）
  - `EnrichProperty`（line 392，`AppName` + `EnvironmentName`）
  - `InitSelfLog`（line 316，Serilog 壞掉時的 fallback）
- **抄概念**：`D:\money_csp\customer-service-backend\Ehs.CustomerService.API\Middleware\LoggerMiddleware.cs`
  - Body buffering + password 欄位移除（line 87-103）— **ET 加 32KB cap 與設定驅動黑名單**
- **不抄**：
  - FaceAI 的 `FaceAi.Common.Log.Logger`（雙層 wrapper，ET 直接用 `ILogger<T>`）
  - FaceAI 的 File sink 預設啟用（container 反模式）
  - FaceAI 的 Email sink（過早）
  - csp 的 NLog 整套（ARCHITECTURE 鎖 Serilog）
  - csp 的 `HostLogger` / `NormalLogger` / `ExtendLogger` 三 logger 分流（用 `ILogger.ForContext("LogSource", ...)` 取代）
  - csp 的 `ActivitySourceManger`（用 `Activity.Current` + 自家 CorrelationId）
  - csp 的 `IExceptionFilter`（MVC filter；ET 用 Minimal API → `IExceptionHandler`，且本 spec 不處理，留 F-003）

## 依賴決策（NuGet）

CONVENTIONS.md 規定加 NuGet 前要先評估必要性。本 spec 引入：

| 套件 | 用途 | 為什麼必要 | 替代方案評估 |
|---|---|---|---|
| `Serilog.AspNetCore` | ASP.NET Core 整合 | ARCHITECTURE 已鎖 Serilog | 內建 `ILogger` 不夠（缺結構化 sink、缺 enricher） |
| `Serilog.Sinks.Console` | console JSON sink | Day 1 唯一 sink | — |
| `Serilog.Formatting.Compact` | CLEF JSON formatter | 跟未來 Seq / EFK 相容 | 自寫 formatter 浪費時間 |
| `Serilog.Enrichers.Environment` | `MachineName` / `EnvironmentUserName` | 標準做法 | 自寫 enricher 重複造輪子 |
| `Serilog.Sinks.InMemory` | test sink | AC-1 / AC-2 / AC-3 / AC-4 都要 assert log | 自寫 sink 可行但要花時間 |

**未引入**（本 spec 範圍外）：FluentValidation（F-003）、Polly / Polly.Extensions.Http（F-004）、Microsoft.EntityFrameworkCore.SqlServer（F-005）、EFCore.NamingConventions（F-005）、Testcontainers（F-005）、`Serilog.Sinks.Seq`（P1.x）。

## Open questions

- [x] **Q-F002-001**: Body capture 預設啟用還是預設關閉？— **Resolved 2026-05-09**：預設**關閉**（`Logging:RequestBody:Enabled=false`），需要 debug 時 ops 顯式開啟。理由：預設開啟讓 prod 默默吃效能 + 增加 secret leak 表面積；debug 是低頻情境，付出顯式開關成本合理。
- [x] **Q-F002-002**: Mask fields 設定模式？— **Resolved 2026-05-09**：baseline 在 code hardcode（`Common/Logging/MaskFields.cs`），appsettings 只能透過 `Logging:Mask:AdditionalFields` 加欄位、不能覆蓋。理由：appsettings 完全可覆蓋是 secret leak 級反模式（ops 手滑清空 list = 密碼直接落 log）。

## 變更記錄

| 日期 | 變更 | PR |
|---|---|---|
| 2026-05-09 | 初版 (status: draft) | #TBD |
| 2026-05-09 | status: draft → approved；AC-8 / 實作連結 .ps1 → .py（per CONTEXT.md D-03，跨平台 + 既有 governance scripts 全 python） | #TBD |
