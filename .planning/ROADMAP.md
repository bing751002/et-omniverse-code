# Roadmap: ET-Omniverse v2

## Overview

Milestone v1.0 — GSD/SDD Process Validation。目標不是交付軟體功能，而是讓整套 SDD + GSD 工具棧在 et-omniverse-v2 這個 repo 跑通一次完整循環，產物包含一個前端 login → welcome demo 頁、一份人寫的 SDD spec、以及可在 commit history 追溯的 governance 記錄，讓 team 能觀察重現整個流程。

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [ ] **Phase 1: Frontend Login Demo** - 跑完一輪完整 GSD 流程並產出 login → welcome 前端 demo，驗證 SDD spec、governance hook 與 GSD 工具棧可在此 repo 端到端運作

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
- [ ] 01-03-PLAN.md — Manual UAT checkpoint + F-001 status: implementing → implemented + WALKTHROUGH.md (UI-03, DEMO-01, DOC-01)
**UI hint**: yes

## Progress

**Execution Order:**
Phases execute in numeric order: 1

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Frontend Login Demo | 0/3 | Not started | - |
