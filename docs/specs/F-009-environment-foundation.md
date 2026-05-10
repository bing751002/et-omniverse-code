---
id: F-009
title: Environment Foundation
status: implemented
owner: platform
related-adr: []
---

# F-009: Environment Foundation

## Context

v1.1 hardens the non-business foundation before new feature work starts. Developers need a local stack that can be validated before containers are started, and a config tool that can explain missing or unsafe configuration without running business workflows.

## In Scope

- Docker compose validation remains part of verification.
- API and web compose services expose health checks.
- `.env.example` contains only values currently used by the local stack.
- ConfigTool supports config validation and redacted config output.

## Out of Scope

- Business seed data.
- Real Identity/JWT/RBAC secrets.
- Production deployment automation.
- EFK/Prometheus rollout.

## Acceptance Criteria

- [x] **AC-1 Compose healthchecks**: API and web services define health checks suitable for local stack verification.
- [x] **AC-2 Env example discipline**: `.env.example` does not include unused auth/business secrets.
- [x] **AC-3 Config validation**: `ETOmniverse.Tools.ConfigTool validate` checks required non-business config.
- [x] **AC-4 Redacted config output**: `ETOmniverse.Tools.ConfigTool print --redacted` prints effective JSON config with secret-like values masked.
- [x] **AC-5 No business data**: ConfigTool does not seed or mutate business data.

## Implementation Links

- API Dockerfile: `docker/Dockerfile`
- Compose API service: `docker/compose/base.api.yml`
- Compose web service: `docker/compose/base.web.yml`
- Env example: `docker/.env.example`
- ConfigTool: `src/backend/ETOmniverse.Tools.ConfigTool/Program.cs`

---
*Created: 2026-05-10*
*Status: implemented as part of v1.1 Phase 9 foundation hardening*
