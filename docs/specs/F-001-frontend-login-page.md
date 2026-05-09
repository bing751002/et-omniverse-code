---
id: F-001
title: Frontend login page (dogfood)
module:
status: implemented
owner: jimmyliao
created: 2026-05-09
updated: 2026-05-09
supersedes:
superseded-by:
related-adr: []
related-interview: []
phase: 1
---

# F-001：Frontend login page (dogfood)

## 業務背景

本 feature 是 milestone v1.0 (GSD/SDD Process Validation) 的 dogfood 載體，不是真實 auth 功能。實際目標是讓 team 在這個 repo 觀察一輪完整 SDD + GSD 流程跑得通：spec 由人撰寫、PLAN 由 GSD 產出、commit 走 governance hook、最終 status 隨 phase 進度流轉。UI 契約鎖定於 `.planning/phases/01-frontend-login-demo/01-UI-SPEC.md`，視覺刻意停在 Vue/CSS 預設層。對應 7-step 流程：與後端 7-step 排播流程無關，僅為流程示範。

## 用戶故事

1. As a team member observing the dogfood demo, I want to follow `/login` → submit → `/welcome` end-to-end after `pnpm dev`, so that the GSD/SDD pipeline becomes observable in this repo.
2. As an internal developer running this milestone, I want a human-written spec that I can point to during the walkthrough, so that the team can compare spec vs PLAN.md and understand what each artifact carries.

## 範圍

### In scope
- `/login` page with `Username` + `Password` form, submit triggers `router.push('/welcome')` (no validation, no auth state, no network)
- `/welcome` page rendering the literal word `Welcome`
- Minimal `vue-router` config (3 routes: `/` redirect → `/login`, `/login`, `/welcome`)
- `App.vue` reduced to a single `<router-view />`
- `main.ts` registering the router via `app.use(router)`
- Browser tab title set per UI-SPEC (`Login — ET-Omniverse` / `Welcome — ET-Omniverse`) via `document.title` in `onMounted`

### Out of scope
- Real authentication (BE-01/02/03 — Phase 2)
- Form validation (UI-04), error UI (UI-05), design tokens (UI-06), router guards (UI-07)
- Pinia store, localStorage fake auth, `<keep-alive>`, animations, dark mode, logo / brand mark
- Tailwind / UnoCSS / component libraries
- Tests (per CONTEXT.md D-17 — manual UAT is the acceptance for this dogfood phase)
- All items listed under `01-UI-SPEC.md` §"Out-of-Scope Reminders for Executor"

## 驗收條件

- [x] `pnpm dev` 起得來，`http://localhost:5173/login` 顯示包含 `Username` / `Password` label 與 `Log in` 按鈕的 form — 對應測試：manual UAT
- [x] 直接打開 `http://localhost:5173/` 自動 redirect 到 `/login` — 對應測試：manual UAT
- [x] 在 username 或 password input 填任意值，點 `Log in` 或按 Enter，URL 變 `/welcome` 且頁面顯示 `Welcome` — 對應測試：manual UAT
- [x] tab title 在 `/login` 為 `Login — ET-Omniverse`，在 `/welcome` 為 `Welcome — ET-Omniverse` — 對應測試：manual UAT
- [x] `pnpm build` 不出錯 (vue-tsc + vite build 通過) — 對應測試：build smoke

## 實作連結（完工後填）

> 路徑以 `<...>` 標記表示尚未存在，待 Plan 02/03 完工後改為實際 backtick 路徑（屆時 `scripts/check-spec-links.py` 會驗證真實存在）。

- Router config：`<src/frontend/ETOmniverse.Web/src/router/index.ts>` (Plan 02)
- Login page：`<src/frontend/ETOmniverse.Web/src/pages/Login.vue>` (Plan 02)
- Welcome page：`<src/frontend/ETOmniverse.Web/src/pages/Welcome.vue>` (Plan 02)
- App shell：`<src/frontend/ETOmniverse.Web/src/App.vue>` (Plan 02)
- Composition root：`<src/frontend/ETOmniverse.Web/src/main.ts>` (Plan 02)
- Vite config fix：`src/frontend/ETOmniverse.Web/vite.config.ts` (`[vue]` → `[vue()]`, Plan 01 Task 1.2)
- 主要 PR：#TBD

## Open questions

- [x] Q-F001-001: vue-router 鎖 5.x — Resolved 2026-05-09 by `pnpm add vue-router` (resolved to registry latest `^5.0.6`)

## 變更記錄

| 日期 | 變更 | PR |
|---|---|---|
| 2026-05-09 | 初版 (status: draft) | #TBD |
| 2026-05-09 | status: draft → approved (plan-phase 完，準備開工) | #TBD |
| 2026-05-09 | status: approved → implementing (execute-phase 開始實作) | #TBD |
| 2026-05-09 | status: implementing → implemented (manual UAT 通過、phase 完成) | #TBD |
