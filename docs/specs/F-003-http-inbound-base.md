---
id: F-003
title: HTTP inbound base
module:
status: approved
owner: jimmyliao
created: 2026-05-09
updated: 2026-05-09
supersedes:
superseded-by:
related-adr: []
related-interview: []
phase: 03-http-inbound-base
---

# F-003：HTTP inbound base

## 業務背景

F-003 是所有後端 API endpoint 的 inbound 契約基礎。F-002 先落地 CorrelationId / request log 後，F-003 釘住 endpoint 如何回傳成功、錯誤、validation failure、未處理例外，以及 OpenAPI 呈現方式，避免後續 12 個模組各自發明 HTTP response 形狀。

對應 7-step：與特定業務 step 無關，是所有模組共用的 API contract foundation。

## 用戶故事

1. As a frontend developer consuming ET-Omniverse APIs, I want every API error to use the same ProblemDetails shape, so that generated clients and UI error handling do not need module-specific branches.
2. As a backend developer adding a Minimal API endpoint, I want Result-to-HTTP and validation mapping helpers, so that endpoint body stays focused on binding, validation, use case call, and response mapping.
3. As an operator investigating an API error, I want every ProblemDetails response to include the same correlation id as `X-Correlation-Id`, so that user-facing error reports can be tied back to logs from F-002.

## 範圍

### In scope

- **Result -> HTTP 統一轉換**
  - 以現有 `ETOmniverse.Domain.Common.Model.Result` 為起點，新增 API 層 extension：`ToHttpResult()`
  - 在 Domain 補齊 generic `Result<T>`，但不讓 Domain 知道 HTTP status / ProblemDetails
  - error model 使用 Domain-level `ErrorKind`，固定集合：`Validation` / `NotFound` / `Conflict` / `Unauthorized` / `Forbidden` / `ExternalDependency` / `Unexpected`
  - `Result.Success()` -> 200 / 204 依 endpoint 決定，helper 需支援 no-content 與 value response
  - `Result<T>.Success(value)` -> 200 + JSON value
  - `Result.Failure(errorCode, errorMessage)` -> RFC 7807 ProblemDetails
  - `ErrorKind` -> HTTP status mapping 固定在 API 層：
    - `Validation` -> 400
    - `Unauthorized` -> 401
    - `Forbidden` -> 403
    - `NotFound` -> 404
    - `Conflict` -> 409
    - `ExternalDependency` -> 502
    - `Unexpected` -> 500

- **ProblemDetails 標準化**
  - 所有錯誤 response 回 RFC 7807 形狀
  - 必含 `traceId`，值取 F-002 的 CorrelationId
  - 不輸出 stack trace、internal exception message、connection string、secret
  - error code 放在 extension field，例如 `code`
  - response content type 為 `application/problem+json`
  - response shape：
    ```json
    {
      "type": "https://et-omniverse/errors/<code>",
      "title": "<short title>",
      "status": 400,
      "detail": "<safe message>",
      "instance": "/api/...",
      "traceId": "<correlation id>",
      "code": "<domain error code>",
      "errors": { "field": ["message"] }
    }
    ```
  - `errors` 只在 validation failure 時出現

- **Global exception handler**
  - 未 catch exception -> 500 ProblemDetails
  - log level 為 Error，且同一 request log / exception log 共享 CorrelationId
  - Development 可由 log 查 stack trace；HTTP response 不外洩 stack trace

- **FluentValidation endpoint filter pattern**
  - FluentValidation 放 `Api/Features/<Feature>/Adapter/In/Validation`
  - Minimal API 透過 endpoint filter 或 route group extension 做 validation
  - validation failure -> 400 ProblemDetails
  - response body 包含欄位錯誤集合，且含 `traceId`
  - 不在 Domain reference FluentValidation
  - Validator registration 以 feature assembly scanning 為主；沒有 validator 的 endpoint 不套 validation filter

- **Common Ping sample**
  - 建 `Api/Features/Common/Ping` 作為第一個 inbound pipeline 範本
  - 覆蓋 route group、validation、Result -> HTTP、ProblemDetails、OpenAPI tag
  - Ping sample 不做業務邏輯、不連 DB、不做 auth
  - 固定 sample endpoints：
    - `GET /api/common/ping` -> `200 { "message": "pong" }`
    - `POST /api/common/ping/echo` -> request `{ "message": "<1-50 chars>" }`，valid 時 echo，invalid 時 400 ProblemDetails
    - `GET /api/common/ping/fail` 只在 `IntegrationTest` environment 註冊，用來測 exception handler，不出現在 Development / Staging / Production

- **CORS 策略**
  - Development allow-all，方便 Vite dev server
  - Staging / Production 白名單由 config 提供
  - 沒有設定 production allowlist 時 fail closed，不開全域 allow-all
  - config key：`Cors:AllowedOrigins`

- **OpenAPI 約定**
  - OpenAPI title / version / common error schema
  - Feature slice 以 tag 分組
  - schema 命名不可混 module / endpoint 名稱造成衝突
  - OpenAPI endpoint 僅 Development / IntegrationTest 開啟；Staging / Production 預設不 expose，除非 ops config 明確開啟
  - config key：`OpenApi:Enabled`

### Out of scope

- API versioning（內部 50 人 + on-prem，等真的要 break change 再加）
- Rate limiting（內部系統，Day 1 不加）
- 真實 auth / RBAC middleware（Identity 模組處理；本 spec 只保留 hook）
- Frontend OpenAPI client generation（後續 frontend integration phase）
- Business endpoints（只做 Common/Ping sample）
- 調整 F-002 logging middleware 行為（只消費 CorrelationId / log context）
- Outbound HTTP typed client（F-004）
- DB / repository / migration（F-005）

## 驗收條件

- [ ] **AC-1 Result -> HTTP mapping**：`Result.Success()` / `Result<T>.Success(value)` / `Result.Failure(...)` 都透過同一 extension 回傳，failure 依 `ErrorKind` 產生對應 status code ProblemDetails 且含 `code` / `traceId` — 對應測試：api
- [ ] **AC-2 Global exception handler**：sample endpoint 故意 throw 時，HTTP response 為 500 ProblemDetails，不含 stack trace，log 含 Error + CorrelationId — 對應測試：api
- [ ] **AC-3 Validation failure**：Ping sample 的 invalid request 回 400 ProblemDetails，含欄位錯誤集合與 `traceId` — 對應測試：api
- [ ] **AC-4 CorrelationId 對齊**：ProblemDetails `traceId` 等於 response header `X-Correlation-Id` — 對應測試：api
- [ ] **AC-5 CORS policy**：Development allow-all；Production 未設定白名單時不允許任意 origin — 對應測試：unit / api
- [ ] **AC-6 OpenAPI metadata**：OpenAPI 文件含 title/version、Common/Ping tag、ProblemDetails schema，且 Production 預設不 expose OpenAPI endpoint — 對應測試：api / snapshot
- [ ] **AC-7 Ping sample**：`GET /api/common/ping`、`POST /api/common/ping/echo`、IntegrationTest-only `/api/common/ping/fail` 跑通完整 inbound pipeline — 對應測試：api
- [ ] **AC-8 endpoint shape**：Ping sample 位於 `Api/Features/Common/Ping`，endpoint body 只做 binding、validation、use case/result mapping，不寫業務邏輯 — 對應測試：code review
- [ ] **AC-9 build clean**：`dotnet build` / `dotnet test` 通過 — 對應測試：build smoke

## 實作連結（完工後填）

- Result mapping：`<src/backend/ETOmniverse.Api/Features/Common/ProblemDetails/ResultHttpExtensions.cs>`
- Result model：`<src/backend/ETOmniverse.Domain/Common/Model/Result.cs>`
- Error kind：`<src/backend/ETOmniverse.Domain/Common/Model/ErrorKind.cs>`
- ProblemDetails factory：`<src/backend/ETOmniverse.Api/Features/Common/ProblemDetails/ProblemDetailsExtensions.cs>`
- Exception handler：`<src/backend/ETOmniverse.Api/Middleware/GlobalExceptionHandler.cs>`
- Validation filter：`<src/backend/ETOmniverse.Api/Features/Common/Validation/ValidationEndpointFilter.cs>`
- Ping endpoint：`<src/backend/ETOmniverse.Api/Features/Common/Ping/>`
- API tests：`<tests/backend/ETOmniverse.Api.Tests/HttpInbound/>`
- 主要 PR：#TBD

## 依賴決策（NuGet）

| 套件 | 用途 | 為什麼必要 | 替代方案評估 |
|---|---|---|---|
| `FluentValidation` | request validation | CONVENTIONS 已指定 FluentValidation 放 Adapter/In | 手寫 validation 會讓每個 endpoint 發明不同錯誤格式 |
| `FluentValidation.DependencyInjectionExtensions` | 掃描 validator / DI 註冊 | 減少手動註冊錯漏 | 可手動註冊，但模組數多時易漏 |

**不引入**：API versioning、rate limit 套件、auth 套件。

## Open questions

- [x] Q-F003-001: `Result` 是否要在本 spec 擴成 generic `Result<T>` / typed error，或先只在 API 層 adapter 處理現有 non-generic `Result`？— **Resolved 2026-05-09**：本 spec 補 `Result<T>` 與 Domain-level `ErrorKind`，但 HTTP status / ProblemDetails mapping 只放 API 層。理由：後續 endpoint 會自然需要 typed success value；若只在 API adapter 包 value，會讓每個 feature 重複造 mapping。Domain 仍不依賴 HTTP。

## 變更記錄

| 日期 | 變更 | PR |
|---|---|---|
| 2026-05-09 | 初版 (status: draft) | #TBD |
| 2026-05-09 | status: draft → approved（per .planning/phases/03-http-inbound-base/03-CONTEXT.md，9 條 AC + 5 plan 切分鎖定） | #TBD |
