# Requirements: ET-Omniverse v1.1

**Defined:** 2026-05-10
**Milestone:** v1.1 — Foundation Hardening Before Business
**Core Value:** 用 SDD + GSD 工具棧把 7-step 排播流程從紙本散亂搬到結構化、可追溯的數位系統

## v1.1 Requirements

### Verification Foundation

- [ ] **VER-01**: Developer can run one documented local verification command that performs restore/build/test/governance checks and reports which layer failed.
- [ ] **VER-02**: CI runs the same quality gates as local verification, including docs governance, ADR index, spec links, forbidden logging API scan, forbidden DateTime scan, and test endpoint namespace scan.
- [ ] **VER-03**: CI covers backend build/test, frontend build, and Docker compose configuration validation without requiring business data.
- [ ] **VER-04**: Known Windows build/test failure modes such as MSB3491 access-denied and NuGet audit/network failures are documented with a supported workaround.

### Environment Foundation

- [ ] **ENV-01**: Developer can validate Docker compose configuration for infra/app services before running containers.
- [ ] **ENV-02**: API and web compose services expose health/readiness checks suitable for local stack verification.
- [ ] **ENV-03**: Environment variable examples and config docs distinguish required values, optional values, and secrets without introducing unused business/auth secrets.
- [ ] **ENV-04**: ConfigTool provides non-business commands to validate config and print effective config with secrets redacted.

### Execution Foundation

- [ ] **EXEC-01**: API host has a Quartz foundation for registering background jobs without embedding business workflow logic.
- [ ] **EXEC-02**: Background jobs use the established correlation/logging/time abstractions and have test coverage for execution and failure logging.
- [ ] **EXEC-03**: Frontend has a standard API client integration path based on existing OpenAPI/ProblemDetails contracts.
- [ ] **EXEC-04**: Frontend has a Playwright/test-auth harness that can exercise authenticated API/UI flows without real login or business workflows.

### Dependency Source Foundation

- [ ] **DEP-01**: Frontend build-critical package versions are exact and lockfile-backed.
- [ ] **DEP-02**: Repo-local pnpm config prevents accidental inheritance of developer-global offline mode.
- [ ] **DEP-03**: Package source / TLS / cache failures are documented as dependency-source failures, not build/test failures.

### Frontend Contract Foundation

- [ ] **CONTRACT-01**: API OpenAPI snapshot can be exported from the local API host.
- [ ] **CONTRACT-02**: Frontend has a committed OpenAPI snapshot and deterministic generated TypeScript contract metadata.
- [ ] **CONTRACT-03**: Verification detects stale frontend API contract output.

### Migration / DB Ops Foundation

- [ ] **DBOPS-01**: Repo has a local dotnet-ef tool manifest and scripts use it instead of relying on global `dotnet ef`.
- [ ] **DBOPS-02**: Developer can inspect migration status and generate idempotent SQL through repo scripts.
- [ ] **DBOPS-03**: Database update is explicit and not part of default verification.

## Future Requirements

### Business Features

- **BUS-01**: First real 7-step business slice can start after v1.1 hardening, using the verified build/test/CI/environment/job/frontend foundations.
- **AUTH-01**: Real login, local user store, RBAC, and scoped permissions remain deferred until an explicit Identity milestone.

## Out of Scope

| Feature | Reason |
|---------|--------|
| 7-step business workflow implementation | v1.1 is explicitly foundation hardening before business. |
| Real Identity/RBAC implementation | Security-sensitive and should be a dedicated milestone with product decisions. |
| Fugo / AD / LDAP / Qdrant | Existing architecture decisions defer these to later phases. |
| Production EFK/Prometheus rollout | INFRA stages this later; v1.1 only strengthens local/CI/developer foundations. |
| Business seed data | ConfigTool scope is config validation only in this milestone. |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| VER-01 | Phase 8 | Complete |
| VER-02 | Phase 8 | Complete |
| VER-03 | Phase 8 | Partial: backend restore/build/test and Docker compose config are verified; frontend build is blocked by missing pnpm package metadata/tarballs in offline cache |
| VER-04 | Phase 8 | Complete |
| ENV-01 | Phase 9 | Complete |
| ENV-02 | Phase 9 | Complete |
| ENV-03 | Phase 9 | Complete |
| ENV-04 | Phase 9 | Complete |
| EXEC-01 | Phase 10 | Blocked: Quartz.NET package unavailable in current restore path |
| EXEC-02 | Phase 10 | Blocked: depends on EXEC-01 |
| EXEC-03 | Phase 10 | Complete |
| EXEC-04 | Phase 10 | Partial: test-auth header helper exists; Playwright harness blocked by missing package/install |
| DEP-01 | Phase 11 | Complete |
| DEP-02 | Phase 11 | Complete |
| DEP-03 | Phase 11 | Complete; full install still needs package tarballs or reachable registry |
| CONTRACT-01 | Phase 12 | Complete |
| CONTRACT-02 | Phase 12 | Complete |
| CONTRACT-03 | Phase 12 | Complete |
| DBOPS-01 | Phase 13 | Complete |
| DBOPS-02 | Phase 13 | Partial: idempotent SQL generation verified; live DB status not run |
| DBOPS-03 | Phase 13 | Complete |

**Coverage:**
- v1.1 requirements: 21 total
- Mapped to phases: 21
- Unmapped: 0

---
*Requirements defined: 2026-05-10*
*Last updated: 2026-05-10 after dependency source, frontend contract, and DB ops foundation setup*
