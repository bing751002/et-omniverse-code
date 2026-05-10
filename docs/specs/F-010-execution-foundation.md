---
id: F-010
title: Execution Foundation
status: implementing
owner: platform
related-adr: []
---

# F-010: Execution Foundation

## Context

v1.1 should establish execution paths before business workflows start. Two surfaces matter:

1. Background execution: the architecture targets Quartz.NET, but the package is not available in the current offline package cache.
2. Frontend integration: the backend already exposes OpenAPI, ProblemDetails, and test-auth conventions; frontend code needs a standard API path before real pages start calling APIs.

## In Scope

- Frontend package-free API client foundation.
- ProblemDetails parsing on the client side.
- Test-auth header helper for future Playwright/API smoke tests.
- Documenting the Quartz/Playwright package blocker instead of pretending those integrations are done.

## Out of Scope

- Business jobs.
- Real login.
- Real Playwright E2E suite until `@playwright/test` can be installed.
- Quartz implementation until the package is restored through an approved package path.

## Acceptance Criteria

- [x] **AC-1 API client foundation**: frontend has a shared HTTP client that accepts a base URL and maps ProblemDetails-like responses.
- [x] **AC-2 Test auth helper**: frontend/test code has a helper for `X-Test-User` / `X-Test-Roles` headers.
- [ ] **AC-3 Quartz package integration**: blocked until Quartz.NET package is available in restore path.
- [ ] **AC-4 Playwright harness**: blocked until `@playwright/test` package is available in pnpm install path.
- [x] **AC-5 No business workflow**: no 7-step business endpoint, page, or job is introduced.

## Implementation Links

- HTTP client: `src/frontend/ETOmniverse.Web/src/api/http.ts`
- ProblemDetails type: `src/frontend/ETOmniverse.Web/src/api/problemDetails.ts`
- Test auth helper: `src/frontend/ETOmniverse.Web/src/api/testAuth.ts`

## Blockers

- Quartz.NET is not present under the local NuGet cache, and this session cannot fetch packages.
- Playwright is not present in the frontend package set, and `pnpm install` currently hits a Windows `EPERM unlink` issue in this checkout.

---
*Created: 2026-05-10*
*Status: implementing because package-dependent parts are explicitly blocked*
