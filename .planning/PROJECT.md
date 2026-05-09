# ET-Omniverse v2

## What This Is

東森（EHSN）內部的 7-step 排播平台 v2。greenfield .NET modular monolith（.NET 10 / EF Core 10 / MSSQL / Quartz.NET）+ Vue 3 前端，鎖定節目部 / 編輯部 / 行銷部跨部門協作流程：建批次 → 商品挑選 → 受眾分眾 → AI VCR 生成 → 行銷物料 → 排播 → 通知 / 共編。

完整專案脈絡見 [`README.md`](../README.md)、[`docs/ARCHITECTURE.md`](../docs/ARCHITECTURE.md)、[`docs/DECISIONS.md`](../docs/DECISIONS.md)、[`.planning/codebase/`](codebase/)。

## Core Value

**用 SDD（spec-driven development）+ GSD 工具棧把 7-step 排播流程從紙本散亂搬到結構化、可追溯的數位系統**。一切契約（spec / ADR / 訪談）優先於實作，code 永遠對齊已 approved 的 spec。

## Current State

**Shipped:** v1.0 — GSD/SDD Process Validation + Backend Foundation (2026-05-09)

- 7 phases, 32 plans, 110 commits — audit passed（56/56 ACs, 16/16 wirings, 5/5 E2E flows）
- F-001 ~ F-007 全部 status = `implemented`
- Backend foundation 就緒：logging / HTTP in/out / persistence / test-mode auth / testability
- Decisions logged: D-19, D-20, D-21, D-22

詳見 [`milestones/v1.0-ROADMAP.md`](milestones/v1.0-ROADMAP.md) 與 [`milestones/v1.0-REQUIREMENTS.md`](milestones/v1.0-REQUIREMENTS.md)。

## Next Milestone Goals (v1.1 — TBD)

v1.0 已建立完整 backend foundation；v1.1 應該是**第一個業務 feature**。下個 milestone 透過 `/gsd:new-milestone` 定義具體 scope，候選方向：

- 第一個業務 entity（CRUD + spec + Authorize endpoint，dogfood 完整 stack：F-002 logging + F-003 inbound + F-005 persistence + F-006 test auth）
- 真實 auth foundation（D-14：JWT / session / local user 表 / RBAC）
- 7-step 排播流程的第一步（建批次？商品挑選？— 待 product 訪談確認）

## Constraints

- **Tech stack（鎖死）**：.NET 10 + EF Core 10 + MSSQL + Quartz.NET（後端）；Vue 3 + Vite + pnpm（前端）
- **Workflow（鎖死）**：所有開發必走 GSD（`/gsd:add-phase` → `/gsd:discuss-phase` → `/gsd:plan-phase` → `/gsd:execute-phase` → `/gsd:verify-work` → `/gsd:ship`）。Bug fix 走 `/gsd:debug`
- **SDD（鎖死）**：每個 phase 必有對應 `docs/specs/F-XXX-*.md`，由人撰寫，GSD 不自動產
- **Governance（執行中）**：commit 前 pre-commit hook 強制檢查；違規必更新對應 KB 或加 `docs/no-doc-update-*.md` rationale
- **F-XXX:phase 1:1 預設對映**
- **spec status 流轉**：draft → approved → implementing → implemented → modifying → deprecated（D-08 4-step）

## Key Decisions

歷史決策見 [`docs/DECISIONS.md`](../docs/DECISIONS.md)。專案層級決策：

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Milestone v1.0 範圍從「process validation」擴張為「+ backend foundation」 | Phase 02 完成後使用者要求把 backend 共用基建一次納入，避免 v1.1 業務 phase 又在補基建 | ✅ Validated — 7 phases 全部蓋章，v1.1+ 業務 phase 開工就有完整 stack |
| GSD 版本鎖採 `.gsd-version` + ONBOARDING 引用，不 vendor GSD 引擎進 repo | meta-tooling 投資延後；vendor 是 1-3 小時 + 持續維運成本 | ✅ Validated — 全程 110 commits hook-clean，無工具版本爭議 |
| PHASE-SUMMARY / plan SUMMARY 為 disk-only（`.gitignore`），對外契約由 specs + ADR + commit history 承擔 | SDD purist：契約只 1 份（spec），summary 為內部 working artifact | ✅ Validated — v1.0 完整跑通，外部審計可從 specs + ADR + git log 重建脈絡 |

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
3. Update Current State + Next Milestone Goals

<details>
<summary>Previous milestone history (v1.0)</summary>

### Milestone v1.0 — GSD/SDD Process Validation + Backend Foundation (2026-05-08 → 2026-05-09)

原本只規劃 Phase 01 frontend login demo（process validation），Phase 02 完成後使用者擴張範圍把 backend foundation（logging / HTTP / persistence / test infra）全納入。最終 7 phases 全部蓋章，audit passed。

**Validated requirements:** DEMO-01/02/03, UI-01/02/03, DOC-01, F-002 (AC-1..11), F-003 (AC-1..9), F-004 (AC-1..8), F-005 (AC-1..10), F-006 (AC-1..10), F-007 (AC-A1..D3 / 22 ACs).

詳見 [`milestones/v1.0-ROADMAP.md`](milestones/v1.0-ROADMAP.md).

</details>

---
*Last updated: 2026-05-09 after v1.0 milestone complete*
