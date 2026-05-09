# ET-Omniverse v2

## What This Is

東森（EHSN）內部的 7-step 排播平台 v2。greenfield .NET modular monolith（.NET 10 / EF Core 10 / MSSQL / Quartz.NET）+ Vue 3 前端，鎖定節目部 / 編輯部 / 行銷部跨部門協作流程：建批次 → 商品挑選 → 受眾分眾 → AI VCR 生成 → 行銷物料 → 排播 → 通知 / 共編。

完整專案脈絡見 [`README.md`](../README.md)、[`docs/ARCHITECTURE.md`](../docs/ARCHITECTURE.md)、[`docs/DECISIONS.md`](../docs/DECISIONS.md)、[`.planning/codebase/`](codebase/)（剛產出的 codebase map）。

## Core Value

**用 SDD（spec-driven development）+ GSD 工具棧把 7-step 排播流程從紙本散亂搬到結構化、可追溯的數位系統**。一切契約（spec / ADR / 訪談）優先於實作，code 永遠對齊已 approved 的 spec。

## Requirements

### Validated

#### Milestone v1.0 — GSD/SDD Process Validation (Phase 01 complete 2026-05-09)

- [x] **DEMO-01**: 跑完一輪完整 GSD 流程（add-phase → discuss-phase → plan-phase → execute-phase → verify-work → ship）— Validated in Phase 01
- [x] **DEMO-02**: 產出對應的 `docs/specs/F-001-*.md`（人寫，跟 PLAN.md 並存，驗證 spec/PLAN 雙軌可行）— Validated in Phase 01
- [x] **DEMO-03**: pre-commit hook + governance script 在實際 commit 中運作（11/11 hook-clean，rationale-bypass 一次 organic 觸發）— Validated in Phase 01
- [x] **UI-01**: 前端 login 頁（form：username + password；submit 後 `router.push('/welcome')`）— Validated in Phase 01
- [x] **UI-02**: 前端 welcome placeholder 頁（顯示 "Welcome" 文字即可）— Validated in Phase 01
- [x] **UI-03**: `pnpm dev` 起得來、瀏覽器可手動操作完整 login → welcome 跳轉（manual UAT 2026-05-09 5/5 pass）— Validated in Phase 01
- [x] **DOC-01**: 給 team 的 walkthrough 素材（`.planning/phases/01-frontend-login-demo/WALKTHROUGH.md` 9-section pointer）— Validated in Phase 01

### Active

(None — milestone v1.0 唯一 phase 已驗收，等 `/gsd:complete-milestone` 收尾)

### Out of Scope

#### 此 milestone 排除

- 後端 API（任何 .NET endpoint） — Phase 2 才碰，此 milestone 純前端 dogfood
- 真實 auth（JWT / session / user store / RBAC） — D14 推到 Phase 2，此 milestone 連 demo 都不做假版
- Auth state 模擬（router guard / localStorage `fakeLoggedIn`） — 用最簡的 `router.push`，避免 demo 雜訊
- 視覺設計系統 / design tokens — 用 Vue/CSS 預設，不抽元件
- Form validation（必填 / email 格式） — 純 UI 跳轉，欄位內容無意義
- 跨 browser / 響應式 / a11y 嚴格驗證 — dogfood 不做，未來 phase 再補
- 部署（Docker compose / Jenkins build） — 本機跑得起來即可
- 多個 phase — 此 milestone 只跑 1 個 phase 一次完整循環

#### 整個 Phase 1 排除（從 README）

- Fugo 復購服務 — Phase 2
- AD/LDAP — Phase 2（D14）
- Qdrant / RAG — Phase 2
- Jenkins / Harbor / EFK 上線級監控 — P1.6 之後

## Context

**專案狀態**：剛 bootstrap，docs/ 完整、src/ 是 skeleton（無業務邏輯）、tests/ 是 placeholder（UnitTest1.cs）。

**已 dogfood 過的工具棧**：
- SDD 文件結構（`docs/specs/` `docs/decisions/` `docs/interviews/` `docs/patterns/` `docs/retrospectives/`）— 模板齊備，待第一份 spec 產生
- Documentation governance（`scripts/check-doc-governance.py` + `.githooks/pre-commit` + Jenkinsfile Docs Lint stage）— 已驗證 Rule 1/2/4 + rationale bypass 機制
- GSD 流程（`/gsd:map-codebase` 已跑、產出 `.planning/codebase/` 7 份文件）

**團隊規範**：
- 強制 Claude Code + GSD（見 `docs/AI-GUIDE.md`）
- 強制 SDD（spec → ADR → execute → verify）
- F-XXX:phase 1:1 預設對映
- spec status 流轉：draft → approved → implementing → implemented → modifying → deprecated

**GSD 版本鎖**：`.gsd-version` = 1.28.0（team 全員一致；升版由 lead 改檔 + 公告）

## Constraints

- **Tech stack（鎖死）**：.NET 10 + EF Core 10 + MSSQL + Quartz.NET（後端，此 milestone 不動）；Vue 3 + Vite + pnpm（前端，此 milestone 唯一動的部分）
- **Workflow（鎖死）**：所有開發必走 GSD（`/gsd:add-phase` → `/gsd:discuss-phase` → `/gsd:plan-phase` → `/gsd:execute-phase` → `/gsd:verify-work` → `/gsd:ship`）。Bug fix 走 `/gsd:debug`
- **SDD（鎖死）**：每個 phase 必有對應 `docs/specs/F-XXX-*.md`，由人（含主 Claude 共寫）撰寫，GSD 不自動產
- **Governance（執行中）**：commit 前 pre-commit hook 強制檢查；違規必更新對應 KB 或加 `docs/no-doc-update-*.md` rationale
- **Phase 1 邊界（D10 起）**：見 README + Out of Scope；看到 Phase 2 code 立刻 reject
- **Demo 非交付給終端使用者**：milestone v1.0 對象是 **team 內部**，不對外、不上 staging、不收集真實使用 metrics

## Key Decisions

歷史決策見 [`docs/DECISIONS.md`](../docs/DECISIONS.md) D10-D18。本專案層級新增決策追加到此表（同步 `docs/decisions/D-XX-*.md`）：

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Milestone v1.0 範圍收斂為「process validation」而非「Phase 1 上半 MVP」 | 跳過大範圍把 GSD 工具棧先在 repo 跑通，避免工具沒驗證就推大範圍工作 | — Pending |
| Login demo 用純 `router.push` 不做 auth state 模擬 | 此 milestone 重點是 GSD 流程不是 auth；fake state 只增 demo 雜訊 | — Pending |
| Demo 形式 = PR + commit history + 口頭 walkthrough，不做 slide deck | 工具棧本身就是 demo artifact；slide 重複又會跟 code 漂移 | — Pending |
| GSD 版本鎖採 `.gsd-version` + ONBOARDING 引用，不 vendor GSD 引擎進 repo | meta-tooling 投資延後；vendor 是 1-3 小時 + 持續維運成本，現階段非必要 | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd:transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd:complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-05-09 after Phase 01 verification passed*
