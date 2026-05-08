# Phase 1: Frontend Login Demo - Context

**Gathered:** 2026-05-09
**Status:** Ready for planning
**Mode:** `--auto` (Claude selected recommended defaults; choices logged in DISCUSSION-LOG.md)

<domain>
## Phase Boundary

Deliver one full GSD/SDD round-trip on `et-omniverse-v2` (add-phase → discuss → plan → execute → verify → ship) that produces:

1. A manually operable Vue 3 frontend with `/login` form → `router.push('/welcome')` → `/welcome` placeholder page
2. A human-written `docs/specs/F-001-frontend-login-page.md` whose `status` field tracks the phase lifecycle
3. Real pre-commit governance hook firings on this phase's commits, including at least one exercise of the rationale-bypass mechanism
4. Walkthrough material the team can replay (PR + commit history + GSD artifacts; no slide deck)

**This phase is the dogfood vehicle, not a product feature.** The UI is intentionally trivial. Milestone v1.0 success = SDD/GSD toolchain runs end-to-end on this repo, observable by team.

**Frontend root (locked):** `src/frontend/ETOmniverse.Web/` — existing Vite/Vue skeleton (`vite.config.ts`, `package.json`, `src/main.ts`, `src/App.vue`). All Phase 1 code lives under here.

</domain>

<decisions>
## Implementation Decisions

### Routing & Page Structure

- **D-01:** Add `vue-router@^5` to `src/frontend/ETOmniverse.Web/package.json` via pnpm (`pnpm add vue-router`, resolves to `5.0.6` registry latest as of 2026-05-09). No other new runtime deps for this phase. Originally written as `@4`; corrected after RESEARCH.md Q-RES-001 verified `vue-router@4` is now on the `next` dist-tag (legacy-shim) while `latest` moved to 5.x. API surface used by this phase (`createRouter` / `createWebHistory` / `app.use` / `<router-view />` / `router.push`) is identical between 4.x and 5.x.
- **D-02:** Single router config file at `src/frontend/ETOmniverse.Web/src/router/index.ts` (NOT split per-module). The CONVENTIONS rule "router routes 按模組分檔" addresses multi-module future state — this phase has 2 routes total, splitting now is premature.
- **D-03:** Use `createWebHistory()` (HTML5 mode), not hash mode. Standard Vue 3 default; Vite dev server handles SPA fallback out of the box.
- **D-04:** Routes: `/login` → `Login.vue`, `/welcome` → `Welcome.vue`, `/` → redirect to `/login`. The redirect (vs. setting `/login` as default route at `/`) gives an explicit URL that matches the demo narrative.
- **D-05:** Page components at `src/frontend/ETOmniverse.Web/src/pages/Login.vue` and `src/pages/Welcome.vue` (per UI-SPEC `Notes for Downstream Consumers`).
- **D-06:** `src/main.ts` modified to register router via `app.use(router)`. `src/App.vue` reduced to a single `<router-view />` (current placeholder content removed — it conflicts with UI-SPEC's "no chrome" rule).

### SDD Spec (F-001)

- **D-07:** `docs/specs/F-001-frontend-login-page.md` written **by user (with main Claude as co-author) during plan-phase, before execute-phase begins**. Aligns with `docs/WORKFLOW.md` SDD step 5 ("write spec") and the rule "沒 spec 不寫 code".
- **D-08:** F-001 status flow: `draft` (plan-phase) → `approved` (before execute-phase starts) → `implementing` (during execute-phase) → `implemented` (after verify-work pass). Status field updated as part of the same commit chain.
- **D-09:** F-001 frontmatter required keys: `id: F-001`, `title`, `phase: 1`, `status`, `owner`, `created`, `updated`. Status transitions are commits, not silent edits.

### Governance Hook Validation (DEMO-03)

- **D-10:** Governance hook validation is **opportunistic, not staged**. Phase commits will naturally trip the doc-governance check; the executor reacts to real failures with real rationale (or real fix), not theatre.
- **D-11:** The rationale-bypass mechanism (`docs/no-doc-update-*.md`) **must be exercised at least once** in this phase, organically. Example trigger: a commit that changes `docs/specs/F-001` for a non-substantive reason where no other doc update is warranted — record rationale rather than fabricate doc churn.
- **D-12:** All commits in this phase route through `.githooks/pre-commit`. No `--no-verify` allowed at any point (would invalidate DEMO-03).

### Demo / Walkthrough Material (DOC-01)

- **D-13:** No separate slide deck or walkthrough doc. The walkthrough artifact set IS:
  - `.planning/phases/01-frontend-login-demo/` (entire directory — CONTEXT, PLAN, SUMMARY, etc.)
  - `docs/specs/F-001-frontend-login-page.md` (the spec)
  - Phase commit range on the feature branch (commit-by-commit log)
  - The PR itself
- **D-14:** A short pointer file `.planning/phases/01-frontend-login-demo/WALKTHROUGH.md` (≤ 1 page) is added at phase end listing where to look during the team session — section headings only, no narrative duplication. (Per PROJECT.md "Demo 形式 = PR + commit history + 口頭 walkthrough".)

### Build & Tooling

- **D-15:** Package manager: **pnpm** (matches REQUIREMENTS UI-03 `pnpm dev` and existing `package.json` script set). No npm / yarn lockfiles introduced.
- **D-16:** No additional Vite plugins, no Tailwind, no ESLint/Prettier config additions in this phase. UI-SPEC explicitly forbids these. Type checking via existing `vue-tsc -b && vite build` already in `package.json`.
- **D-17:** No tests written for this phase. REQUIREMENTS does not list a test deliverable; CONVENTIONS "沒測試不算完成" applies once feature specs declare acceptance — this phase's acceptance is manual UAT (UI-03) per design. Note this as an explicit dogfood exception, not a normative pattern.
- **D-18:** Fix the existing bug in `src/frontend/ETOmniverse.Web/vite.config.ts` where `plugins: [vue]` should be `plugins: [vue()]` (`@vitejs/plugin-vue` is a factory; bare reference is invalid — only works today because the placeholder `App.vue` is trivial enough that Vite's HMR still recovers). Phase 1 will exercise this code path (router + 2 SFCs + form bindings) and almost certainly surface the bug. Resolution: fix it as a Wave 1 prerequisite task. **The fix doubles as the D-11 organic rationale-bypass trigger** — a single-line `vite.config.ts` fix is `src/`-touching code with no corresponding doc/spec to update, which is exactly the legitimate trigger for `docs/no-doc-update-vite-plugin-init.md`. (Resolves RESEARCH.md Q-RES-002.)

### Claude's Discretion

- Exact filename casing of router file (`index.ts` vs `index.routes.ts`) — executor picks per existing repo style, default `index.ts`.
- Whether `App.vue` keeps a wrapper `<main>` element around `<router-view />` or is bare `<router-view />` — both acceptable; UI-SPEC says full-bleed pages, no nav chrome. Default to bare `<router-view />`.
- Inline `<style scoped>` per page vs. tiny `src/style.css` global with `body { font-family: system-ui; }` — UI-SPEC explicitly allows both. Default to per-page scoped, no global stylesheet.
- Whether to use `<script setup lang="ts">` (yes per CONVENTIONS) — locked, not at discretion.
- Submit handler signature (`(e: Event) => void` vs `(e: SubmitEvent) => void`) — executor picks; both valid.

### Folded Todos

None — `gsd-tools todo match-phase 1` returned 0 matches.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase scope & contract
- `.planning/PROJECT.md` — Milestone v1.0 scope, Out of Scope list, Key Decisions table (process-validation framing, demo-first, no slide deck)
- `.planning/REQUIREMENTS.md` — DEMO-01/02/03, UI-01/02/03, DOC-01 acceptance items + traceability table
- `.planning/ROADMAP.md` §"Phase 1: Frontend Login Demo" — 5 success criteria
- `.planning/phases/01-frontend-login-demo/01-UI-SPEC.md` — locked visual/interaction contract (already approved 2026-05-09)

### Repo-level conventions & decisions
- `docs/CONVENTIONS.md` §"寫 code（前端 / Vue）" — Composition API, `<script setup lang="ts">`, Pinia rule, router-per-module rule, OpenAPI codegen rule
- `docs/CONVENTIONS.md` §"Git / Commit" — branch naming `feat/<owner>/<short-name>`, squash merge, no Co-Authored-By
- `docs/WORKFLOW.md` §"SDD（Spec-Driven Development）" — spec-before-code rule
- `docs/WORKFLOW.md` §"SDD 流程詳細（GSD 對映）" — 9-step new-feature pipeline (this phase IS step 4-9)
- `docs/DECISIONS.md` §"規劃期決策" — D14 (no AD/LDAP this phase), D15 (demo-first scaffolding), D17 (MSSQL only, no Qdrant) — all reinforce Phase 1 backend-untouched stance
- `docs/AI-GUIDE.md` — red lines, stop-and-ask triggers, prompt templates (executor must obey)

### Codebase maps (already produced by `/gsd:map-codebase`)
- `.planning/codebase/STRUCTURE.md` §"Directory Layout" — confirms `src/frontend/ETOmniverse.Web/` as Vue root
- `.planning/codebase/STACK.md` §"Frontend Framework" + §"Build/Dev" — Vue 3.5, Vite 7, TS 5.9, pnpm
- `.planning/codebase/CONVENTIONS.md` §"TypeScript/Vue" — naming + import order

### Governance machinery
- `.githooks/pre-commit` — the hook DEMO-03 must exercise (do NOT modify in this phase)
- `scripts/check-doc-governance.py` — the rule engine (Rule 1/2/4 already validated per PROJECT.md)
- `docs/no-doc-update-_template.md` — template for the rationale-bypass file required by D-11
- Existing example: `docs/no-doc-update-wire-governance.md` — reference for tone/format

### SDD spec template
- `docs/specs/` — empty directory; F-001 will be the first occupant. Frontmatter conventions inferred from `docs/DOCUMENTATION.md` (read this before drafting F-001).

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets

- **Vite + Vue 3 skeleton** (`src/frontend/ETOmniverse.Web/`) — already builds. `pnpm dev` works today against the placeholder App.vue. Phase 1 only needs to ADD router + 2 pages, not bootstrap a project.
- **`package.json` scripts** — `dev`, `build`, `preview` already wired. No script changes needed.
- **`vite.config.ts`** — already has `@vitejs/plugin-vue`. Likely no changes; if SPA fallback for unknown routes is needed, single-line `appType: 'spa'` (default) suffices.
- **`tsconfig.json`** — strict mode + ES2022 target + bundler module resolution already set. Vue SFC TS works out of the box.

### Established Patterns

- **No router today** — `main.ts` does `createApp(App).mount('#app')` directly. Phase 1 inserts `app.use(router)` before mount.
- **Placeholder `App.vue`** — current content (`<h1>ET-Omniverse</h1><p>Project skeleton is ready.</p>` + scoped styles) is the FIRST thing to delete. UI-SPEC rules out chrome and brand marks.
- **No Pinia / no API client / no OpenAPI codegen yet** — CONVENTIONS rules apply but are not exercised in this phase (no auth state, no fetch calls).
- **No existing components/ directory** — UI-SPEC explicitly forbids creating reusable components in this phase. Two flat page files only.

### Integration Points

- **`src/main.ts`** — composition root for the Vue app. Will gain `import router from './router'` and `app.use(router)`.
- **`src/App.vue`** — top-level shell; reduces to `<router-view />` (or thin wrapper).
- **NEW: `src/router/index.ts`** — exports the configured router instance.
- **NEW: `src/pages/Login.vue`, `src/pages/Welcome.vue`** — the two route components.
- **NEW: `docs/specs/F-001-frontend-login-page.md`** — spec lives outside `src/`, in repo-level `docs/`.

### Constraints from existing repo

- `.githooks/pre-commit` will run on every Phase 1 commit. Plan tasks must assume the hook may flag commits that lack corresponding doc updates — this is expected behavior, not a planning failure (DEMO-03 explicitly wants this exercise).
- `docs/DOCUMENTATION.md` defines spec frontmatter conventions — F-001 author must read it BEFORE drafting (don't invent a frontmatter schema).
- Backend code in `src/backend/` is fully out of scope. Any plan task touching `*.cs` is wrong.

</code_context>

<specifics>
## Specific Ideas

- **Login page literal copy** (locked by UI-SPEC): h1 `Login`, labels `Username` / `Password`, button `Log in`, tab title `Login — ET-Omniverse`.
- **Welcome page literal copy** (locked by UI-SPEC): single word `Welcome`, tab title `Welcome — ET-Omniverse`.
- **Submit behavior** (locked by UI-SPEC): `event.preventDefault()` then `router.push('/welcome')`. No network, no localStorage, no auth state, no loading state.
- **Form submission key**: Enter key in either input must also submit (browser-default `<form>` behavior — do not capture Enter manually).
- **No `Welcome, {username}`** — UI-SPEC explicitly says no auth state, so no source for username.

</specifics>

<deferred>
## Deferred Ideas

None new from this discussion. Standing deferrals (already enumerated in REQUIREMENTS §"v2 Requirements" and PROJECT §"Out of Scope") remain in force:

- Real auth (BE-01/02/03), form validation (UI-04), error UI (UI-05), design tokens (UI-06), router guards (UI-07) → milestone v2+
- Backend API endpoints, JWT/session, AD/LDAP (D14), Fugo, Qdrant → Phase 2

### Reviewed Todos (not folded)

None — `todo match-phase 1` returned 0 matches.

</deferred>

---

*Phase: 01-frontend-login-demo*
*Context gathered: 2026-05-09*
*Mode: --auto (recommended defaults; see DISCUSSION-LOG.md for choice rationale)*
