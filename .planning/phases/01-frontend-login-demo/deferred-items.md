# Deferred Items — Phase 01 Frontend Login Demo

Out-of-scope discoveries surfaced during execute-phase. Tracked here for future plans / phases. NOT blocking for this phase.

## Discovered during Plan 01-02

### vue-tsc declaration-emit leaks into `src/`

- **Found during:** Task 2.5 `pnpm build` smoke test
- **Symptom:** After `pnpm build`, the source tree gets polluted with `*.vue.js`, `*.vue.js.map`, `main.js`, `main.js.map` files emitted alongside the `.vue` and `.ts` sources.
- **Root cause (likely):** `tsconfig.json` (the Vue app one, not the node one) does not set `noEmit: true` or `emitDeclarationOnly: true`, so `vue-tsc -b` emits JS for each source. The standard Vue 3 + Vite scaffold has `noEmit: true` for the app tsconfig because Vite handles bundling — type-checking only.
- **Workaround applied (Plan 02):** Added these patterns to `.gitignore` so the pollution doesn't leak into git. The actual fix (set `noEmit: true` in `src/frontend/ETOmniverse.Web/tsconfig.app.json` or equivalent) is a separate concern.
- **Recommendation:** Plan 03 (manual UAT) doesn't depend on this; can be picked up post-phase as a small `chore(frontend)` cleanup commit, or in a future phase that touches the build config.
- **Severity:** low (cosmetic / git hygiene only — does not affect dev or prod build correctness)
