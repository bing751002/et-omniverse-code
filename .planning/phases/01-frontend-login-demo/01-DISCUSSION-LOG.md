# Phase 1: Frontend Login Demo - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in `01-CONTEXT.md` — this log preserves the alternatives considered.

**Date:** 2026-05-09
**Phase:** 01-frontend-login-demo
**Mode:** `--auto` — Claude selected the recommended option for each gray area without prompting the user. Each row's "Selected" mark reflects the auto-pick.
**Areas discussed:** Routing & Page Structure, SDD Spec (F-001) Lifecycle, Governance Hook Validation, Demo / Walkthrough Material, Build & Tooling

---

## Routing & Page Structure

### Q1.1 — Vue Router history mode

| Option | Description | Selected |
|--------|-------------|----------|
| `createWebHistory()` HTML5 | Standard SPA URLs (`/login`, `/welcome`); requires Vite dev fallback (default on) | ✓ |
| `createWebHashHistory()` | URLs with `#` fragment; works without server-side fallback | |
| `createMemoryHistory()` | In-memory, for SSR/tests | |

**Auto-pick rationale:** HTML5 mode is the Vue 3 default and works with Vite dev server out of the box. Hash mode is for legacy hosting. Memory mode is for tests.

### Q1.2 — Router file layout

| Option | Description | Selected |
|--------|-------------|----------|
| Single `src/router/index.ts` | One file, two routes | ✓ |
| Per-module split (`src/router/identity.routes.ts` + `index.ts`) | Matches CONVENTIONS rule for multi-module future state | |

**Auto-pick rationale:** CONVENTIONS rule "router routes 按模組分檔" exists to avoid `index.ts` git conflicts when many modules ship in parallel. Phase 1 has 2 routes total — splitting now is premature. The rule is honored in spirit (single-module phase = single file), not violated.

### Q1.3 — Entry route `/`

| Option | Description | Selected |
|--------|-------------|----------|
| `/` redirects to `/login` | Explicit redirect; URL becomes `/login` after load | ✓ |
| `/login` registered as default at `/` | Same component shown at both URLs | |
| 404 / no entry handling | Loading `/` shows nothing | |

**Auto-pick rationale:** Explicit `/` → `/login` redirect matches the demo narrative ("user opens app → sees login form"). UI-SPEC says "executor's choice" — the redirect URL is more honest about what's loaded.

### Q1.4 — Page file location

| Option | Description | Selected |
|--------|-------------|----------|
| `src/pages/Login.vue` + `src/pages/Welcome.vue` | Per UI-SPEC `Notes for Downstream Consumers` | ✓ |
| `src/views/Login.vue` + `src/views/Welcome.vue` | Vue 3 default scaffold convention | |

**Auto-pick rationale:** UI-SPEC explicitly names `src/pages/Login.vue` / `src/pages/Welcome.vue`. No reason to override the design contract.

---

## SDD Spec (F-001) Lifecycle

### Q2.1 — When to write F-001

| Option | Description | Selected |
|--------|-------------|----------|
| During plan-phase, before execute-phase | Spec drives planning; matches WORKFLOW.md SDD step 5 | ✓ |
| During execute-phase opening | Plan exists, spec catches up | |
| After execute-phase, before verify-work | Spec retrofitted | |

**Auto-pick rationale:** WORKFLOW.md and CONVENTIONS both encode "沒 spec 不寫 code" — spec must precede execute. Writing during plan-phase is the only option consistent with the rule.

### Q2.2 — F-001 status flow

| Option | Description | Selected |
|--------|-------------|----------|
| draft → approved → implementing → implemented | All four transitions, each a real commit | ✓ |
| draft → implemented (skip intermediate states) | Only first/last states tracked | |

**Auto-pick rationale:** Demonstrating spec status flow IS part of DEMO-02's deliverable. Skipping states defeats the demo.

### Q2.3 — Frontmatter source

| Option | Description | Selected |
|--------|-------------|----------|
| Read `docs/DOCUMENTATION.md` for spec frontmatter conventions, then draft | Convention-aligned | ✓ |
| Invent a minimal frontmatter set | Faster but creates schema drift risk | |

**Auto-pick rationale:** Repo already has documentation conventions. Inventing schema would force a rewrite later and undermine the SDD demo.

---

## Governance Hook Validation (DEMO-03)

### Q3.1 — How to trigger hook

| Option | Description | Selected |
|--------|-------------|----------|
| Opportunistic — let real commits trip the hook | Authentic exercise of governance | ✓ |
| Staged — craft a commit specifically to demo the hook | Reliable demo, less authentic | |

**Auto-pick rationale:** Authentic > theatrical for a process-validation milestone. If real phase commits don't trip the hook, that itself is a useful negative signal.

### Q3.2 — Rationale-bypass mechanism exercise

| Option | Description | Selected |
|--------|-------------|----------|
| Required — at least one `docs/no-doc-update-*.md` produced organically | Per DEMO-03 explicit "rationale 機制有實際使用至少一次" | ✓ |
| Optional — only if circumstances naturally call for it | Risk: DEMO-03 unmet | |

**Auto-pick rationale:** REQUIREMENTS DEMO-03 mandates rationale mechanism use. Opportunistic with active intent (executor watches for legitimate trigger) is the working compromise.

### Q3.3 — `--no-verify` policy

| Option | Description | Selected |
|--------|-------------|----------|
| Forbidden in this phase | Any bypass invalidates DEMO-03 | ✓ |
| Allowed if hook misbehaves | Faster but breaks the demo | |

**Auto-pick rationale:** DEMO-03 is the deliverable. Bypassing the hook erases the evidence.

---

## Demo / Walkthrough Material (DOC-01)

### Q4.1 — Walkthrough format

| Option | Description | Selected |
|--------|-------------|----------|
| PR + commit history + GSD artifacts (no slide deck) | Per PROJECT.md Key Decision | ✓ |
| Slide deck (NotebookLM-generated) | Familiar format but breaks the "tools as artifact" principle | |
| Recorded video walkthrough | Higher production cost, lower team replay-ability | |

**Auto-pick rationale:** PROJECT.md explicitly locks "Demo 形式 = PR + commit history + 口頭 walkthrough，不做 slide deck" with rationale "slide 重複又會跟 code 漂移".

### Q4.2 — Pointer file for walkthrough?

| Option | Description | Selected |
|--------|-------------|----------|
| One-page `.planning/phases/01-frontend-login-demo/WALKTHROUGH.md` listing artifact paths | Helps team find artifacts without narrative duplication | ✓ |
| No pointer — team navigates artifacts directly | Lower overhead but raises onboarding friction | |
| Full narrative document | Duplicates content from CONTEXT/PLAN/SUMMARY | |

**Auto-pick rationale:** Section-headings-only pointer is cheap, doesn't drift, helps team.

---

## Build & Tooling

### Q5.1 — Package manager

| Option | Description | Selected |
|--------|-------------|----------|
| pnpm | Per REQUIREMENTS UI-03 (`pnpm dev`) and existing scripts | ✓ |
| npm | Default but mismatched | |
| yarn | Mismatched | |

**Auto-pick rationale:** Locked by REQUIREMENTS.

### Q5.2 — New tooling additions

| Option | Description | Selected |
|--------|-------------|----------|
| None — only `vue-router` runtime dep | UI-SPEC forbids design system, validation, etc. | ✓ |
| Add Tailwind / ESLint / Prettier | Better DX but breaks UI-SPEC scope lock | |

**Auto-pick rationale:** UI-SPEC `Out-of-Scope Reminders for Executor` is unambiguous.

### Q5.3 — Test infrastructure

| Option | Description | Selected |
|--------|-------------|----------|
| No tests this phase; manual UAT only | REQUIREMENTS lists no test deliverable | ✓ |
| Add Vitest unit + Playwright E2E | Aligns with CONVENTIONS test discipline but out of REQUIREMENTS | |

**Auto-pick rationale:** REQUIREMENTS for milestone v1.0 has no test row. CONVENTIONS "沒測試不算完成" applies once feature specs declare testable acceptance — this phase's acceptance is manual UAT (UI-03). Document this as an explicit dogfood exception in CONTEXT so it doesn't become a normative pattern.

---

## Claude's Discretion

Items where executor has explicit latitude (not gray areas requiring user decision):

- Router file casing (`index.ts` vs `index.routes.ts`) — default `index.ts`
- `App.vue` shell shape (bare `<router-view />` vs thin wrapper) — default bare
- Per-page `<style scoped>` vs tiny global `style.css` — default per-page scoped
- Submit handler typing (`Event` vs `SubmitEvent`) — executor picks

## Deferred Ideas

None new. Standing deferrals from PROJECT.md / REQUIREMENTS.md remain in force (real auth, validation, error UI, design tokens, router guards, backend, AD/LDAP, Fugo, Qdrant — all out of milestone v1.0 scope).

---

*Auto-mode log generated 2026-05-09. Inputs: PROJECT.md, REQUIREMENTS.md, ROADMAP.md, 01-UI-SPEC.md, docs/CONVENTIONS.md, docs/WORKFLOW.md, docs/DECISIONS.md, .planning/codebase/* maps, current `src/frontend/ETOmniverse.Web/` state.*
