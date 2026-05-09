# Phase 1 Walkthrough — Frontend Login Demo

> Material for the team session. Pointers only — open each artifact during the session. ≤ 1 page by design (per D-14).

## 1. Phase boundary & scope
Where this milestone fits and what was deliberately NOT done.
- `.planning/PROJECT.md` (milestone v1.0 framing — process validation, not product feature)
- `.planning/REQUIREMENTS.md` (DEMO-01..03, UI-01..03, DOC-01)
- `.planning/ROADMAP.md` §"Phase 1" (5 success criteria)

## 2. Discussion → Context (auto-mode)
How locked decisions were captured before any code was written.
- `.planning/phases/01-frontend-login-demo/01-CONTEXT.md` (D-01..D-18 — 18 locked decisions)
- `.planning/phases/01-frontend-login-demo/01-UI-SPEC.md` (locked visual / interaction contract)
- `.planning/phases/01-frontend-login-demo/01-DISCUSSION-LOG.md` (auto-mode rationale trail)

## 3. Research
How Claude verified the technical surface before planning.
- `.planning/phases/01-frontend-login-demo/01-RESEARCH.md` (vue-router 5.x verification, vite.config.ts bug, governance hook trigger matrix)

## 4. Plan
How the work was sliced into waves and tasks.
- `.planning/phases/01-frontend-login-demo/01-01-PLAN.md` (Wave 1: F-001 draft, vite fix + rationale-bypass, vue-router install, F-001 approved)
- `.planning/phases/01-frontend-login-demo/01-02-PLAN.md` (Wave 2: router + 2 pages + main.ts + App.vue, F-001 implementing)
- `.planning/phases/01-frontend-login-demo/01-03-PLAN.md` (Wave 3: manual UAT, F-001 implemented, this WALKTHROUGH.md)

## 5. SDD spec — the human-written contract
The spec that drove the implementation, and its lifecycle in git.
- `docs/specs/F-001-frontend-login-page.md` (terminal status: implemented)
- Run `git log --oneline -- docs/specs/F-001-frontend-login-page.md` to see the draft → approved → implementing → implemented chain (5 commits)

## 6. Execute — commit-by-commit
Each commit is one PLAN row. Show the git log range for this phase.
- `git log --oneline feat/p1/frontend-login-demo` (or whatever branch was used) for the full Phase 1 range
- Map each commit to the corresponding PLAN task and F-001 acceptance criterion

## 7. Governance hook in action
The DEMO-03 evidence — pre-commit hook ran on every commit, rationale-bypass fired once organically.
- `.githooks/pre-commit` (the hook script)
- `scripts/check-doc-governance.py` (Rule 1/2/4 source)
- `docs/no-doc-update-vite-plugin-init.md` (the D-11 rationale-bypass artifact, paired with the vite.config.ts fix per D-18)
- During the session: show one commit's hook output (`git commit -v` excerpt or re-run `python scripts/check-doc-governance.py` against a recent diff)

## 8. Verify & ship
Wrap-up artifacts (created by `/gsd:verify-work` and `/gsd:ship` after this plan).
- `.planning/phases/01-frontend-login-demo/01-03-SUMMARY.md`
- The PR (eventual)

## 9. Live demo
The 90-second demo at the end of the session.
- `cd src/frontend/ETOmniverse.Web && pnpm dev`
- Browser: `http://localhost:5173/` → auto-redirect to `/login` → fill any text → submit (click or Enter) → `/welcome` displays the literal word `Welcome`
