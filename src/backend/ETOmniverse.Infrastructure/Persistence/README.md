# Persistence

MSSQL is the transactional source of truth.

EF Core 10 migrations live under `Persistence/Migrations/`.

Rules:

- `EtOmniverseDbContext` is partial. Add module-specific mapping in `EtOmniverseDbContext.<Module>.cs`.
- Migrations are generated from the Infrastructure project with Api as startup project.
- `InitialBaseline` is intentionally empty; it pins the migration sequence before the first business table.
- Save operations should go through `IUnitOfWork`, not directly through feature code calling `DbContext.SaveChangesAsync`.
