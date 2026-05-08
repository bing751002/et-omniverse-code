---
phase: 01-frontend-login-demo
plan: 01
subsystem: governance
tags: [sdd, governance-hook, vue-router, vite, pnpm, rationale-bypass, dogfood]

requires:
  - phase: 00-bootstrap
    provides: docs/specs/_template.md, .githooks/pre-commit, scripts/check-doc-governance.py, scripts/check-spec-links.py, src/frontend/ETOmniverse.Web skeleton
provides:
  - F-001 spec at status:approved (the human-written SDD anchor for Phase 1)
  - vite.config.ts factory-invocation fix (unblocks Plan 02 router + multi-SFC)
  - vue-router ^5.0.6 runtime dep registered in package.json
  - First organic D-11 rationale-bypass artifact (docs/no-doc-update-vite-plugin-init.md) on disk
  - Live evidence that .githooks/pre-commit fires on real commits (DEMO-03 partially discharged)
affects: [01-02, 01-03, milestone-v1.0-walkthrough]

tech-stack:
  added: [vue-router@^5.0.6]
  patterns:
    - "Status-as-commit: F-001 status transitions land as separate commits, not silent edits (D-08)"
    - "Rationale-bypass pairing: src/-touching mechanical fixes pair with docs/no-doc-update-*.md in a single commit (D-11)"
    - "Spec-link placeholder: future-file paths in 實作連結 use <...> wrappers so check-spec-links.py treats them as placeholders"

key-files:
  created:
    - docs/specs/F-001-frontend-login-page.md
    - docs/no-doc-update-vite-plugin-init.md
  modified:
    - src/frontend/ETOmniverse.Web/vite.config.ts
    - src/frontend/ETOmniverse.Web/package.json

key-decisions:
  - "Status flips are commits not silent edits (D-08 enforced — 3 commits touch F-001 across the plan)"
  - "Rationale-bypass triggered organically by the D-18 vite.config.ts fix, not theatrically (D-11 satisfied)"
  - "Placeholder src paths in F-001 實作連結 wrapped with <...> after check-spec-links.py rejected backtick paths to non-existent files"

patterns-established:
  - "Plan-to-spec linkage: PLAN frontmatter's must_haves.artifacts list is the executor's checklist; SUMMARY records each artifact's commit hash"
  - "Hook-clean enforcement: every commit logs the pre-commit pipeline output; absence of --no-verify is verifiable via shell history"

requirements-completed: [DEMO-02, DEMO-03]

duration: 4min
completed: 2026-05-09
---

# Phase 01 Plan 01: Frontend Login Demo Pre-Execute Setup Summary

**F-001 spec authored (draft → approved across 3 commits), vite plugin-factory bug fixed paired with the first organic D-11 rationale-bypass artifact, vue-router ^5.0.6 installed via pnpm — all 4 commits cleared `.githooks/pre-commit` without `--no-verify`.**

## Performance

- **Duration:** ~4 min
- **Started:** 2026-05-08T22:59:41Z
- **Completed:** 2026-05-08T23:03:08Z
- **Tasks:** 4
- **Files modified:** 4 (2 created, 2 modified)

## Accomplishments

- F-001 spec exists at `docs/specs/F-001-frontend-login-page.md` with full template-shape frontmatter, status `approved`, and a 變更記錄 table tracking the draft → approved transition
- `vite.config.ts` plugin invocation corrected from `[vue]` (bare reference, invalid for `@vitejs/plugin-vue`) to `[vue()]` (factory call) — Plan 02 (router + multi-SFC) can now safely exercise the Vite loader
- `vue-router ^5.0.6` registered in `src/frontend/ETOmniverse.Web/package.json` via `pnpm add vue-router` (D-15 honored: pnpm only)
- `docs/no-doc-update-vite-plugin-init.md` is the first organic exercise of the D-11 rationale-bypass mechanism — paired with the vite fix in a single commit, recognized by `check-doc-governance.py` ("with rationale" log line)
- All 4 plan commits routed through `.githooks/pre-commit` cleanly; DEMO-03 ("hook + governance script in real commits, not theatre") is now substantively discharged

## Task Commits

Each task was committed atomically:

1. **Task 1.1: Author F-001 spec at status:draft** — `571d5d4` (docs)
2. **Task 1.2: Fix vite.config.ts + rationale-bypass commit** — `7a9e413` (fix)
3. **Task 1.3: Install vue-router via pnpm + Q-F001-001 resolved** — `66fc9c6` (feat)
4. **Task 1.4: F-001 status:draft → approved** — `059c1ba` (docs)

_Plan metadata commit (SUMMARY + STATE + ROADMAP + REQUIREMENTS) follows this section per execute-plan workflow._

## Files Created/Modified

- `docs/specs/F-001-frontend-login-page.md` — Human-written SDD spec for the login page; status approved; tracks Q-F001-001 resolution; 變更記錄 has 2 rows
- `docs/no-doc-update-vite-plugin-init.md` — D-11 rationale-bypass artifact paired with the vite fix; satisfies governance Rule 1 for src/-touching mechanical fixes
- `src/frontend/ETOmniverse.Web/vite.config.ts` — `plugins: [vue]` → `plugins: [vue()]` (single-character fix per D-18)
- `src/frontend/ETOmniverse.Web/package.json` — `dependencies` gains `"vue-router": "^5.0.6"`

## Decisions Made

- **Spec implementation links use `<...>` placeholder syntax for files that don't exist yet.** `scripts/check-spec-links.py` only treats paths containing `<`, `>`, `...`, `Module`, `Feature`, or `XXX` as placeholders. The plan's original spec body wrote real backtick paths to files that Plan 02 will create — the link checker rejected them. Wrapping them in `<...>` (matching the template's `<title>` convention) made the spec hook-clean while still being readable. Plan 02 executor will swap them to real backticks after creating the files. Tracked as Rule 1 deviation below.
- **Followed plan otherwise verbatim** — no architectural decisions, no scope-of-work shifts, no skipped tasks.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 — Bug] Spec implementation links rejected by `check-spec-links.py`**

- **Found during:** Task 1.1 (first commit attempt)
- **Issue:** The plan-supplied spec body for `## 實作連結（完工後填）` listed real backtick `src/frontend/...` paths to files that don't exist yet (`router/index.ts`, `pages/Login.vue`, `pages/Welcome.vue`, `App.vue`, `main.ts`). `scripts/check-spec-links.py` treats every backtick-quoted `src/...` path as a real link that must exist on disk; placeholder detection only triggers on `<`, `>`, `...`, `Module`, `Feature`, `XXX`. The hook failed with 3 broken-link errors, blocking the commit.
- **Fix:** Wrapped the future-file paths in `<...>` so the link checker's `is_placeholder()` recognizes them. Real existing file `vite.config.ts` kept as plain backtick (which the checker validated and passed). Added a one-liner note above the list explaining that Plan 02 will replace `<...>` wrappers with real paths after the files are created.
- **Files modified:** `docs/specs/F-001-frontend-login-page.md` (Implementation Links section only)
- **Verification:** Re-running `git commit` produced `OK 1 spec link(s) verified across 1 file` (vite.config.ts validated; placeholders skipped).
- **Committed in:** `571d5d4` (Task 1.1)

**2. [Rule 1 — Encoding] Used ASCII `->` in commit messages instead of Unicode `→`**

- **Found during:** Task 1.3 (commit message construction)
- **Issue:** The plan specified commit messages with the Unicode arrow `→` (e.g., `"feat(deps): add vue-router for /login → /welcome routing"`). Windows PowerShell / Git Bash on this machine has occasionally mangled Unicode in commit messages; on this work PC the safer-and-still-readable alternative is ASCII `->`.
- **Fix:** Used `->` in commit messages for Tasks 1.3 and 1.4. The semantic meaning is identical; the commit history remains greppable for status flows (`draft -> approved`).
- **Files modified:** none (only commit messages)
- **Verification:** `git log --oneline -4` shows clean ASCII messages.
- **Committed in:** `66fc9c6`, `059c1ba`

---

**Total deviations:** 2 auto-fixed (1 bug — broken spec links; 1 encoding — Unicode arrow → ASCII)
**Impact on plan:** Both deviations were mechanical and stayed inside the plan's intent. No scope creep, no architectural change. The spec-link placeholder convention adopted here is now a transferable pattern for future specs that reference yet-to-exist files (added to `patterns-established` above).

## Issues Encountered

- `check-spec-links.py` halted Task 1.1's first commit. Diagnosed root cause from `scripts/check-spec-links.py`'s placeholder logic and applied the `<...>` wrapper fix. No fight with the hook — the hook caught a real shape problem in the plan-supplied spec body, exactly as DEMO-03 wants.

## Hook Discipline Audit (DEMO-03 evidence)

Per D-12 ("No `--no-verify` allowed at any point"), every commit in this plan ran `.githooks/pre-commit` end-to-end:

| Commit | Pre-commit log signature | Outcome |
|---|---|---|
| `571d5d4` | `validating spec links` + `enforcing doc governance` | OK (1 spec link verified, 1 file passed governance) |
| `7a9e413` | `enforcing doc governance` (no spec touched) | OK (2 changed files passed governance **with rationale** — D-11 mechanism recognized) |
| `66fc9c6` | `validating spec links` + `enforcing doc governance` | OK (1 spec link verified, 2 files passed governance) |
| `059c1ba` | `validating spec links` + `enforcing doc governance` | OK (1 spec link verified, 1 file passed governance) |

The "with rationale" log line on `7a9e413` is the live evidence that D-11's rationale-bypass mechanism fired organically on a legitimate trigger, not as a contrived demo.

## F-001 Current State

- **Path:** `docs/specs/F-001-frontend-login-page.md`
- **Frontmatter snapshot:**
  - `id: F-001`
  - `title: Frontend login page (dogfood)`
  - `status: approved`
  - `owner: jimmyliao`
  - `created: 2026-05-09`
  - `updated: 2026-05-09`
  - `phase: 1`
- **Body:** all 7 template sections present (業務背景 / 用戶故事 / 範圍 / 驗收條件 / 實作連結 / Open questions / 變更記錄). Q-F001-001 closed. 變更記錄 has 2 rows (initial draft + draft → approved).

## Next Phase Readiness

- **Plan 02 executor**: F-001 is `approved`, vue-router is installed and importable, `vite.config.ts` is correct. Ready to drive code work (router config, Login.vue, Welcome.vue, App.vue shell, main.ts wiring). Per D-08, Plan 02's first or final commit should also bump F-001 `status: approved → implementing` (or `→ implemented` after verify-work, per project convention).
- **Plan 02 spec link follow-up**: When Plan 02 creates `router/index.ts`, `pages/Login.vue`, `pages/Welcome.vue`, `App.vue`, `main.ts`, the executor must update F-001 `## 實作連結` to swap `<src/...>` placeholders for real backtick paths. `check-spec-links.py` will then validate those as real links (currently they're skipped as placeholders).
- **No blockers.**

## Self-Check: PASSED

- `docs/specs/F-001-frontend-login-page.md` — FOUND
- `docs/no-doc-update-vite-plugin-init.md` — FOUND
- `src/frontend/ETOmniverse.Web/vite.config.ts` — FOUND (contains `plugins: [vue()],`)
- `src/frontend/ETOmniverse.Web/package.json` — FOUND (contains `"vue-router": "^5.0.6"`)
- Commit `571d5d4` — FOUND in git log
- Commit `7a9e413` — FOUND in git log
- Commit `66fc9c6` — FOUND in git log
- Commit `059c1ba` — FOUND in git log

---
*Phase: 01-frontend-login-demo*
*Completed: 2026-05-09*
