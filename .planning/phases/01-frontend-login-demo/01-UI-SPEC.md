---
phase: 1
slug: frontend-login-demo
status: approved
shadcn_initialized: false
preset: none
created: 2026-05-09
reviewed_at: 2026-05-09
---

# Phase 1 — UI Design Contract

> Visual and interaction contract for Phase 1 (Frontend Login Demo). Dogfood-grade scope per PROJECT.md / REQUIREMENTS.md — milestone success is GSD process validation, not visual quality. UI is the vehicle, not the deliverable.

---

## Scope Lock (read first)

This phase deliberately uses **Vue 3 + Vite defaults with no design system**. The following are out of scope and will not appear in this contract:

- Design tokens / theming layer (UI-06, deferred to v2)
- Component library (Tailwind / UnoCSS / Element Plus / Naive UI / shadcn-vue / etc.)
- Form validation, error states, loading states (UI-04 / UI-05, deferred)
- Auth state, router guards (UI-07, deferred)
- Responsive / mobile / cross-browser / a11y rigor (deferred)
- Reusable component abstraction (`components/` library)

If the executor finds themselves reaching for any of the above — STOP, this phase is wrong. Reopen `/gsd:discuss-phase` instead.

**Pages in scope:** `/login` (UI-01), `/welcome` (UI-02). Both are single-file Vue components (`src/pages/Login.vue`, `src/pages/Welcome.vue` — exact path at executor discretion).

---

## Design System

| Property | Value |
|----------|-------|
| Tool | none |
| Preset | not applicable |
| Component library | none — plain `<input>`, `<button>`, `<form>` |
| Icon library | none — no icons in this phase |
| Font | browser default (system stack via UA stylesheet) |

Rationale: UI-06 (design system extraction) is explicitly v2. Vue/CSS defaults are sufficient for internal dogfood.

---

## Spacing Scale

No formal token scale. Executor may use the following ad-hoc values directly in `<style scoped>` if helpful, but is **not required** to follow them rigorously:

| Approx Value | Typical Usage |
|--------------|---------------|
| 8px | gap between form rows |
| 16px | input vertical padding, page padding |
| 24px | gap between page title and form |

Exceptions: none enforced. Default browser margins are acceptable.

---

## Typography

Browser defaults via UA stylesheet. No custom font, no custom size scale.

| Role | Size | Weight | Line Height |
|------|------|--------|-------------|
| Body | UA default (~16px) | UA default (400) | UA default (~1.5) |
| Label | UA default | UA default | UA default |
| Heading (h1) | UA default | UA default (bold) | UA default |
| Display | not used | — | — |

If executor wants to set `body { font-family: system-ui, sans-serif; }` once in a global stylesheet, that is acceptable. No further typography decisions.

---

## Color

No palette declared. Browser defaults only.

| Role | Value | Usage |
|------|-------|-------|
| Dominant (60%) | `#fff` (UA default body bg) | page background |
| Secondary (30%) | not used | — |
| Accent (10%) | UA default link/button color | submit button only |
| Destructive | not used | no destructive actions in this phase |

Accent reserved for: the single submit button on `/login`. No other accent surfaces.

Rationale: visual design system is v2. Internal dogfood does not require a palette.

---

## Copywriting Contract

All copy is English and static. No i18n, no template interpolation.

| Element | Copy |
|---------|------|
| Browser tab title `/login` | `Login — ET-Omniverse` |
| Browser tab title `/welcome` | `Welcome — ET-Omniverse` |
| Login page heading (h1) | `Login` |
| Username field label | `Username` |
| Password field label | `Password` |
| Primary CTA (submit button) | `Log in` |
| Welcome page content | `Welcome` (single static word, h1 or equivalent) |
| Empty state | not applicable — no empty states in this phase |
| Error state | not applicable — no error UI in this phase |
| Destructive confirmation | not applicable — no destructive actions |

Notes:
- Submit button does **not** show "Logging in…" or any loading text. On click, immediately `router.push('/welcome')` per UI-01.
- Welcome page shows the literal word `Welcome` only. No `Welcome, {username}` — there is no auth state to source a username from (per locked decision in PROJECT.md).
- No placeholder text inside the inputs (labels are sufficient).

---

## Interaction Contract

| Trigger | Behavior |
|---------|----------|
| User loads `/` | router redirects to `/login` (or `/login` is the default route — executor's choice) |
| User types in username / password | no validation, no feedback, default `<input>` behavior |
| User submits form (click button or press Enter) | `event.preventDefault()` then `router.push('/welcome')`. No network call, no auth check, no localStorage write. |
| User loads `/welcome` directly (no prior login) | page renders normally — no router guard exists |
| User loads any unknown route | executor's discretion (404 page or redirect to `/login`); not gated for this phase |

---

## Registry Safety

| Registry | Blocks Used | Safety Gate |
|----------|-------------|-------------|
| none | none | not applicable — no third-party UI components used |

No shadcn / shadcn-vue / external block registries are used in this phase. Executor must NOT introduce one without reopening discuss-phase.

---

## Out-of-Scope Reminders for Executor

When implementing, the following are explicit non-goals — do not add them even if they feel natural:

- ❌ Tailwind / UnoCSS / any utility CSS framework
- ❌ Element Plus / Naive UI / Vuetify / PrimeVue / shadcn-vue
- ❌ `<input required>` or `:rules` or any FluentValidation-style client validation
- ❌ Pinia store for `currentUser` / `isLoggedIn`
- ❌ `localStorage.setItem('fakeLoggedIn', ...)` or similar fake auth state
- ❌ Router `beforeEach` guard
- ❌ Loading spinner / skeleton on submit
- ❌ Any `axios` / `fetch` call from the login page
- ❌ Animations, transitions, hover micro-interactions beyond browser defaults
- ❌ Dark mode toggle
- ❌ Logo / brand mark / hero imagery
- ❌ Footer / header / navigation chrome (pages are full-bleed forms)

If something on this list seems "easy to add anyway" — the entire point of the milestone is to demonstrate scope discipline through GSD. Adding any of these breaks the dogfood demo.

---

## Checker Sign-Off

- [x] Dimension 1 Copywriting: PASS (CTA `Log in`; empty/error/destructive states correctly marked not-applicable per locked scope)
- [x] Dimension 2 Visuals: PASS (defaults only, no visual contract to enforce)
- [x] Dimension 3 Color: PASS (UA defaults; accent reserved for single submit button)
- [x] Dimension 4 Typography: PASS (UA defaults only — 0 custom sizes/weights)
- [x] Dimension 5 Spacing: PASS (ad-hoc 8/16/24px, multiples of 4, non-mandatory)
- [x] Dimension 6 Registry Safety: PASS (no third-party registry; explicit forbid list)

**Approval:** approved (2026-05-09 by gsd-ui-checker)

---

## Notes for Downstream Consumers

- **gsd-planner**: Plan tasks should reference UI-01 / UI-02 / UI-03 from REQUIREMENTS.md directly. No design-token tasks. Two `.vue` files + one router config + one `main.ts` mount = full surface area.
- **gsd-executor**: When in doubt, write less code. Plain `<form>` with two `<input>` and one `<button>`. Inline `<style scoped>` is fine; a global `style.css` with only `body { font-family: system-ui; }` is also fine. Both are dogfood-acceptable.
- **gsd-ui-checker / gsd-ui-auditor**: This contract intentionally declares "defaults only" across most dimensions. Validate that the implementation has NOT silently added a design system, validation, auth state, or guards. Presence of any of those = FAIL even if visually polished.
