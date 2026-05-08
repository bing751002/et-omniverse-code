# Requirements: ET-Omniverse v2

**Defined:** 2026-05-08
**Core Value:** 用 SDD（spec-driven development）+ GSD 工具棧把 7-step 排播流程從紙本散亂搬到結構化、可追溯的數位系統

## v1 Requirements

Milestone **v1.0 — GSD/SDD Process Validation**。實際 deliverable 不是程式功能，而是**驗證並 demo 整套 SDD + GSD 工具棧在 et-omniverse-v2 跑得起來**，產物可讓 team 觀察重現。

### Process

- [ ] **DEMO-01**: 跑完一輪完整 GSD 流程（add-phase → discuss-phase → plan-phase → execute-phase → verify-work → ship），所有對應的 `.planning/phases/<phase>/` artifact 完整保留
- [x] **DEMO-02**: 產出對應的 `docs/specs/F-001-frontend-login-page.md`（人寫的 SDD 契約，frontmatter 完整、status 隨 phase 進度流轉）
- [x] **DEMO-03**: pre-commit hook + governance script 在實際 phase commits 中運作（不是空跑），rationale 機制有實際使用至少一次

### UI

- [ ] **UI-01**: 前端 login 頁含 username + password form，submit 後 `router.push('/welcome')`，無 form validation、無 auth state、無 router guard
- [ ] **UI-02**: 前端 welcome placeholder 頁（顯示 "Welcome" 文字即可，不做動態內容）
- [ ] **UI-03**: `pnpm dev` 起 frontend，瀏覽器手動操作可完成 login → welcome 跳轉路徑

### Documentation

- [ ] **DOC-01**: 給 team 的 walkthrough 素材（口頭 + screen share 可用）：spec vs PLAN 差異、governance 機制、commit history 對應 PLAN tasks 的清單

## v2 Requirements

未來 milestone 會加入的能力，留意但不在本 milestone roadmap：

### Backend

- **BE-01**: .NET API endpoint 真實 auth（POST /api/auth/login）
- **BE-02**: 真實 user store（local user 表 + RBAC，per D14）
- **BE-03**: JWT / session 管理

### UI Polish

- **UI-04**: Form validation（必填 / 帳密格式）
- **UI-05**: Error message UI（登入失敗顯示）
- **UI-06**: Design system / design tokens 抽出
- **UI-07**: Router guard（未登入訪問 protected route 自動跳 login）

### Infra

- **INFRA-01**: 部署上 staging（Docker compose / Jenkins）

## Out of Scope

明確排除，避免 scope creep：

| Feature | Reason |
|---------|--------|
| 後端 API（任何 .NET endpoint） | 此 milestone 純前端 dogfood，backend Phase 2 才碰 |
| 真實 auth（JWT / session / user store / RBAC） | D14 推到 Phase 2，此 milestone 連 demo 都不做假版 |
| Auth state 模擬（router guard / localStorage `fakeLoggedIn`） | 用最簡的 `router.push`，避免 demo 雜訊 |
| Form validation | 純 UI 跳轉，欄位內容無意義；demo 焦點是 GSD 流程不是 form 行為 |
| 視覺設計系統 / design tokens | 用 Vue/CSS 預設，不抽元件；未來 phase 真實 UI 才做 |
| 跨 browser / 響應式 / a11y 嚴格驗證 | dogfood 不做，未來 phase 再補 |
| 部署（Docker compose / Jenkins build） | 本機跑得起來即可 |
| 多個 phase | 此 milestone 只跑 1 個 phase 一次完整循環 |
| Fugo / AD / Qdrant / 上線級監控 | Phase 2 議題，與此 milestone 無關 |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| DEMO-01 | Phase 1 | Pending |
| DEMO-02 | Phase 1 | Complete |
| DEMO-03 | Phase 1 | Complete |
| UI-01 | Phase 1 | Pending |
| UI-02 | Phase 1 | Pending |
| UI-03 | Phase 1 | Pending |
| DOC-01 | Phase 1 | Pending |

**Coverage:**
- v1 requirements: 7 total
- Mapped to phases: 7
- Unmapped: 0

---
*Requirements defined: 2026-05-08*
*Last updated: 2026-05-08 after roadmap creation*
