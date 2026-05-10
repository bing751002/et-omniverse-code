---
id: F-013
title: Migration and DB Ops Foundation
module: platform
status: implementing
owner: platform
created: 2026-05-10
updated: 2026-05-10
supersedes:
superseded-by:
related-adr: []
related-interview: []
phase: 13
---

# F-013: Migration and DB Ops Foundation

## Background

EF Core persistence is in place, but DB operations are still mostly developer memory. Before business tables grow, the repo needs explicit scripts for migration status, SQL script generation, and database update.

## In Scope

- Non-destructive migration status command.
- Idempotent SQL migration script generation.
- Explicit database update command that uses the existing API startup configuration.
- INFRA/CONVENTIONS docs that explain when each command is safe to use.

## Out of Scope

- Creating new business migrations.
- Editing committed migration files.
- Running production DB changes automatically.
- Backup/restore automation for production data.

## Acceptance Criteria

- [ ] **AC-1 Migration status script**: developer can list pending/applied migrations with one repo script. Script exists; live DB verification not run in default verification.
- [x] **AC-2 Idempotent SQL script generation**: developer can generate SQL without connecting to a database.
- [ ] **AC-3 DB update script**: developer can explicitly apply migrations to a configured database. Script exists; mutation command is intentionally not run by default verification.
- [x] **AC-4 Docs updated**: `docs/INFRA.md` and `docs/CONVENTIONS.md` explain the command split and safety boundary.
- [x] **AC-5 Verification-safe**: default verification does not mutate a database.

## Implementation Links

- Add migration: `scripts/db-add-migration.ps1`
- Migration status: `scripts/db-status.ps1`
- Migration script: `scripts/db-script-migration.ps1`
- Database update: `scripts/db-update.ps1`

## Verification

Passed:

```powershell
dotnet tool restore
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ensure-dotnet-ef.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\db-script-migration.ps1 -OutputPath $env:TEMP\et-omniverse-idempotent.sql
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\verify-local.ps1 -SkipFrontend
```

Result: repo-local `dotnet-ef` is available, idempotent SQL script generation works without DB mutation, and default verification does not run DB update.

Not run: `db-status.ps1` and `db-update.ps1` against a live database. `db-status.ps1` needs a reachable DB; `db-update.ps1` mutates DB state and must stay explicit.

## Open Questions

- [ ] Q-F013-001: Should deployment later use EF migration bundle, SQL script review, or a release-runner job?

## Change Log

| Date | Change | PR |
|---|---|---|
| 2026-05-10 | Implemented no-mutation migration script path; live DB status/update pending | #TBD |
