---
id: F-011
title: Dependency Source Foundation
module: platform
status: implemented
owner: platform
created: 2026-05-10
updated: 2026-05-10
supersedes:
superseded-by:
related-adr: []
related-interview: []
phase: 11
---

# F-011: Dependency Source Foundation

## Background

v1.1 verification can build and test the backend, but frontend package install is blocked by a mix of missing local pnpm cache, global `offline=true`, and no `pnpm-lock.yaml`.
Quartz and Playwright are also blocked by package source availability.

This foundation does not introduce business behavior. It only makes dependency resolution explicit and repeatable.

## In Scope

- Frontend package versions are pinned instead of floating through `^` ranges.
- Repo-local pnpm config overrides global offline mode for this project.
- `pnpm-lock.yaml` is generated when registry access is available.
- NuGet package source expectations are documented and verifiable.
- Verification reports package-source blockers distinctly from code/build failures.

## Out of Scope

- Adding Quartz or Playwright runtime behavior.
- Private registry / artifact mirror provisioning.
- Updating production deployment credentials or secrets.

## Acceptance Criteria

- [x] **AC-1 Frontend versions pinned**: `src/frontend/ETOmniverse.Web/package.json` uses exact dependency versions for build-critical packages.
- [x] **AC-2 Repo pnpm policy**: repo-local frontend config does not inherit a developer's accidental global offline mode.
- [x] **AC-3 Lockfile present**: `src/frontend/ETOmniverse.Web/pnpm-lock.yaml` exists and is used by verification when present.
- [x] **AC-4 NuGet source docs**: `docs/INFRA.md` documents package-source expectations and when `-PackagesPath` may be used.
- [x] **AC-5 Verification script remains layer-specific**: dependency-source failure is surfaced at frontend install instead of being confused with TypeScript/Vite compile errors.

## Implementation Links

- Frontend package policy: `src/frontend/ETOmniverse.Web/package.json`
- Frontend pnpm config: `src/frontend/ETOmniverse.Web/.npmrc`
- Verification: `scripts/verify-local.ps1`
- Infra docs: `docs/INFRA.md`

## Open Questions

- [ ] Q-F011-001: Should the team use an internal npm/NuGet mirror, or is direct registry access acceptable for CI?

## Verification

Passed:

```powershell
$src = Resolve-Path src\frontend\ETOmniverse.Web
$work = Join-Path $env:TEMP ("et-omniverse-pnpm-lock-check-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Force -Path $work
Copy-Item -Path (Join-Path $src "package.json"),(Join-Path $src ".npmrc"),(Join-Path $src "pnpm-lock.yaml") -Destination $work
pnpm install --dir $work --frozen-lockfile --lockfile-only --offline
```

Result: lockfile is consistent with `package.json`.

Known residual blocker: full offline install still fails if the local pnpm store does not contain package tarballs, for example `vite-7.3.2.tgz`. That is package-cache availability, not lockfile drift.

## Change Log

| Date | Change | PR |
|---|---|---|
| 2026-05-10 | Implemented package source policy and lockfile | #TBD |
