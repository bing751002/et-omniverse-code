# Roadmap: ET-Omniverse v2

## Overview

Milestone v1.0 — GSD/SDD Process Validation。目標不是交付軟體功能，而是讓整套 SDD + GSD 工具棧在 et-omniverse-v2 這個 repo 跑通一次完整循環，產物包含一個前端 login → welcome demo 頁、一份人寫的 SDD spec、以及可在 commit history 追溯的 governance 記錄，讓 team 能觀察重現整個流程。

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [x] **Phase 1: Frontend Login Demo** - 跑完一輪完整 GSD 流程並產出 login → welcome 前端 demo，驗證 SDD spec、governance hook 與 GSD 工具棧可在此 repo 端到端運作 (completed 2006-04-XX)
- [x] **Phase 2: Backend Logging Foundation** - 共用 log 基礎建設（Serilog JSON console + CorrelationId + request log middleware + masking），落地 F-002 spec、為後續 HTTP / DB / 模組開發提供 LogContext 基礎 (completed 2006-05-09)
- [x] **Phase 3: HTTP inbound base** - 建立 inbound API contract foundation（Result<T> / ErrorKind → ProblemDetails、global exception handler、FluentValidation endpoint filter、Ping sample、CORS / OpenAPI policy），對應 F-003 (completed 2006-05-09)
- [x] **Phase 4: HTTP outbound base** - 建立 outbound HTTP typed client foundation（IHttpClientFactory、CorrelationId propagation、latency/status logging、timeout/retry resilience、sample typed client），對應 F-004 (completed 2006-05-09)
- [x] **Phase 5: Persistence foundation** - 建立 MSSQL / EF Core persistence foundation（DbContext skeleton、baseline migration、UoW / repository base、Testcontainers MSSQL fixture、seed boundary），對應 F-005 (completed 2006-05-09)
- [x] **Phase 6: Test-mode authentication** - env-guarded TestAuthenticationHandler + `X-Test-User` / `X-Test-Roles` header + Production hard-fail，讓 v1.1+ 業務 phase 可對 `[Authorize]` endpoint 寫 integration / E2E test，對應 F-006 / D-19 (completed 2006-05-09)
- [ ] **Phase 7: Testability foundation** - TimeProvider 強制 (D-20) + Respawn / TransactionalTestBase / MsSqlContainerFixture (D-21) + `/api/test/*` namespace 集中註冊 + Production hard-fail (D-22)，對應 F-007

## Phase Details

### Phase 1: Frontend Login Demo
**Goal**: Team 可以觀察到一輪完整 GSD 流程（add-phase → discuss → plan → execute → verify → ship），前端有可手動操作的 login → welcome 跳轉，SDD spec 與 governance hook 均有實際產出
**Depends on**: Nothing (first phase)
**Requirements**: DEMO-01, DEMO-02, DEMO-03, UI-01, UI-02, UI-03, DOC-01
**Success Criteria** (what must be TRUE):
  1. `pnpm dev` 啟動前端後，瀏覽器可手動填入 username + password、點 submit，頁面自動跳轉至 `/welcome` 並顯示 "Welcome" 文字
  2. `.planning/phases/01-frontend-login-demo/` 下存在完整 GSD artifacts（PLAN.md、SUMMARY.md 等），可重現整輪 GSD 流程
  3. `docs/specs/F-001-frontend-login-page.md` 存在，frontmatter 完整，status 為 `implemented`，內容由人撰寫（非 GSD 自動生成）
  4. 至少一次真實 commit 觸發 pre-commit hook，且 governance script 正確執行（含 rationale bypass 機制實際使用一次）
  5. Team 可取得 walkthrough 素材，說明 spec vs PLAN 差異、governance 機制、以及 commit history 如何對應 PLAN tasks
**Plans**: 3 plans
Plans:
- [x] 01-01-PLAN.md — F-001 spec authoring + vite.config.ts fix (D-11/D-18 rationale-bypass) + vue-router install + F-001 status: draft → approved (DEMO-02, DEMO-03)
- [x] 01-02-PLAN.md — vue-router config + Login.vue + Welcome.vue + main.ts/App.vue wiring + F-001 status: approved → implementing (UI-01, UI-02)
- [x] 01-03-PLAN.md — Manual UAT checkpoint + F-001 status: implementing → implemented + WALKTHROUGH.md (UI-03, DEMO-01, DOC-01)
**UI hint**: yes

## Progress

**Execution Order:**
Phases execute in numeric order: 1, 2, 3, 4, 5, 6, 7

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Frontend Login Demo | 3/3 | Complete   | 2006-04-XX |
| 2. Backend Logging Foundation | 6/5 | Complete   | 2006-05-09 |
| 3. HTTP inbound base | 6/5 | Complete   | 2006-05-09 |
| 4. HTTP outbound base | 5/5 | Complete | 2006-05-09 |
| 5. Persistence foundation | 5/5 | Complete | 2006-05-09 |
| 6. Test-mode authentication | 4/4 | Complete | 2006-05-09 |
| 7. Testability foundation | 0/5 | Planned | — |

### Phase 2: Backend Logging Foundation

**Goal:** 落地 F-002 spec — 共用 log 基礎建設（Serilog console JSON + CorrelationId middleware + request log middleware with masking + body cap + ICurrentUser stub + IBackgroundCorrelationScope helper + InMemory test sink + CONVENTIONS/INFRA/docker-compose 文件修補 + CI 禁區掃描），讓後續 HTTP / DB / 模組開發都能直接消費 LogContext 與結構化 log
**Spec**: `docs/specs/F-002-backend-logging-foundation.md` (status: draft → 規劃時 → approved → implementing → implemented，per D-08 4-step)
**Requirements**: AC-1, AC-2, AC-3, AC-4, AC-5, AC-6, AC-7, AC-8, AC-9, AC-10, AC-11 (F-002 spec 11 條 AC)
**Depends on:** Phase 1 (process validation only — no code dependency)
**Plans:** 6/5 plans complete

Plans:
- [x] 02-01-PLAN.md — Foundation + Test Sink (Serilog bootstrap + enrichers + BootstrapLogger + InMemorySink TestSupport) + F-002 status flip draft → approved → implementing + spec .ps1 → .py drift fix (AC-1, AC-5, AC-6)
- [x] 02-02-PLAN.md — HTTP middleware (CorrelationId + RequestLogging with body cap + additive mask + /health 排除) + MaskFields baseline + LoggingOptions + Common.Tests project (AC-2, AC-3, AC-4)
- [x] 02-03-PLAN.md — Background helper (IBackgroundCorrelationScope + LoggingHeartbeatHostedService) + ICurrentUser port + AnonymousCurrentUser stub + DI 註冊 (AC-7)
- [x] 02-04-PLAN.md — Docs / Infra / CI (CONVENTIONS.md Logging 段 + INFRA.md Day 1 retention policy + docker-compose json-file rotation + scripts/check-no-console-write.py + pre-commit hook 串接) (AC-8, AC-9, AC-10)
- [x] 02-05-PLAN.md — 收尾 (build/test smoke + spec 實作連結填實 + F-002 status flip implementing → implemented) (AC-11)

**Wave Structure:**
- Wave 1: 02-01 (foundation — 後續 plan 依賴 SerilogSetup + LoggingTestWebAppFactory)
- Wave 2: 02-02, 02-03 (兩 plan 都僅 depends_on 02-01；可並行，但 Program.cs 觸碰同檔，建議 sequential commit)
- Wave 3: 02-04 (純文件 / infra / CI；不動 src/backend code，可與 wave 2 並行)
- Wave 4: 02-05 (嚴格依賴前 4 plan 全部 done — 跑 dotnet build/test smoke + status flip 收尾)

### Phase 3: HTTP inbound base

**Goal:** [To be planned]
**Requirements**: TBD
**Depends on:** Phase 2
**Plans:** 6/5 plans complete

Plans:
- [ ] TBD (run /gsd-plan-phase 3 to break down)

### Phase 4: HTTP outbound base

**Goal:** 建立 outbound HTTP typed client foundation（IHttpClientFactory typed client + Domain port convention、CorrelationId propagation、latency/status logging、timeout/retry resilience、SampleEcho test-only client），讓後續外部服務整合不用各自發明 HTTP client 基礎
**Spec**: `docs/specs/F-004-http-outbound-base.md`
**Requirements**: AC-1, AC-2, AC-3, AC-4, AC-5, AC-6, AC-7, AC-8
**Depends on:** Phase 3
**Plans:** 5/5 plans complete

Plans:
- [x] 04-01-PLAN.md — Contracts/options/DI foundation（Domain SampleEcho port、ExternalServiceOptions、package refs、config validation、F-004 status: draft → approved → implementing）(AC-1, AC-6)
- [x] 04-02-PLAN.md — Outbound handlers（CorrelationId propagation + latency/status structured logging + secret-safe log assertions）(AC-2, AC-4)
- [x] 04-03-PLAN.md — Resilience pipeline（timeout/retry/backoff、non-retry 4xx、disabled circuit breaker default；implementation uses ResilientHttpClientHandler deviation）(AC-3, AC-6)
- [x] 04-04-PLAN.md — SampleEcho typed client（test-only fake server、Result<T> failure mapping、200/400/500/timeout coverage）(AC-1, AC-2, AC-3, AC-4, AC-5, AC-7)
- [x] 04-05-PLAN.md — Docs/spec closeout（ExternalServices config docs、secret grep、build/test smoke、F-004 implemented、phase summary）(AC-6, AC-8)

### Phase 5: Persistence foundation

**Goal:** 建立 MSSQL / EF Core persistence foundation（partial DbContext、SQL Server registration、baseline migration、UoW/repository base、Testcontainers MSSQL fixture、seed boundary），讓第一個業務 entity 不需要混入 persistence 基礎建設
**Spec**: `docs/specs/F-005-persistence-foundation.md`
**Requirements**: AC-1, AC-2, AC-3, AC-4, AC-5, AC-6, AC-7, AC-8, AC-9, AC-10
**Depends on:** Phase 4
**Plans:** 5/5 plans complete

Plans:
- [x] 05-01-PLAN.md — EF/MSSQL skeleton（package refs、partial DbContext、design-time factory、DI、ready health check、F-005 status: draft → approved → implementing）(AC-1, AC-2, AC-3)
- [x] 05-02-PLAN.md — Naming + baseline migration（snake_case/plural convention、empty InitialBaseline、migration CLI script/docs）(AC-4, AC-5, AC-6)
- [x] 05-03-PLAN.md — Repository/UoW foundation（IAggregateRoot、IRepository<T>、IUnitOfWork、RepositoryBase、UnitOfWork）(AC-7)
- [x] 05-04-PLAN.md — Testcontainers MSSQL fixture（container migrate + sample aggregate CRUD + Docker unavailable behavior）(AC-8, AC-4, AC-7)
- [x] 05-05-PLAN.md — Seed/docs/spec closeout（dev seed vs prod migration data、build/test smoke、F-005 implemented、phase summary）(AC-9, AC-10)

### Phase 6: Test-mode authentication

**Goal:** 落地 F-006 spec — env-guarded TestAuthenticationHandler + `X-Test-User` / `X-Test-Roles` header + Production hard-fail，讓 v1.1+ 業務 phase 開工就能對 `[Authorize]` endpoint 寫 integration / E2E test，不需要等真實 auth phase
**Spec**: `docs/specs/F-006-test-mode-authentication.md`
**Requirements**: AC-1, AC-2, AC-3, AC-4, AC-5, AC-6, AC-7, AC-8, AC-9, AC-10
**Depends on:** Phase 5
**Plans:** 4 plans

Plans:
- [x] 06-01-PLAN.md — Handler / Options / Defaults / AddTestAuthentication extension + unit test + F-006 status: draft → approved → implementing (AC-1, AC-2)
- [x] 06-02-PLAN.md — Program.cs IntegrationTest-only Authentication 註冊 + Production startup hard-fail guard + integration test (AC-7)
- [x] 06-03-PLAN.md — Features/Test/Auth/ fixture endpoints (whoami / admin) + 5 條 integration test (authenticated 200、no header 401、role missing 403、role match 200、X-Correlation-Id 留存) (AC-3, AC-4, AC-5, AC-6, AC-9)
- [x] 06-04-PLAN.md — AuthenticatedTestClientExtensions helper + spec 實作連結填實 + F-006 status: implementing → implemented + Phase 06 PHASE-SUMMARY (AC-8, AC-10)

**Wave Structure:**
- Wave 1: 06-01 (foundation — handler/options/extension 沒 dependency)
- Wave 2: 06-02 (depends on 06-01 — Program.cs 用 AddTestAuthentication + Defaults const)
- Wave 3: 06-03 (depends on 06-02 — fixture endpoint 需要 IntegrationTest env auth pipeline 已就緒)
- Wave 4: 06-04 (depends on 06-03 — helper 內含 e2e smoke 打 /test/auth/admin，需要 06-03 endpoint)

### Phase 7: Testability foundation

**Goal:** 落地 F-007 spec 三大區塊 — (A) TimeProvider 全棧強制 + check-no-datetime-now.py 守門 (D-20)、(B) Respawn + TransactionalTestBase + MsSqlContainerFixture 測試 DB lifecycle (D-21)、(C) Test-only endpoints `/api/test/*` namespace 集中註冊 + Production hard-fail (D-22)，讓 v1.1+ 業務 phase 不用各自重造測試基建
**Spec**: `docs/specs/F-007-testability-foundation.md`
**Requirements**: AC-A1~A6, AC-B1~B6, AC-C1~C7, AC-D1, AC-D2, AC-D3
**Depends on:** Phase 6
**Plans:** 5 plans

Plans:
- [x] 07-01-PLAN.md — A 區塊 TimeProvider (DI 註冊 + check-no-datetime-now.py + pre-commit + FakeTimeProviderFixture + CONVENTIONS 段 + F-007 status draft -> approved -> implementing) (AC-A1, A2, A3, A4, A5, A6)
- [ ] 07-02-PLAN.md — B 區塊基建 (MsSqlContainerFixture + DatabaseCollection + Respawn/Testcontainers.MsSql pkg 加入 + 兩 class 共用 container smoke) (AC-B1, B4, B5)
- [ ] 07-03-PLAN.md — B 區塊執行 (RespawnDatabaseReset + TransactionalTestBase + 50-fact 速度 smoke + __EFMigrationsHistory 保留整合測) (AC-B2, B3, B6)
- [ ] 07-04-PLAN.md — C 區塊 (MapTestOnlyEndpointsExtensions + Program.cs 重構 + Production hard-fail + F-002 4 file path migration + check-test-endpoints.py + pre-commit) (AC-C1, C2, C3, C4, C5, C6, C7)
- [ ] 07-05-PLAN.md — 收尾 (dotnet build/test solution 全綠 + F-007 status implementing -> implemented + 實作連結填實 + Phase 07 PHASE-SUMMARY) (AC-D1, D2, D3)
