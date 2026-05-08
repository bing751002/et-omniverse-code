# Phase 1: Frontend Login Demo - Research

**Researched:** 2026-05-09
**Domain:** Vue 3 SPA bootstrap (router introduction) + SDD/governance dogfood mechanics
**Confidence:** HIGH

## Summary

Phase 1 is a dogfood vehicle: prove the GSD/SDD pipeline + governance hook end-to-end on this repo by shipping a trivial Vue 3 login → welcome navigation. All locked decisions in CONTEXT.md (D-01..D-17) drive a small, prescriptive surface — add `vue-router`, two pages, replace `App.vue` body with `<router-view />`, write `docs/specs/F-001-frontend-login-page.md` by hand, drive the `pre-commit` hook with real commits, and exercise `docs/no-doc-update-*.md` rationale at least once. There is no novel technical question to answer; the research goal is to anchor every planning task to a verified file, line, or rule so the planner cannot drift.

The five non-obvious findings the planner must absorb:

1. **`vue-router@4` is stale on the registry as of May 2026.** `npm view vue-router version` returns `5.0.6` as `latest`; `4.x` is published under the `next` dist-tag (legacy). This contradicts CONTEXT.md D-01 (`vue-router@4`). The Vue 3 line moved to vue-router 5.x. API is API-compatible (`createRouter` + `createWebHistory` + `app.use(router)` unchanged). **Planner must surface this conflict to the user before locking the install command** — do not silently substitute, do not silently obey D-01. See §"Open Questions" Q-RES-001.
2. **`vite.config.ts` has a real bug today** — `plugins: [vue]` instead of `plugins: [vue()]`. `@vitejs/plugin-vue` exports a factory; passing the bare reference is invalid. The current placeholder app likely renders only because `App.vue` is so trivial Vite's HMR pipeline still recovers, but Phase 1 (which adds router + 2 SFCs + form bindings) WILL surface this. Planner must either fix it as part of execute-phase or treat it as a Wave 0 prerequisite.
3. **Vite 7's default SPA fallback works for HTML5 history mode in `vite dev`** — no explicit config needed. Production deploy is out of scope for this milestone (PROJECT.md), so production fallback is a non-issue.
4. **The `pre-commit` hook will trigger naturally on this phase, not require staging.** `scripts/check-doc-governance.py` Rule 1 (`source-code-requires-spec-or-kb`) fires whenever `src/` or `tests/` is touched — every Vue file commit triggers it. The legitimate satisfier is `.planning/` artifacts (the GSD phase folder ALREADY counts) OR `docs/specs/F-001-*.md`. So commits that bundle code + the F-001 spec OR `.planning/` updates pass cleanly; commits that touch only code AND don't update `.planning/` need rationale. This gives D-11 a real, organic trigger without theatre.
5. **`docs/specs/_template.md` already pins frontmatter shape** but uses `module:` not the CONTEXT.md D-09 list. `docs/specs/README.md` says `module:` may be left blank ("模組劃分未定"). F-001 must follow the template; D-09's required keys are a SUBSET of the template, not a replacement. Planner: write F-001 frontmatter using the template's full key set, not D-09's reduced list.

**Primary recommendation:** Plan executes as 4-5 small tasks (router install → router config → page components → main.ts/App.vue rewire → F-001 spec lifecycle commits), each scoped to be a single commit that naturally satisfies governance Rule 1 by bundling `.planning/` updates or F-001 status edits. Reserve one task to deliberately produce a code-only commit so the rationale-bypass mechanism (D-11) gets exercised authentically.

## User Constraints (from CONTEXT.md)

### Locked Decisions

**Routing & Page Structure**

- **D-01:** Add `vue-router@4` to `src/frontend/ETOmniverse.Web/package.json` via pnpm. No other new runtime deps for this phase. **⚠️ See §"Open Questions" Q-RES-001 — registry latest is 5.0.6, not 4.x.**
- **D-02:** Single router config file at `src/frontend/ETOmniverse.Web/src/router/index.ts` (NOT split per-module). The CONVENTIONS rule "router routes 按模組分檔" addresses multi-module future state — this phase has 2 routes total, splitting now is premature.
- **D-03:** Use `createWebHistory()` (HTML5 mode), not hash mode. Standard Vue 3 default; Vite dev server handles SPA fallback out of the box.
- **D-04:** Routes: `/login` → `Login.vue`, `/welcome` → `Welcome.vue`, `/` → redirect to `/login`. The redirect (vs. setting `/login` as default route at `/`) gives an explicit URL that matches the demo narrative.
- **D-05:** Page components at `src/frontend/ETOmniverse.Web/src/pages/Login.vue` and `src/pages/Welcome.vue`.
- **D-06:** `src/main.ts` modified to register router via `app.use(router)`. `src/App.vue` reduced to a single `<router-view />` (current placeholder content removed).

**SDD Spec (F-001)**

- **D-07:** `docs/specs/F-001-frontend-login-page.md` written **by user (with main Claude as co-author) during plan-phase, before execute-phase begins**.
- **D-08:** F-001 status flow: `draft` (plan-phase) → `approved` (before execute-phase starts) → `implementing` (during execute-phase) → `implemented` (after verify-work pass). Status transitions are commits, not silent edits.
- **D-09:** F-001 frontmatter required keys: `id: F-001`, `title`, `phase: 1`, `status`, `owner`, `created`, `updated`. (Note: actual template `_template.md` has additional keys — `module`, `supersedes`, `superseded-by`, `related-adr`, `related-interview` — these may be left blank but should be present for template parity.)

**Governance Hook Validation**

- **D-10:** Governance hook validation is **opportunistic, not staged**. Phase commits will naturally trip the doc-governance check.
- **D-11:** The rationale-bypass mechanism (`docs/no-doc-update-*.md`) **must be exercised at least once** in this phase, organically.
- **D-12:** All commits route through `.githooks/pre-commit`. No `--no-verify` allowed.

**Demo / Walkthrough Material**

- **D-13:** No separate slide deck. Walkthrough artifact set IS: `.planning/phases/01-frontend-login-demo/`, `docs/specs/F-001-frontend-login-page.md`, phase commit range, the PR.
- **D-14:** Short pointer `.planning/phases/01-frontend-login-demo/WALKTHROUGH.md` (≤ 1 page) at phase end, section headings only.

**Build & Tooling**

- **D-15:** Package manager: **pnpm**. No npm / yarn lockfiles.
- **D-16:** No additional Vite plugins, no Tailwind, no ESLint/Prettier in this phase.
- **D-17:** No tests written this phase. Manual UAT (UI-03) is acceptance.

### Claude's Discretion

- Exact filename casing of router file (`index.ts` vs `index.routes.ts`) — default `index.ts`.
- Whether `App.vue` keeps a wrapper `<main>` element around `<router-view />` or is bare `<router-view />` — default bare.
- Inline `<style scoped>` per page vs. tiny `src/style.css` global — default per-page scoped, no global.
- Submit handler signature (`(e: Event) => void` vs `(e: SubmitEvent) => void`) — both valid.

### Deferred Ideas (OUT OF SCOPE)

Standing deferrals from REQUIREMENTS §"v2 Requirements" and PROJECT §"Out of Scope":

- Real auth (BE-01/02/03), form validation (UI-04), error UI (UI-05), design tokens (UI-06), router guards (UI-07) → milestone v2+
- Backend API endpoints, JWT/session, AD/LDAP (D14), Fugo, Qdrant → Phase 2
- Tailwind / UnoCSS / component libraries / Pinia / localStorage fake auth / `beforeEach` guards / loading states / animations / logo / brand mark — explicitly forbidden by UI-SPEC.md "Out-of-Scope Reminders for Executor"

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| DEMO-01 | 跑完一輪完整 GSD 流程 (add-phase → discuss → plan → execute → verify → ship), all `.planning/phases/<phase>/` artifacts retained | Already validated up through context — execute / verify / ship gates are GSD-tool concerns; planner just needs to ensure tasks emit the artifacts in standard locations |
| DEMO-02 | Produce `docs/specs/F-001-frontend-login-page.md` (human-written, frontmatter complete, status flows with phase) | §"F-001 Spec Authoring" — template at `docs/specs/_template.md`, frontmatter in `docs/specs/README.md`, status states in `docs/WORKFLOW.md` §"Spec status 流轉" |
| DEMO-03 | pre-commit hook + governance script run on real commits (not dry-run); rationale mechanism used at least once | §"Governance Hook Behavior" — Rule 1/2/4 sources mapped to file change patterns; legitimate organic triggers identified |
| UI-01 | Frontend login page with username + password form; submit calls `router.push('/welcome')`; no validation, no auth state, no guard | §"vue-router Integration" + §"Architecture Patterns" — minimal `<form>` with `@submit.prevent`; UI-SPEC.md Copywriting + Interaction tables already pin all literals |
| UI-02 | Frontend welcome placeholder page (literal "Welcome") | §"Architecture Patterns" — single SFC, h1 only |
| UI-03 | `pnpm dev` starts frontend; manual browser interaction completes login → welcome | §"Build & Tooling" — current `package.json` `dev` script + Vite 7 SPA fallback handle this; no config change required (modulo the `vite.config.ts` bug, see §"Common Pitfalls") |
| DOC-01 | Walkthrough material for team (verbal + screen share): spec vs PLAN diff, governance, commit history → PLAN tasks | D-13/D-14 — artifact set is the GSD output itself; planner adds short `WALKTHROUGH.md` pointer |

## Project Constraints (from CLAUDE.md / docs/AI-GUIDE.md)

These are hard directives the planner must respect:

| Source | Directive |
|--------|-----------|
| `docs/AI-GUIDE.md` 紅線 | Phase 1 must not introduce Fugo / AD code. No secrets in code. No Domain referencing EF/ASP.NET. **In Phase 1 these are non-issues — backend is untouched — but still binding if planner drifts.** |
| `docs/AI-GUIDE.md` Stop & Ask | "要加新 NuGet / pnpm 套件" → adding `vue-router` IS a stop-and-ask trigger. CONTEXT.md D-01 already resolved the ask, so planner is cleared, BUT the version conflict (5.0.6 vs 4.x) is a NEW stop-and-ask. |
| `docs/CONVENTIONS.md` 寫 code 通用 | "不寫 stub / 半完成 — 不會做就標 placeholder + 連結對應 open item." Welcome page IS a placeholder by design — fine, but spec must declare it as such. |
| `docs/CONVENTIONS.md` Git/Commit | Branch `feat/<owner>/<short-name>`. Squash merge. Commit message imperative + short. **No Co-Authored-By footer.** |
| `docs/CONVENTIONS.md` 寫 code 前端 | `<script setup lang="ts">`. Pinia for state (N/A this phase). Router routes split per module (this phase exempts — single config per D-02). API client from OpenAPI codegen (N/A this phase, no API calls). |
| `docs/WORKFLOW.md` SDD | "沒 spec 不寫 code." F-001 must be `approved` before execute-phase begins (D-08). |
| `docs/WORKFLOW.md` PR description 必填 | The eventual PR (ship phase) needs all required sections; planner should ensure SUMMARY.md captures the inputs (spec id, modify type, affected scope, acceptance, breaking changes). |
| `~/.claude/CLAUDE.md` plan-phase 自問 | (Already satisfied at the orchestrator level by the structured prompt that spawned this researcher.) |
| `~/.claude/CLAUDE.md` Persona 紀律 | Subagent is OK to be narrow; main session stays generalist. (Researcher = subagent, fine.) |
| `~/.claude/CLAUDE.md` Git push/merge | "Push / merge 前確認目標 remote branch — 不確定列出讓使用者選." Applies at ship-phase. |

## Standard Stack

### Core (already installed, no changes)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| vue | ^3.5.0 | Component framework | Locked Vue 3 in STACK.md |
| @vitejs/plugin-vue | ^6.0.0 | Vue SFC compiler plugin for Vite | Standard Vite + Vue 3 setup |
| vite | ^7.0.0 | Bundler + dev server | Locked, STACK.md |
| typescript | ^5.9.0 | Type system | Locked, tsconfig strict mode on |
| vue-tsc | ^3.0.0 | Vue-aware TS type checker | Used by `pnpm build` |

### To Add (one runtime dep)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| vue-router | **see Q-RES-001** (5.x stable / 4.x as D-01 currently states) | Client-side routing for `/login` ↔ `/welcome` | Official Vue Router; only credible option for Vue 3 SPA |

**Verified versions (npm registry, 2026-05-09):**
```
$ pnpm view vue-router dist-tags
{
  next:   '4.0.13',  ← CONTEXT.md D-01 references this line
  legacy: '3.6.5',   ← Vue 2 era
  edge:   '4.4.0-alpha.3',
  beta:   '5.0.0-beta.2',
  latest: '5.0.6'    ← current Vue 3 mainline
}
```

vue-router peerDependency on `vue: ^3.5.0` for both 4.x and 5.x lines. API surface used by Phase 1 (`createRouter`, `createWebHistory`, `routes` array, `app.use(router)`, `router.push`, `<router-view />`) is identical in 4.x and 5.x — migration cost is zero. The only practical difference is the `latest` tag pointer.

**Install command (deferred to user choice via Q-RES-001):**
```bash
cd src/frontend/ETOmniverse.Web
pnpm add vue-router            # → installs 5.0.6 (registry latest)
# OR, if obeying D-01 strictly:
pnpm add vue-router@^4         # → installs 4.5.x line
```

CONVENTIONS.md says nothing about caret vs exact pin policy for pnpm deps. Existing `package.json` uses `^x.y.0` style for everything. Recommend caret (`^`) to match house style.

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| vue-router | unplugin-vue-router (file-based) | Adds tooling, contradicts D-16 "no additional Vite plugins" |
| vue-router | hand-rolled `if (location.pathname === ...)` | Not standard, bypasses HTML5 history hooks Vue Router provides; would fail "is GSD producing real-shape code?" smell test |

### Lockfile Policy

STACK.md says `pnpm-lock.yaml` is "Not committed". Verified — no `pnpm-lock.yaml` exists at `src/frontend/ETOmniverse.Web/pnpm-lock.yaml` today. **No project-level statement (README, docs/INFRA, docs/CONVENTIONS) explicitly says `pnpm-lock.yaml` MUST stay uncommitted** — the STACK.md note is observational. Recommendation: keep current pattern (don't commit lockfile) for Phase 1. Lockfile policy is a separate ADR-worthy decision; do not bundle it into this milestone.

## Architecture Patterns

### Recommended Project Structure (additions only)

```
src/frontend/ETOmniverse.Web/src/
├── main.ts                      # MODIFIED: import router, app.use(router)
├── App.vue                      # MODIFIED: body becomes <router-view />, scoped styles deleted
├── router/
│   └── index.ts                 # NEW: createRouter + 3 routes (/, /login, /welcome)
└── pages/
    ├── Login.vue                # NEW: per UI-SPEC contract
    └── Welcome.vue              # NEW: per UI-SPEC contract
```

No `components/`, no `stores/`, no `api/`, no `composables/`, no `types/` folder additions this phase.

### Pattern 1: Minimal vue-router setup (Vue 3)

**What:** One router config file, three routes (one redirect, two component routes), HTML5 history.
**When to use:** Default for Vue 3 SPA with < 5 routes and no module split yet.
**Example** (Source: <https://router.vuejs.org/guide/>, verified 2026-05-09):

```ts
// src/router/index.ts
import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import Login from '../pages/Login.vue'
import Welcome from '../pages/Welcome.vue'

const routes: RouteRecordRaw[] = [
  { path: '/', redirect: '/login' },
  { path: '/login', name: 'login', component: Login },
  { path: '/welcome', name: 'welcome', component: Welcome },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router
```

```ts
// src/main.ts
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

createApp(App).use(router).mount('#app')
```

```vue
<!-- src/App.vue -->
<template>
  <router-view />
</template>
```

`type RouteRecordRaw` is a `type-only` import — TypeScript strict mode + `verbatimModuleSyntax` if ever turned on will require the `type` keyword. Current `tsconfig.json` does not set `verbatimModuleSyntax`, so plain `import { RouteRecordRaw }` would also work, but the `type` keyword is cheap insurance and matches CONVENTIONS "Type imports (separate from runtime imports)".

### Pattern 2: Submit-without-network in Login.vue

**What:** Native `<form>` with `@submit.prevent` calling `router.push`. No state ref for "isSubmitting", no fetch, no validation.
**When to use:** Dogfood placeholder where the form's job is navigation only.
**Example** (no Context7 source needed — this is just Vue 3 + vue-router idiom):

```vue
<!-- src/pages/Login.vue -->
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const username = ref('')
const password = ref('')

function onSubmit() {
  router.push('/welcome')
}
</script>

<template>
  <h1>Login</h1>
  <form @submit.prevent="onSubmit">
    <label>
      Username
      <input v-model="username" type="text" name="username" autocomplete="username" />
    </label>
    <label>
      Password
      <input v-model="password" type="password" name="password" autocomplete="current-password" />
    </label>
    <button type="submit">Log in</button>
  </form>
</template>
```

Notes:
- `@submit.prevent` covers BOTH click on `<button type="submit">` AND Enter-in-input (browser default). UI-SPEC's "Form submission key: Enter key in either input must also submit" is satisfied automatically — do NOT add manual `@keyup.enter` handlers.
- `username` / `password` refs are kept even though they're never read further — they are the v-model targets; removing them would mean removing v-model, which removes the realistic-form smell. UI-SPEC says "username + password form", not "two empty inputs".
- `autocomplete` attributes silence browser warnings (a11y / password-manager hint). Optional but recommended; UI-SPEC does not forbid.
- Browser tab title `Login — ET-Omniverse` (UI-SPEC Copywriting Contract): set with `useHead` from `@unhead/vue` would be over-engineering for this phase. Use plain `document.title = 'Login — ET-Omniverse'` inside `onMounted`, OR change `index.html` `<title>` once and accept that both pages share the title (slightly violates UI-SPEC but defensible — flag for executor discretion). The cheapest UI-SPEC-compliant path: add `onMounted(() => { document.title = 'Login — ET-Omniverse' })` in each page. Cheap and explicit.

### Pattern 3: Welcome.vue (placeholder)

```vue
<!-- src/pages/Welcome.vue -->
<script setup lang="ts">
import { onMounted } from 'vue'
onMounted(() => { document.title = 'Welcome — ET-Omniverse' })
</script>

<template>
  <h1>Welcome</h1>
</template>
```

### Anti-Patterns to Avoid

- **Adding `<input required>`** — UI-SPEC Out-of-Scope Reminders explicitly forbids `<input required>` and any FluentValidation-style rule.
- **Adding a logo / `<header>` / `<footer>`** — UI-SPEC explicitly forbids.
- **Adding a Pinia `userStore`** — D-17 / UI-SPEC forbid; auth state has no source in this phase.
- **`localStorage.setItem('fakeLoggedIn', '1')` after login** — explicitly forbidden by UI-SPEC.
- **Manual `@keyup.enter` on inputs** — `<form>` + `@submit` handles this natively.
- **`useHead` / `unhead` for title** — would require a new dep; D-01 forbids.
- **`<router-link to="/welcome">` instead of programmatic `router.push`** — UI-SPEC Interaction Contract specifies `event.preventDefault()` + `router.push('/welcome')` on submit; a link would skip the form altogether.
- **Splitting router into per-module files** — D-02 explicitly defers this until module count justifies it.
- **Setting `appType: 'spa'` in vite.config.ts** — Vite 7's default appType IS `'spa'`. Explicit is no-op noise; do not add.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Client-side routing | Custom hashchange / history popstate listener | `vue-router` | Vue 3 mainline answer; UI-SPEC + CONTEXT both require it |
| HTML5 history fallback in dev | Manual middleware in vite.config.ts | Vite 7 default | `appType: 'spa'` is the default; SPA fallback automatic |
| Form Enter-to-submit | `@keyup.enter` handler | `<form @submit.prevent>` | Browser default already does it |
| Browser tab title | Vue meta plugin | `document.title = '...'` in `onMounted` | Two pages, two literal strings — a meta plugin is overkill |
| Auth state | localStorage fake / Pinia store | **Nothing — explicitly out of scope** | Phase 1 does not have auth |

**Key insight:** This phase's defining feature is restraint. The temptation list (Tailwind, validation, store, guards, fake auth) is a TRAP — the milestone IS the discipline of NOT adding those things. UI-SPEC §"Out-of-Scope Reminders" is the canonical anti-temptation list; planner should re-read it once per task.

## Common Pitfalls

### Pitfall 1: `vite.config.ts` plugin invocation bug

**What goes wrong:** Current file:
```ts
plugins: [vue],   // ← BUG: should be vue()
```
`@vitejs/plugin-vue` exports a factory function. `[vue]` passes the function itself as a plugin object, which Vite either ignores silently or errors on, depending on version. With Vite 7 and a non-trivial Vue app (router + multiple SFCs + reactivity), this WILL surface.

**Why it happens:** Likely a typo in the original skeleton bootstrap; the placeholder `App.vue` is so simple Vite's loader cascade still serves it raw.

**How to avoid:** Fix as part of Phase 1 — change to `plugins: [vue()]`. This counts as a real code change to `src/frontend/.../vite.config.ts`, BUT it lives outside the Rule 1 trigger (Rule 1 covers `src/` and `tests/`; `vite.config.ts` IS under `src/frontend/...` so it DOES trigger Rule 1). Bundle the fix into the same commit as the router-add task so `.planning/phases/01-frontend-login-demo/` updates satisfy Rule 1 naturally.

**Warning signs:** `pnpm dev` errors out with "vue is not a function" / SFC compilation skipped / `<router-view />` renders as raw text instead of the routed component.

### Pitfall 2: Treating CONTEXT D-01 (`vue-router@4`) as gospel

**What goes wrong:** Researcher / planner / executor pins to `^4` even though `latest` is `5.0.6`. Repo ships with stale dep before its first prod commit.

**Why it happens:** D-01 was written before npm registry verification. Auto-mode discussion accepted Claude's recommendation; Claude's training data was stale.

**How to avoid:** Open Q-RES-001. Surface to user during plan-check or before executor runs `pnpm add`. Default recommendation: `pnpm add vue-router` (resolves to 5.0.6) since API is identical and 5.x is the maintained line. **Do not silently rewrite D-01** — that violates the "discuss decisions are locked" contract.

**Warning signs:** Planner emits a task that says `pnpm add vue-router@^4` without comment; reviewers on the eventual PR ask "why pin to a non-latest line on a greenfield repo?"

### Pitfall 3: `pre-commit` hook failures interpreted as "GSD broken"

**What goes wrong:** Executor commits `src/frontend/.../router/index.ts` without also touching `.planning/` or `docs/specs/F-001-*.md`. Hook fails with Rule 1. Executor panics, runs `--no-verify`, breaks D-12.

**Why it happens:** Rule 1 (`source-code-requires-spec-or-kb`) requires every `src/` change to be matched by ANY of: `.planning/`, `docs/specs/`, `docs/patterns/`, or several KB docs. Executors not familiar with the rule see the failure as a tooling bug.

**How to avoid:** Plan tasks so each commit naturally bundles `.planning/phases/01-frontend-login-demo/` updates (PLAN-progress notes, EXECUTION-LOG entries, etc.) OR `docs/specs/F-001-*.md` status edits. The rationale-bypass mechanism (`docs/no-doc-update-*.md`) is the LAST resort, used at most once per phase per D-11.

**Warning signs:** A task description that reads "commit Login.vue" with no `.planning/` artifact mentioned. A commit message that doesn't reference a F-id or PLAN row.

### Pitfall 4: Confusing Rule 2 (infra-requires-infra-doc) trigger

**What goes wrong:** Executor edits `.githooks/pre-commit` or `scripts/*.py` to "improve" governance during this phase, triggering Rule 2 which requires `docs/INFRA.md` / `docs/WORKFLOW.md` / `docs/DOCUMENTATION.md` updates.

**Why it happens:** Mission creep — "while we're here, let's tighten the hook."

**How to avoid:** D-12 says no `--no-verify`; CONTEXT canonical_refs say "do NOT modify [.githooks/pre-commit] in this phase." Phase 1 USES the hook, does not modify it.

**Warning signs:** Diff shows changes to `.githooks/`, `scripts/check-doc-governance.py`, `ci/`, or `docker/`.

### Pitfall 5: `npm` not on PATH on this work PC

**What goes wrong:** Executor runs `npm install vue-router` (out of habit) and gets `'npm' 不是內部或外部命令`. Or scripts that invoke `pnpm` internally break trying to spawn `npm`.

**Why it happens:** Verified during research (2026-05-09): `pnpm view vue-router version` failed with `spawnSync npm ENOENT` — pnpm itself shells out to `npm` internally for some operations and fails. Workaround: use `pnpm view` queries via a path that doesn't require `npm`, or do registry lookups via WebFetch.

**How to avoid:** Plan tasks use `pnpm add` / `pnpm dev` / `pnpm build` exclusively. If a task description says "run `npm ...`", planner caught a bug. For ad-hoc registry queries during execute-phase, prefer WebFetch over `pnpm view` to avoid the spawn issue.

**Warning signs:** Any task action containing `npm install`, `npm run`, or `npx` (D-15 forbids npm anyway, but be vigilant).

### Pitfall 6: Two pages, two `document.title` lines — easy to miss the unmount case

**What goes wrong:** User navigates `/login` → `/welcome`, title changes to `Welcome — ET-Omniverse`. User clicks browser back. Vue Router routes back to `Login.vue`, but if title was set in `onMounted` and the component is cached / not re-mounted, title may stay as `Welcome — ...`.

**Why it happens:** Default vue-router 5.x does NOT cache route components — each navigation re-mounts. So `onMounted` re-fires. **Likely a non-issue,** but if executor adds `<keep-alive>` for fun (UI-SPEC forbids unnecessary additions, so this should not happen), the bug surfaces.

**How to avoid:** Don't add `<keep-alive>`. Or use `onActivated` if `<keep-alive>` is genuinely needed (Phase 2+).

**Warning signs:** `<keep-alive>` appears in `App.vue`. Tab title doesn't update on back-navigation during UAT.

## Code Examples

### Example 1: Full `src/router/index.ts`

```ts
import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import Login from '../pages/Login.vue'
import Welcome from '../pages/Welcome.vue'

const routes: RouteRecordRaw[] = [
  { path: '/', redirect: '/login' },
  { path: '/login', name: 'login', component: Login },
  { path: '/welcome', name: 'welcome', component: Welcome },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router
```

Source: <https://router.vuejs.org/guide/> (verified 2026-05-09)

### Example 2: Full `src/main.ts` (after edit)

```ts
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

createApp(App).use(router).mount('#app')
```

### Example 3: Full `src/App.vue` (after edit)

```vue
<template>
  <router-view />
</template>
```

(No `<script>`, no `<style>`. Existing scoped styles in current `App.vue` are deleted per D-06.)

### Example 4: Full `src/pages/Login.vue` (per UI-SPEC + CONVENTIONS)

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const username = ref('')
const password = ref('')

onMounted(() => {
  document.title = 'Login — ET-Omniverse'
})

function onSubmit() {
  router.push('/welcome')
}
</script>

<template>
  <h1>Login</h1>
  <form @submit.prevent="onSubmit">
    <label>
      Username
      <input v-model="username" type="text" name="username" autocomplete="username" />
    </label>
    <label>
      Password
      <input v-model="password" type="password" name="password" autocomplete="current-password" />
    </label>
    <button type="submit">Log in</button>
  </form>
</template>

<style scoped>
form {
  display: grid;
  gap: 8px;
  max-width: 320px;
}
label {
  display: grid;
  gap: 4px;
}
</style>
```

Style block is the minimum to make labels stack vertically. Optional per UI-SPEC; remove if truly want defaults-only.

### Example 5: Full `src/pages/Welcome.vue`

```vue
<script setup lang="ts">
import { onMounted } from 'vue'

onMounted(() => {
  document.title = 'Welcome — ET-Omniverse'
})
</script>

<template>
  <h1>Welcome</h1>
</template>
```

### Example 6: F-001 spec skeleton (frontmatter follows `_template.md`)

```markdown
---
id: F-001
title: Frontend login page (dogfood)
module:
status: draft
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

Phase 1 dogfood vehicle for GSD/SDD pipeline validation. Not a real auth feature — see PROJECT.md milestone v1.0 framing. UI契約 locked at `.planning/phases/01-frontend-login-demo/01-UI-SPEC.md`.

## 用戶故事

1. As a team observer, I want to follow a complete GSD round-trip in this repo, so that I can replicate the workflow on my own feature.
2. As an internal user, I want to manually drive `/login` → `/welcome` after `pnpm dev`, so that the demo is observable.

## 範圍

### In scope
- `/login` page with username + password form (no validation, no auth state)
- `/welcome` placeholder page
- `vue-router` minimal config
- `App.vue` reduced to `<router-view />`

### Out of scope
- Real auth, form validation, error UI, router guards, design tokens, Pinia store (all deferred — see UI-SPEC §"Scope Lock")

## 驗收條件

- [ ] `pnpm dev` 起得來，`http://localhost:5173/login` 顯示 form — 對應測試：manual UAT
- [ ] `/` 自動 redirect 到 `/login` — 對應測試：manual UAT
- [ ] 點 submit 或在 input 按 Enter，URL 變 `/welcome` 並顯示 "Welcome" — 對應測試：manual UAT
- [ ] tab title 在 `/login` 為 `Login — ET-Omniverse`、在 `/welcome` 為 `Welcome — ET-Omniverse` — 對應測試：manual UAT

## 實作連結（完工後填）

- Router config: `src/frontend/ETOmniverse.Web/src/router/index.ts`
- Login page: `src/frontend/ETOmniverse.Web/src/pages/Login.vue`
- Welcome page: `src/frontend/ETOmniverse.Web/src/pages/Welcome.vue`
- App shell: `src/frontend/ETOmniverse.Web/src/App.vue`
- Composition root: `src/frontend/ETOmniverse.Web/src/main.ts`
- 主要 PR：#TBD

## Open questions

- [ ] Q-F001-001: vue-router 鎖 `^4` 還是 `^5`？（CONTEXT D-01 寫 4，registry latest 是 5.0.6）

## 變更記錄

| 日期 | 變更 | PR |
|---|---|---|
| 2026-05-09 | 初版 (status: draft) | #TBD |
```

### Example 7: `docs/no-doc-update-<topic>.md` for D-11 organic trigger

Reuse format from `docs/no-doc-update-wire-governance.md`. Likely topic for Phase 1: a small README adjustment, or a `package.json` scripts cleanup, or a vite.config.ts whitespace fix that touches `src/` but does NOT require any KB / spec / pattern update.

```markdown
# No Documentation Update Rationale — <topic>

## Change

- Files changed:
  - `<path>`
- Summary: <1-2 sentences>

## Reason No KB Update Is Needed

Explain why this does not change behavior, architecture, infra, access control, glossary, conventions, or reusable patterns. Reference the relevant `docs/...` section that already covers the area.

## Verification

- Command: `pnpm dev` / `pnpm build` / `python scripts/check-doc-governance.py`
- Result: <expected pass>
```

Concrete legitimate trigger (planner picks ONE):
- **Option A:** Fix `vite.config.ts` plugin bug (`[vue]` → `[vue()]`) in a standalone commit. The fix is mechanical, doesn't change behavior intent (the file is supposed to register the Vue plugin), no KB doc needs updating because nothing in `docs/INFRA.md` or `docs/CONVENTIONS.md` documents Vite plugin registration patterns. → write `docs/no-doc-update-vite-plugin-init.md`.
- **Option B:** Add `autocomplete` attributes to inputs in `Login.vue` after the initial commit (executor realizes browsers warn). Pure UX hint, no KB rule. → `docs/no-doc-update-login-autocomplete.md`.
- **Option C:** Tweak `index.html` lang attr or favicon (no, favicon is forbidden by UI-SPEC; skip this).

Recommend Option A — most defensible, fixes a real bug, has clear "this isn't behavior, it's correctness" framing.

## F-001 Spec Authoring (deep dive)

### Frontmatter required vs optional

Per `docs/specs/_template.md` + `docs/specs/README.md` + CONTEXT D-09:

| Key | Required | Source | Notes |
|-----|----------|--------|-------|
| `id` | YES | template + D-09 | `F-001` |
| `title` | YES | template + D-09 | One-line description |
| `module` | optional | template (CONTEXT D-09 omits) | Leave blank — `docs/specs/README.md` says module split deferred |
| `status` | YES | template + D-09 | Drives D-08 lifecycle |
| `owner` | YES | template + D-09 | GitHub handle |
| `created` | YES | template + D-09 | YYYY-MM-DD |
| `updated` | YES | template + D-09 | YYYY-MM-DD; bumped on every status edit |
| `supersedes` | optional | template | Blank for F-001 (first spec) |
| `superseded-by` | optional | template | Blank |
| `related-adr` | optional | template | `[]` (no ADRs this phase) |
| `related-interview` | optional | template | `[]` |
| `phase` | YES (per D-09) | template (template marks optional, D-09 promotes to required) | `1` |

**Resolution of conflict between template and D-09:** Template is the authoritative shape; D-09 lists the SUBSET that must be filled. Optional template keys should be present (with empty values) for parity, NOT omitted.

### Status transitions tied to commits

Per D-08:

| Phase event | F-001 `status` | F-001 `updated` | Trigger |
|-------------|----------------|-----------------|---------|
| Plan-phase end | `draft` | first set | F-001 created |
| Pre-execute (after PLAN approved) | `approved` | bump | Single commit changes status only |
| First execute commit | `implementing` | bump | Could share commit with first code task |
| verify-work pass | `implemented` | bump | Single commit changes status only |

These status-bump commits naturally satisfy Rule 1 because `docs/specs/F-001-*.md` IS in the `required_prefixes` of Rule 1. So a commit that ONLY edits F-001 status is fine — but it must ALSO touch some `src/` or `tests/` file to be meaningful, OR can be a pure docs commit (Rule 1 only fires when `src/` or `tests/` changes).

**Implication:** A pure `status: draft → approved` commit on F-001 alone trips ZERO governance rules — clean docs-only commit.

### Required content sections

Per `_template.md`: 業務背景, 用戶故事, 範圍 (In scope / Out of scope), 驗收條件, 實作連結 (post-impl), Open questions, 變更記錄. F-001 must include all of these. 驗收條件 must list `對應測試：<unit | integration | api | e2e>` per template — for Phase 1 these all read `manual UAT` per D-17.

## Governance Hook Behavior (deep dive)

### Rule inventory (verified by reading `scripts/check-doc-governance.py` 2026-05-09)

| Rule | Trigger paths | Required satisfier paths | Bypass via rationale? |
|------|--------------|--------------------------|----------------------|
| **Rule 1** `source-code-requires-spec-or-kb` | `src/`, `tests/` | `.planning/`, `docs/specs/`, `docs/patterns/`, `docs/ACCESS-CONTROL.md`, `docs/ARCHITECTURE.md`, `docs/CONVENTIONS.md`, `docs/DECISIONS.md`, `docs/GLOSSARY.md`, `docs/INFRA.md` | YES (rationale allowed) |
| **Rule 2** `infra-requires-infra-doc` | `docker/`, `ci/`, `.githooks/`, `scripts/` | `docs/INFRA.md`, `docs/WORKFLOW.md`, `docs/DOCUMENTATION.md` | YES (rationale allowed) |
| **Rule 4** `adr-summary-sync` | `docs/decisions/` | `docs/DECISIONS.md` | **NO** (rationale rejected; must update DECISIONS.md) |

(Rule numbering matches PROJECT.md "Rule 1/2/4 already validated" — there is no Rule 3 in the current code; the gap was intentional per `no-doc-update-wire-governance.md` "權限變更暫不做 path-based rule".)

### Trigger matrix for Phase 1 expected commits

| Likely commit | Files touched | Rules triggered | Satisfier(s) | Rationale needed? |
|---------------|---------------|-----------------|-------------|---------------------|
| `pnpm add vue-router` | `src/frontend/.../package.json` | Rule 1 | `.planning/phases/01-frontend-login-demo/PLAN.md` (or any `.planning/`) updated in same commit | NO if PLAN updated; YES otherwise |
| Add `src/router/index.ts` | `src/frontend/.../src/router/index.ts` | Rule 1 | Same as above | NO if `.planning/` or F-001 updated |
| Add `Login.vue` + `Welcome.vue` + edit `App.vue` + `main.ts` | `src/frontend/.../src/{App.vue, main.ts, pages/*}` | Rule 1 | F-001 status edit (`approved → implementing`) in same commit | NO |
| Status bump F-001 to `implemented` | `docs/specs/F-001-*.md` | NONE (Rule 1 doesn't trigger; doc-only) | N/A | NO |
| Fix `vite.config.ts` typo `[vue]` → `[vue()]` (standalone) | `src/frontend/.../vite.config.ts` | Rule 1 | None — pure mechanical fix; nothing in KB about Vite plugin registration | **YES — D-11 organic trigger** |

**The `vite.config.ts` typo fix is the most defensible D-11 trigger** (Pitfall 1 + Example 7 Option A). It's a real bug fix discovered during this phase, the file lives under `src/`, no KB doc covers Vite plugin init patterns, and `docs/no-doc-update-vite-plugin-init.md` would honestly answer "why no KB update".

### Hook plumbing notes

- `.githooks/pre-commit` invokes `python scripts/check-doc-governance.py --staged`.
- Setup: `git config core.hooksPath .githooks` (assumed already done; verify in plan-phase via `git config --get core.hooksPath`).
- The hook also runs `scripts/build-adr-index.py` if any ADR changes (no ADRs expected in Phase 1) and `scripts/check-spec-links.py` if `docs/specs/` changes (will fire on every F-001 commit — must pass; verify the spec has valid links).
- Hook requires `python` on PATH (3.10+). Plan-phase should verify.

## Build & Tooling Check

### Verified facts (2026-05-09)

| Fact | Source | Verified |
|------|--------|----------|
| `pnpm dev` starts Vite dev server on port 5173 | `package.json:scripts.dev` + `vite.config.ts:server.port` | ✓ |
| `pnpm build` runs `vue-tsc -b && vite build` | `package.json:scripts.build` | ✓ |
| `tsconfig.json` strict mode ON, `lib: ES2022 + DOM` | `tsconfig.json` direct read | ✓ |
| Vue Router 4.x and 5.x both have `vue: ^3.5.0` peerDep | npm registry direct fetch | ✓ |
| `pnpm-lock.yaml` not committed | `Glob` returned no match | ✓ |
| `vite.config.ts` has `plugins: [vue]` (missing `()`) | direct read line 5 | ✓ — bug |

### Vite SPA fallback for HTML5 history

Vite's default `appType` is `'spa'`, which automatically serves `index.html` for any unknown path during `vite dev`. No config change needed. (Production deploy is out of scope per PROJECT.md "Out of Scope: 部署".)

### TypeScript types for vue-router

`vue-router` ships its own `.d.ts` files in the npm package. No `@types/vue-router` separate package needed. `useRouter()`, `useRoute()`, `RouteRecordRaw`, etc. all type-check via the package's bundled types. tsconfig's `moduleResolution: Bundler` resolves correctly.

### Lockfile decision for this phase

STACK.md observation: "Lockfile: Not committed". No explicit policy doc. Recommendation: do NOT commit `pnpm-lock.yaml` in Phase 1; if the team wants to commit lockfiles in future, that's its own ADR (D-XX) — out of scope for milestone v1.0.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `vue-router@4.x` (Vue 3 mainline 2020-2024) | `vue-router@5.x` (Vue 3 mainline 2025+) | Dist-tag `latest` flipped to 5.x sometime in 2025 | API compatible for Phase 1 use; planner must reconcile with D-01 |
| Vue Router options API (v3, Vue 2 era) | Composition API helpers (`useRouter`, `useRoute`) | vue-router 4.0 | N/A — already on Vue 3 |
| Vue 2's `Vue.use(VueRouter)` global plugin | `app.use(router)` per-app | vue 3 | Already applied |

**Deprecated/outdated:**
- `vue-router@3.x` (`legacy` dist-tag) — Vue 2 era, do not use.
- Hash-mode routing (`createWebHashHistory`) — D-03 already chose HTML5 mode; hash mode is a fallback for environments with no SPA fallback (not our case).

## Open Questions

1. **Q-RES-001: vue-router version pin (`^4` vs `^5`)**
   - What we know: D-01 says `vue-router@4`. npm registry `latest` is `5.0.6`; `4.0.13` is under `next` dist-tag. API surface identical for Phase 1 needs.
   - What's unclear: Was D-01's "@4" a deliberate caution-pin (e.g., team has seen production issues with 5.x elsewhere) or stale-knowledge default? Auto-mode discussion logs (`DISCUSSION-LOG.md`) may clarify.
   - Recommendation: Surface to user during plan-check. Default to `pnpm add vue-router` (latest 5.x) since (a) 5.x is the maintained line, (b) API parity is verified, (c) this is greenfield with zero prod risk. If user prefers 4.x for personal reasons, pin `^4` explicitly. **Either choice is technically fine; the planner just must not silently pick.** Logged into F-001 as Q-F001-001 for traceability.

2. **Q-RES-002: should we fix the `vite.config.ts` `[vue]` bug as part of this phase?**
   - What we know: Bug is real (verified by reading file). Phase 1 will likely surface it once router + multi-page is added.
   - What's unclear: Whether fixing it is "in-scope routine fix" (yes, bundle into early task) or "out-of-scope quality drift" (no, leave for a follow-up).
   - Recommendation: Fix it. Pair the fix with the D-11 rationale-bypass exercise (the cleanest organic trigger). If executor doesn't fix it and `pnpm dev` breaks, the demo fails UI-03.

3. **Q-RES-003: walkthrough delivery format detail (D-14)**
   - What we know: D-13 says no slide deck; D-14 says ≤1 page `WALKTHROUGH.md` with section headings only.
   - What's unclear: Should the WALKTHROUGH.md include suggested talking-time per section (so the team can timebox the walkthrough)?
   - Recommendation: Out of researcher scope; planner can decide. Default: section headings + 1-line description per section, no time hints.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| pnpm | UI-03, D-15 | ✓ | (assumed installed; `package.json` scripts call it) | — |
| Node 22+ | Vite 7 | ✓ | (Vite 7 requires Node 20+; assumed satisfied) | — |
| Python 3.10+ | `pre-commit` hook (`scripts/check-doc-governance.py`) | ✓ | (script header says 3.10+; assumed satisfied based on past hook execution per PROJECT.md "已驗證 Rule 1/2/4") | — |
| `npm` on PATH | NONE — D-15 forbids npm | ✗ | — | Use `pnpm` (matches D-15) |
| git core.hooksPath = `.githooks` | DEMO-03 | unverified | — | Plan-phase task: `git config --get core.hooksPath`; if not set, run `git config core.hooksPath .githooks` (one-time setup) |

**Missing dependencies with no fallback:** None.

**Missing dependencies with fallback:** `npm` is missing from PATH, but D-15 forbids it anyway, so this is a non-issue. Important to call out so executor doesn't reflexively type `npm install`.

**Plan-phase pre-execute verification (Wave 0 candidate):**
```powershell
cd D:\et-omniverse-v2
git config --get core.hooksPath          # expect: .githooks
python --version                         # expect: 3.10+
pnpm --version                           # expect: any
cd src/frontend/ETOmniverse.Web
pnpm install                             # ensure node_modules present before adding vue-router
pnpm dev                                 # smoke test current placeholder before changes
```

## Validation Architecture

**Skipped — `workflow.nyquist_validation` is `false` in `.planning/config.json` (verified 2026-05-09).** D-17 also explicitly removes tests from this phase. Acceptance is manual UAT per UI-03.

## Walkthrough Artifact Layout (D-14)

Recommended `WALKTHROUGH.md` skeleton (≤1 page, section headings only — fill with paths only, no narrative duplication):

```markdown
# Phase 1 Walkthrough — Frontend Login Demo

> Material for the team session. Pointers only — open each artifact during the session.

## 1. Phase boundary & scope
- `.planning/PROJECT.md` (milestone v1.0)
- `.planning/REQUIREMENTS.md` (DEMO-01..03, UI-01..03, DOC-01)

## 2. Discussion → Context (auto-mode)
- `.planning/phases/01-frontend-login-demo/01-CONTEXT.md`
- `.planning/phases/01-frontend-login-demo/01-UI-SPEC.md`
- `.planning/phases/01-frontend-login-demo/DISCUSSION-LOG.md`

## 3. Research
- `.planning/phases/01-frontend-login-demo/01-RESEARCH.md` (this file)

## 4. Plan
- `.planning/phases/01-frontend-login-demo/01-PLAN.md`

## 5. SDD spec (the human-written contract)
- `docs/specs/F-001-frontend-login-page.md` (status timeline visible in git log)

## 6. Execute (commit-by-commit)
- `git log --oneline feat/<owner>/phase-1-login` (range covering this phase)
- Map: each commit ↔ PLAN row ↔ F-001 acceptance criterion

## 7. Governance hook in action
- `.githooks/pre-commit`
- `scripts/check-doc-governance.py` (Rule 1/2/4)
- `docs/no-doc-update-vite-plugin-init.md` (or similar — the rationale exercise)
- Show one commit's hook output (`git commit -v` excerpt or CI log)

## 8. Verify & ship
- `.planning/phases/01-frontend-login-demo/01-SUMMARY.md`
- The PR (eventual)

## 9. Live demo
- `cd src/frontend/ETOmniverse.Web && pnpm dev`
- Browser: `/` → `/login` → submit → `/welcome`
```

## Sources

### Primary (HIGH confidence)
- `D:\et-omniverse-v2\.planning\phases\01-frontend-login-demo\01-CONTEXT.md` — D-01..D-17 locked decisions (direct read 2026-05-09)
- `D:\et-omniverse-v2\.planning\phases\01-frontend-login-demo\01-UI-SPEC.md` — locked visual/interaction contract
- `D:\et-omniverse-v2\.planning\REQUIREMENTS.md` — DEMO/UI/DOC requirement IDs
- `D:\et-omniverse-v2\.planning\PROJECT.md` — milestone scope, Out of Scope, Key Decisions
- `D:\et-omniverse-v2\docs\WORKFLOW.md` — SDD process, status flow, PR rules
- `D:\et-omniverse-v2\docs\CONVENTIONS.md` — Vue conventions, git rules, 已知陷阱
- `D:\et-omniverse-v2\docs\AI-GUIDE.md` — red lines, stop-and-ask
- `D:\et-omniverse-v2\docs\DOCUMENTATION.md` — spec vs KB, governance regime
- `D:\et-omniverse-v2\docs\specs\_template.md` — F-001 frontmatter shape
- `D:\et-omniverse-v2\docs\specs\README.md` — F-XXX numbering, status flow
- `D:\et-omniverse-v2\docs\no-doc-update-_template.md` — rationale template
- `D:\et-omniverse-v2\docs\no-doc-update-wire-governance.md` — tone/format example
- `D:\et-omniverse-v2\.githooks\pre-commit` — direct read
- `D:\et-omniverse-v2\scripts\check-doc-governance.py` — Rule 1/2/4 source
- `D:\et-omniverse-v2\src\frontend\ETOmniverse.Web\package.json` — current deps verified
- `D:\et-omniverse-v2\src\frontend\ETOmniverse.Web\vite.config.ts` — `[vue]` bug verified by direct read
- `D:\et-omniverse-v2\src\frontend\ETOmniverse.Web\src\main.ts` — current composition root verified
- `D:\et-omniverse-v2\src\frontend\ETOmniverse.Web\src\App.vue` — current placeholder verified
- `D:\et-omniverse-v2\src\frontend\ETOmniverse.Web\tsconfig.json` — strict mode + ES2022 verified
- `D:\et-omniverse-v2\.planning\config.json` — `nyquist_validation: false` verified
- npm registry direct fetch <https://registry.npmjs.org/vue-router> — 5.0.6 latest, 4.0.13 next dist-tag (2026-05-09)
- Official Vue Router docs <https://router.vuejs.org/guide/> — `createRouter` + `createWebHistory` + `app.use(router)` verified (2026-05-09)

### Secondary (MEDIUM confidence)
- `D:\et-omniverse-v2\.planning\codebase\STACK.md` — Vue 3.5 / Vite 7 / TS 5.9 / pnpm; lockfile policy ("not committed") observational
- `D:\et-omniverse-v2\.planning\codebase\STRUCTURE.md` — confirms `src/frontend/ETOmniverse.Web/` Vue root
- `D:\et-omniverse-v2\.planning\codebase\CONVENTIONS.md` — TS/Vue naming + import order

### Tertiary (LOW confidence)
- (None — every claim sourced from primary or secondary above.)

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — registry directly verified, code files directly read.
- Architecture: HIGH — UI-SPEC + CONTEXT lock most choices; vue-router setup verified against official docs.
- F-001 spec authoring: HIGH — template + README + WORKFLOW directly read.
- Governance hook behavior: HIGH — script source code directly read.
- vue-router version conflict (Q-RES-001): MEDIUM — registry data is solid; what's unclear is the *intent* behind D-01's `@4` (history of the discussion). Confidence in the technical facts is HIGH; confidence in the recommended path forward is MEDIUM until user confirms.
- vite.config.ts bug: HIGH — direct file read shows `plugins: [vue]` not `[vue()]`.

**Research date:** 2026-05-09
**Valid until:** 2026-06-09 (30 days; vue-router has stable releases on the order of weeks; the rest is repo state which only changes with this phase)

---
