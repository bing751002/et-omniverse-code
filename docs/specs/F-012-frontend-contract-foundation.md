---
id: F-012
title: Frontend Contract Foundation
module: platform
status: implemented
owner: platform
created: 2026-05-10
updated: 2026-05-10
supersedes:
superseded-by:
related-adr: []
related-interview: []
phase: 12
---

# F-012: Frontend Contract Foundation

## Background

Frontend code currently has a hand-written API client foundation. `docs/CONVENTIONS.md` says frontend API clients should come from OpenAPI, so the foundation needs a repeatable contract export/check path before business pages start calling APIs.

## In Scope

- Scriptable OpenAPI export from the local API host.
- A committed frontend OpenAPI snapshot for contract review.
- A lightweight generated TypeScript contract surface derived from the OpenAPI snapshot.
- Verification command that can detect snapshot drift.

## Out of Scope

- Full generated SDK with runtime dependencies.
- Business API endpoints.
- Authenticated Playwright flows.

## Acceptance Criteria

- [x] **AC-1 OpenAPI export script**: a script can export `openapi/v1.json` from the API host without business data.
- [x] **AC-2 Contract snapshot**: frontend owns a committed OpenAPI snapshot.
- [x] **AC-3 Generated TS contract**: a deterministic script generates TypeScript path/method metadata from the snapshot.
- [x] **AC-4 Drift check**: verification can compare regenerated contract output against committed output.
- [x] **AC-5 No business API introduced**: only existing common/test-safe endpoints are represented.

## Implementation Links

- Export script: `scripts/export-openapi.ps1`
- Generate script: `scripts/generate-frontend-api-contract.mjs`
- Check script: `scripts/check-frontend-api-contract.ps1`
- Snapshot: `src/frontend/ETOmniverse.Web/src/api/openapi.json`
- Generated contract: `src/frontend/ETOmniverse.Web/src/api/generated/openapi-contract.ts`

## Open Questions

- [ ] Q-F012-001: Which external generator should be adopted later if a full SDK is needed?

## Verification

Passed:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\check-frontend-api-contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\verify-local.ps1 -SkipFrontend
```

Result: contract drift check is part of local verification and passed.

## Change Log

| Date | Change | PR |
|---|---|---|
| 2026-05-10 | Implemented OpenAPI snapshot and frontend contract drift check | #TBD |
