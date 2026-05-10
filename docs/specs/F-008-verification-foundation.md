---
id: F-008
title: Verification Foundation
status: implemented
owner: platform
related-adr: []
---

# F-008: Verification Foundation

## Context

v1.0 established backend foundation, but local verification is still fragile: Windows `obj` / artifacts ACL issues and NuGet audit/network failures can make `dotnet build/test` fail before code is actually evaluated. CI also needs to run the same quality gates as local development so main quality is enforced by automation, not memory.

This spec does not introduce business features. It only defines the verification entry points for v1.1 Phase 8.

## In Scope

- A single local verification script that runs governance checks, backend restore/build/test, frontend build, and Docker compose config validation.
- A CI wrapper script that calls the same verification path with CI defaults.
- Jenkins pipeline updated to call the CI wrapper instead of re-declaring a parallel checklist.
- Documented workarounds for known local verification failure modes.

## Out of Scope

- Business workflow tests.
- Real deployment.
- Coverage threshold enforcement.
- Docker daemon dependent Testcontainers execution policy changes beyond existing Docker skip/fail behavior.

## Acceptance Criteria

- [x] **AC-1 Local verification entrypoint**: `scripts/verify-local.ps1` exists and can run checks in named stages.
- [x] **AC-2 CI verification entrypoint**: `scripts/verify-ci.ps1` exists and delegates to the local entrypoint with CI-safe defaults.
- [x] **AC-3 CI gate coverage**: Jenkins calls the CI verification entrypoint and covers docs governance, guard scripts, backend build/test, frontend build, and Docker compose config.
- [x] **AC-4 Known failure mode docs**: `docs/INFRA.md` documents the supported temp artifacts workaround, global NuGet cache default, NuGet audit behavior, MSBuild `/m:1` workaround, and frontend temp-copy verification behavior.
- [x] **AC-5 No business scope**: verification scripts do not require business data or execute business workflows.

## Implementation Links

- Local verification: `scripts/verify-local.ps1`
- CI verification: `scripts/verify-ci.ps1`
- Jenkins pipeline: `ci/jenkins/Jenkinsfile`
- Infra documentation: `docs/INFRA.md`

---
*Created: 2026-05-10*
*Status: implemented as part of v1.1 Phase 8 foundation hardening*
