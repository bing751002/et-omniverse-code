# No Documentation Update Rationale — Vite Plugin Init Fix

## Change

- Files changed:
  - `src/frontend/ETOmniverse.Web/vite.config.ts`
- Summary: Fix `plugins: [vue]` → `plugins: [vue()]`. `@vitejs/plugin-vue` exports a factory; passing the bare reference is invalid. The placeholder `App.vue` was trivial enough that Vite's loader cascade still served it, but Phase 1 (router + multi-SFC) will surface the bug. Pure mechanical fix — single-line, single-character — restoring the file to its intended state.

## Reason No KB Update Is Needed

Nothing in `docs/INFRA.md`, `docs/CONVENTIONS.md`, `docs/ARCHITECTURE.md`, `docs/DECISIONS.md`, or `docs/patterns/` documents Vite plugin registration patterns or covers `vite.config.ts` shape. The fix does not change architecture, infra topology, access control, glossary, conventions, or reusable patterns — it brings the config file in line with `@vitejs/plugin-vue`'s documented usage. No spec to update either: `vite.config.ts` is build tooling, not feature code, and Phase 1's F-001 spec covers the login page surface, not the bundler config. This is exactly the case `docs/DOCUMENTATION.md` §強制機制 contemplates for the `docs/no-doc-update-*.md` mechanism.

## Verification

- Command: `cd src/frontend/ETOmniverse.Web && pnpm dev` (smoke test) and `python scripts/check-doc-governance.py --staged`
- Result: governance check passes (Rule 1 satisfied via this rationale file); `pnpm dev` starts cleanly with the corrected plugin invocation.
