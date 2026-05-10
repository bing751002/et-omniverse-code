# Roadmap: ET-Omniverse v2

## Overview

東森（EHSN）內部 7-step 排播平台 v2。greenfield .NET modular monolith（.NET 10 / EF Core 10 / MSSQL / Quartz.NET）+ Vue 3 前端。完整脈絡見 `README.md` / `docs/ARCHITECTURE.md` / `docs/DECISIONS.md`。

## Milestones

- [x] **v1.0 — GSD/SDD Process Validation + Backend Foundation** (completed 2026-05-09) — 7 phases, 32 plans, F-001..F-007 全部 implemented，audit passed (56/56 ACs, 16/16 wirings, 5/5 E2E flows). 詳見 [`milestones/v1.0-ROADMAP.md`](milestones/v1.0-ROADMAP.md).
- [ ] **v1.1 — Foundation Hardening Before Business** — 6 phases, 21 requirements. 目標是在進入任何 7-step 業務流程前，補齊 verification / environment / execution / dependency source / frontend contract / DB ops foundations. Status: Phase 09, 11, and 12 complete; Phase 08 / 10 still have frontend package-source / Quartz / Playwright blockers; Phase 13 is partial because live DB status/update was not run.

## Active Milestone

**v1.1 — Foundation Hardening Before Business**

不做業務流程。先把 build/test/CI、Docker/config、Quartz/frontend integration 補到可支撐後續業務 milestone。

## Phases

### Phase 8 — Verification Foundation

**Goal:** 建立可信的本機與 CI 驗證入口，讓後續業務改動能被穩定檢查。

**Requirements:** VER-01, VER-02, VER-03, VER-04

**Status:** Partial. Backend restore/build/test, governance, guard scripts, and Docker compose config are verified. Frontend build is blocked by pnpm package source/cache availability.

**Artifacts:** `.planning/phases/08-verification-foundation/08-PHASE-SUMMARY.md`, `.planning/phases/08-verification-foundation/08-VERIFICATION.md`

**Success criteria:**
1. Repo 有單一 documented local verification command，能分層跑 restore/build/test/governance。
2. Jenkins/CI 跑 docs governance、ADR index、spec links、backend guard scripts、backend build/test、frontend build、Docker compose config。
3. Windows MSB3491 / NuGet audit/network 類 known failure mode 有文件化 workaround。
4. CI/local 驗證失敗時能判斷是環境問題、restore/build/test 問題，或 governance 問題。

### Phase 9 — Environment Foundation

**Goal:** 強化 Docker/local stack 與 config contract，讓開發者能驗證環境而不是猜設定。

**Requirements:** ENV-01, ENV-02, ENV-03, ENV-04

**Status:** Complete. Docker compose config, healthchecks, env cleanup, and ConfigTool validate/redacted print are implemented and verified.

**Artifacts:** `.planning/phases/09-environment-foundation/09-PHASE-SUMMARY.md`, `.planning/phases/09-environment-foundation/09-VERIFICATION.md`

**Success criteria:**
1. Docker compose config 可被腳本驗證，infra/app service 定義一致。
2. API / web compose service 有可用 health/readiness check。
3. `.env.example`、INFRA、config docs 區分 required/optional/secret，且不包含尚未落地的業務/auth secret。
4. ConfigTool 支援 `validate` 與 redacted effective config 類命令，不做 seed、不碰業務資料。

### Phase 10 — Execution Foundation

**Goal:** 建立非業務的背景任務與前端 API/E2E 整合標準路徑。

**Requirements:** EXEC-01, EXEC-02, EXEC-03, EXEC-04

**Status:** Partial. Frontend package-free API client foundation is implemented. Quartz and Playwright remain blocked by package availability.

**Artifacts:** `.planning/phases/10-execution-foundation/10-PHASE-SUMMARY.md`, `.planning/phases/10-execution-foundation/10-VERIFICATION.md`

**Success criteria:**
1. API host 有 Quartz registration / job abstraction，可註冊 test-only job。
2. Background job 使用 CorrelationId、TimeProvider、structured logging，並有 execution/failure tests。
3. Frontend 有 API client generation 或 typed client integration path，對齊 OpenAPI / ProblemDetails。
4. Playwright/test-auth harness 可跑 authenticated smoke，不需要真 login 或業務流程。

### Phase 11 — Dependency Source Foundation

**Goal:** 讓 NuGet / pnpm dependency resolution 可追溯、可重跑，不再把套件來源問題誤判成 build/test 問題。

**Requirements:** DEP-01, DEP-02, DEP-03

**Status:** Complete for foundation scope. Frontend package versions are pinned, repo-local pnpm config exists, and `pnpm-lock.yaml` is generated. Full package install still needs either a complete pnpm store or a reachable registry/mirror.

**Spec:** `docs/specs/F-011-dependency-source-foundation.md`

**Success criteria:**
1. Frontend build-critical package versions are exact.
2. Repo-local pnpm config prevents accidental global offline inheritance.
3. Lockfile exists and verification can distinguish source/cache/TLS failure from code failure.

### Phase 12 — Frontend Contract Foundation

**Goal:** 建立 OpenAPI snapshot 與 frontend TypeScript contract 的 deterministic path，避免業務頁面開始後手寫 client drift。

**Requirements:** CONTRACT-01, CONTRACT-02, CONTRACT-03

**Status:** Complete. OpenAPI export, generated contract, and drift check are implemented.

**Spec:** `docs/specs/F-012-frontend-contract-foundation.md`

**Success criteria:**
1. OpenAPI can be exported from local API host.
2. Frontend has a committed snapshot and generated TS contract.
3. Verification detects stale generated contract output.

### Phase 13 — Migration / DB Ops Foundation

**Goal:** 把 EF migration status、SQL script generation、DB update 從個人記憶變成 repo scripts，且 default verification 不 mutate DB。

**Requirements:** DBOPS-01, DBOPS-02, DBOPS-03

**Status:** Partial. Scripts and local dotnet tool manifest exist; idempotent SQL generation is verified. Live `db-status` / `db-update` against a configured database is intentionally not run by default verification.

**Spec:** `docs/specs/F-013-migration-ops-foundation.md`

**Success criteria:**
1. `dotnet-ef` is pinned in repo local tool manifest.
2. Migration status and idempotent SQL script generation use repo scripts.
3. Database update is explicit and not part of default verification.

## Phase Numbering

- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.
