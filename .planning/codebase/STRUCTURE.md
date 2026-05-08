# Codebase Structure

**Analysis Date:** 2026-05-08

## Directory Layout

```
et-omniverse-v2/
├── .planning/codebase/         # GSD working documents (this directory)
├── .github/                     # GitHub templates and workflows
├── .gitlab/                     # GitLab merge request templates
├── .githooks/                   # Git hooks
├── ci/                          # Continuous integration
│   └── jenkins/                 # Jenkinsfile for build pipeline
├── docker/                      # Docker and deployment configuration
│   ├── compose/                 # Docker Compose services
│   ├── scripts/                 # Build and deployment scripts
│   ├── docker-compose.yml       # Main app services (api, web, mssql)
│   ├── docker-compose.infra.yml # Infrastructure services (elasticsearch, kibana, apm, fluent-bit)
│   ├── Dockerfile               # Backend API image
│   ├── Dockerfile.frontend      # Frontend image
│   ├── nginx.conf               # Nginx reverse proxy config
│   └── .env.example             # Environment variables template
├── docs/                        # Knowledge base (specs, decisions, interviews, patterns)
│   ├── decisions/               # ADR (Architecture Decision Records) D10-D18
│   ├── interviews/              # Stakeholder interviews
│   ├── patterns/                # Reusable design patterns
│   ├── specs/                   # Feature specifications
│   ├── retrospectives/          # Post-mortems and learnings
│   ├── ARCHITECTURE.md          # High-level tech stack and repo structure
│   ├── DECISIONS.md             # Summary of ADRs (auto-generated index)
│   ├── CONVENTIONS.md           # Coding standards, naming, testing rules
│   ├── WORKFLOW.md              # SDD/ADR/interview process, PR discipline
│   ├── GLOSSARY.md              # Business and technical vocabulary
│   ├── INFRA.md                 # Docker Compose, environment, CI/CD phases
│   ├── ACCESS-CONTROL.md        # Permission model (company/division/dept/external)
│   └── DOCUMENTATION.md         # Spec vs KB, code-comment conventions
├── scripts/                     # Utility scripts (build-adr-index.py, etc.)
├── src/
│   ├── backend/                 # .NET 10 services
│   │   ├── ETOmniverse.Api/                      # Web host + Minimal API endpoints
│   │   │   ├── Features/
│   │   │   │   ├── Common/
│   │   │   │   │   └── Health/                  # Health check endpoints
│   │   │   │   └── Identity/
│   │   │   │       ├── Adapter/
│   │   │   │       │   ├── In/
│   │   │   │       │   │   ├── Endpoints/       # Minimal API endpoint registration
│   │   │   │       │   │   ├── Model/           # Request/response DTOs
│   │   │   │       │   │   ├── Validation/      # FluentValidation rules
│   │   │   │       │   │   └── Extensions/      # Request → Command mapping
│   │   │   │       │   └── Out/
│   │   │   │       │       ├── Model/           # Read model/projection DTOs
│   │   │   │       │       └── Mapper/          # Domain → Response mapping
│   │   │   │       └── ...
│   │   │   ├── Program.cs                       # Composition root, service registration
│   │   │   └── ETOmniverse.Api.csproj
│   │   │
│   │   ├── ETOmniverse.Domain/                  # Pure business logic (no EF/Web/external SDK)
│   │   │   ├── Common/
│   │   │   │   ├── Entity/                      # Base entity classes
│   │   │   │   ├── Model/                       # Shared value objects (Result type)
│   │   │   │   └── Ports/                       # Shared interfaces (IClock)
│   │   │   ├── Identity/                        # Module: Auth + RBAC
│   │   │   │   ├── Entity/                      # User, Role, Permission entities
│   │   │   │   ├── Enum/                        # RoleEnum, PermissionEnum
│   │   │   │   ├── Model/                       # Value objects, DTOs
│   │   │   │   ├── Ports/                       # IUserRepository, IAuthService interfaces
│   │   │   │   ├── UseCase/                     # Login, Register, CreateUser use cases
│   │   │   │   └── Service/                     # Cross-use-case business logic
│   │   │   ├── BatchWorkspace/                  # Module: Batch container
│   │   │   ├── ProductSchedule/                 # Module: Step 0 product scheduling
│   │   │   ├── MdPicks/                         # Module: Step 1 MD picks
│   │   │   ├── Audience/                        # Module: Step 2 audience (read-only)
│   │   │   ├── AiVcr/                           # Module: Step 3 AI VCR ★
│   │   │   ├── MarketingLink/                   # Module: Step 4 marketing links
│   │   │   ├── Schedule/                        # Module: Step 5 broadcast scheduling
│   │   │   ├── Sms/                             # Module: Step 6 SMS (read-only Phase 1)
│   │   │   ├── Collaboration/                   # Module: SignalR co-editing
│   │   │   ├── Notification/                    # Module: In-app + Email notifications
│   │   │   └── Audit/                           # Module: Audit logging
│   │   │
│   │   ├── ETOmniverse.Infrastructure/          # EF Core, Repository, external API clients, Auth
│   │   │   ├── DependencyInjection/
│   │   │   │   └── ServiceCollectionExtensions.cs   # Register all services
│   │   │   ├── Time/
│   │   │   │   └── SystemClock.cs               # Implements IClock
│   │   │   ├── Persistence/
│   │   │   │   ├── EtOmniverseDbContext.cs      # Base DbContext
│   │   │   │   ├── EtOmniverseDbContext.<Module>.cs # Module-specific DbContext config
│   │   │   │   └── Repository/                  # Repository implementations
│   │   │   ├── ExternalServices/                # Wrappers for kie.ai, Gemini, etc.
│   │   │   └── Auth/                            # JWT, user store implementation
│   │   │
│   │   ├── ETOmniverse.Common/                  # Shared utilities (no Domain dependency)
│   │   │   ├── Job/                             # Background job base classes
│   │   │   └── Extensions/                      # Utility methods
│   │   │
│   │   ├── ETOmniverse.Tools.ConfigTool/        # CLI tool for configuration
│   │   │   └── Program.cs
│   │   │
│   │   └── ETOmniverse.sln                      # Solution file
│   │
│   └── frontend/
│       └── ETOmniverse.Web/                     # Vue 3 + TypeScript + Vite SPA
│           ├── src/
│           │   ├── main.ts                      # Entry point
│           │   ├── App.vue                      # Root component
│           │   ├── components/                  # Reusable Vue components
│           │   ├── stores/                      # Pinia state stores
│           │   ├── router/                      # Vue Router (split by module)
│           │   ├── api/                         # Generated OpenAPI client
│           │   └── types/                       # TypeScript type definitions
│           ├── index.html                       # HTML template
│           ├── package.json                     # Dependencies (Vue 3, Vite, TypeScript)
│           ├── vite.config.ts                   # Vite build config
│           ├── tsconfig.json                    # TypeScript config
│           └── ETOmniverse.Web.csproj           # Frontend project (optional .NET reference)
│
├── tests/
│   ├── backend/
│   │   ├── ETOmniverse.Api.Tests/               # WebApplicationFactory integration tests
│   │   ├── ETOmniverse.Domain.Tests/            # Domain UseCase unit tests
│   │   └── ETOmniverse.Infrastructure.Tests/    # Repository + EF integration (Testcontainers)
│   │
│   └── e2e/                                     # Playwright end-to-end tests (frontend + backend)
│
├── Directory.Build.props                        # Global C# project settings (Nullable, ImplicitUsings, etc.)
├── CLAUDE.md                                    # Thin pointer to docs/ for AI tools
├── README.md                                    # Project overview and entry points
└── ETOmniverse.sln                              # Main solution file
```

## Directory Purposes

**`.planning/codebase/`**
- Purpose: GSD working documents (ARCHITECTURE.md, STRUCTURE.md, CONVENTIONS.md, TESTING.md, CONCERNS.md)
- Contains: Markdown analysis files for planning agents
- Key files: ARCHITECTURE.md, STRUCTURE.md

**`docs/`**
- Purpose: Knowledge base, specifications, architecture decisions, patterns
- Contains: Feature specs, ADRs, interviews, retrospectives, glossary, workflow rules
- Key files: `ARCHITECTURE.md` (tech stack overview), `CONVENTIONS.md` (coding rules), `GLOSSARY.md` (vocabulary)

**`src/backend/`**
- Purpose: All .NET 10 backend code (4-layer DDD architecture)
- Contains: ETOmniverse.{Api, Domain, Infrastructure, Common, Tools.ConfigTool}
- Key files: `ETOmniverse.sln`, `Program.cs` (composition root)

**`src/backend/ETOmniverse.Api/`**
- Purpose: HTTP host, Minimal API endpoints, composition root
- Contains: Vertical feature folders under `Features/<Feature>/Adapter/{In,Out}`
- Key files: `Program.cs`, `Features/Identity/Adapter/In/Endpoints/`

**`src/backend/ETOmniverse.Domain/`**
- Purpose: Pure business logic (12 modules following 7-step process)
- Contains: 12 domain modules (Identity, BatchWorkspace, ProductSchedule, ... Audit), each with Entity/Enum/Model/Ports/UseCase/Service
- Key files: `Identity/UseCase/`, `Common/Model/Result.cs`, `Common/Ports/IClock.cs`

**`src/backend/ETOmniverse.Infrastructure/`**
- Purpose: EF Core, Repository implementations, external service clients, dependency injection
- Contains: DbContext, Repository, Time/Auth/ExternalServices implementations
- Key files: `DependencyInjection/ServiceCollectionExtensions.cs`, `Persistence/EtOmniverseDbContext.cs`

**`src/backend/ETOmniverse.Common/`**
- Purpose: Shared utilities with no Domain or Infrastructure dependency
- Contains: Job base classes, Result type, utility extensions
- Key files: `Job/BaseHostedService.cs` (background job base)

**`src/frontend/ETOmniverse.Web/`**
- Purpose: Vue 3 SPA with TypeScript
- Contains: Components, stores (Pinia), router, API client
- Key files: `src/main.ts` (entry point), `src/App.vue`, `package.json`

**`docker/`**
- Purpose: Docker Compose and container configuration
- Contains: docker-compose.yml (app services), docker-compose.infra.yml (EFK/APM), Dockerfile, nginx.conf
- Key files: `docker-compose.yml`, `Dockerfile`, `Dockerfile.frontend`

**`ci/jenkins/`**
- Purpose: CI/CD pipeline configuration
- Contains: Jenkinsfile for multi-stage build (compile, test, build images, push registry)
- Key files: `Jenkinsfile`

**`tests/backend/`**
- Purpose: Backend test suites
- Contains: xUnit tests for Api (WebApplicationFactory), Domain (UseCase unit), Infrastructure (Testcontainers)
- Key files: `ETOmniverse.Api.Tests/`, `ETOmniverse.Domain.Tests/`

**`tests/e2e/`**
- Purpose: End-to-end Playwright tests (browser automation)
- Contains: User journey tests covering frontend + backend happy paths
- Key files: `*.spec.ts` test files

## Key File Locations

**Entry Points:**
- Backend: `src/backend/ETOmniverse.Api/Program.cs` — Composition root, service registration, endpoint mapping
- Frontend: `src/frontend/ETOmniverse.Web/src/main.ts` — Vue app initialization
- Health check: `src/backend/ETOmniverse.Api/Features/Common/Health/HealthEndpointExtensions.cs`

**Configuration:**
- Global C# settings: `Directory.Build.props` — Nullable enable, ImplicitUsings, LangVersion, warnings-as-errors
- Docker: `docker/docker-compose.yml` — API, web, MSSQL services
- Frontend build: `src/frontend/ETOmniverse.Web/vite.config.ts` — Vite dev/build config
- Solution: `ETOmniverse.sln` — Project references (Api → Infrastructure → Domain/Common)

**Core Logic:**
- Domain modules: `src/backend/ETOmniverse.Domain/<Module>/UseCase/` — Business logic (one file per use case)
- Domain services: `src/backend/ETOmniverse.Domain/<Module>/Service/` — Cross-use-case business operations
- Repository interfaces: `src/backend/ETOmniverse.Domain/<Module>/Ports/` — Data access contracts
- Repository implementations: `src/backend/ETOmniverse.Infrastructure/Persistence/Repository/`
- DbContext: `src/backend/ETOmniverse.Infrastructure/Persistence/EtOmniverseDbContext.cs` + module partials

**Testing:**
- Domain unit tests: `tests/backend/ETOmniverse.Domain.Tests/`
- Api integration tests: `tests/backend/ETOmniverse.Api.Tests/` (uses WebApplicationFactory)
- Infrastructure tests: `tests/backend/ETOmniverse.Infrastructure.Tests/` (Testcontainers MSSQL)
- E2E tests: `tests/e2e/` (Playwright)

## Naming Conventions

**Files:**
- C# classes: PascalCase — `LoginUseCase.cs`, `UserRepository.cs`, `CreateUserValidator.cs`
- C# interfaces: PascalCase with I prefix — `IUserRepository.cs`, `IAiEngineClient.cs`
- Vue components: PascalCase with .vue extension — `BatchCard.vue`, `VcrStudio.vue`
- TypeScript files: camelCase (utilities) or PascalCase (components) — `useUser.ts`, `UserStore.ts`
- Database migrations: Timestamp prefix — `20260508_001_CreateUsersTable.cs`

**Directories:**
- Feature folders: PascalCase module name — `Identity/`, `AiVcr/`, `MarketingLink/`
- Adapter subdirs: Always `Adapter/In/` and `Adapter/Out/` (standardized pattern)
- Subfolder names: PascalCase — `Endpoints/`, `Model/`, `Validation/`, `Extensions/`, `Mapper/`

**C# Symbols:**
- Classes/Interfaces: PascalCase — `LoginUseCase`, `IUserRepository`, `User`
- Methods: PascalCase — `CreateUser()`, `GetBatchByIdAsync()`
- Properties: PascalCase — `UserId`, `CreatedAt`
- Local variables: camelCase — `userId`, `batch`, `hasPermission`
- Database tables: snake_case + plural — `users`, `batches`, `user_roles`

**TypeScript Symbols:**
- Components: PascalCase — `BatchCard`, `VcrStudio`
- Functions: camelCase — `useUser()`, `formatDate()`
- Variables: camelCase — `userId`, `isLoading`
- Types/Interfaces: PascalCase — `User`, `BatchWorkspace`
- Constants: UPPER_SNAKE_CASE — `MAX_FILE_SIZE`, `DEFAULT_TIMEOUT`

## Where to Add New Code

**New Feature (Business Logic):**
- Primary code: Create `src/backend/ETOmniverse.Domain/<NewModule>/` with Entity, UseCase, Ports, Service folders
- Endpoints: Create `src/backend/ETOmniverse.Api/Features/<NewModule>/Adapter/{In,Out}/` matching feature slice pattern
- Repository: Implement Port in `src/backend/ETOmniverse.Infrastructure/Persistence/Repository/<NewModule>/`
- DbContext: Add module partial: `src/backend/ETOmniverse.Infrastructure/Persistence/EtOmniverseDbContext.<NewModule>.cs`
- Tests: Create `tests/backend/ETOmniverse.Domain.Tests/<NewModule>/UseCase/` (unit) + `ETOmniverse.Api.Tests/Features/<NewModule>/` (integration)

**New Component/Module (Non-Feature):**
- Utility class: `src/backend/ETOmniverse.Common/Extensions/` or `src/backend/ETOmniverse.Common/Helpers/`
- Shared enum/model: `src/backend/ETOmniverse.Domain/Common/` (if domain-level) or `src/backend/ETOmniverse.Common/` (if pure utility)
- Background job: `src/backend/ETOmniverse.Infrastructure/` with `IHostedService` registration in `ServiceCollectionExtensions.cs`
- External service adapter: `src/backend/ETOmniverse.Infrastructure/ExternalServices/<ServiceName>/`

**Frontend Component/Page:**
- Vue component: `src/frontend/ETOmniverse.Web/src/components/<FeatureOrCategory>/` (PascalCase filename)
- Router: Add route in `src/frontend/ETOmniverse.Web/src/router/<Module>Routes.ts` (split by module to avoid conflicts)
- Store (Pinia): `src/frontend/ETOmniverse.Web/src/stores/<FeatureName>Store.ts`
- Page: `src/frontend/ETOmniverse.Web/src/pages/<FeatureName>/` (if using a router with pages directory)

**Shared Utilities:**
- Backend helpers: `src/backend/ETOmniverse.Common/Extensions/` — String, DateTime, Collection extensions
- Frontend hooks: `src/frontend/ETOmniverse.Web/src/composables/useXxx.ts` (Composition API)
- Types/Models: Backend: `src/backend/ETOmniverse.Domain/<Module>/Model/` | Frontend: `src/frontend/ETOmniverse.Web/src/types/`

## Special Directories

**`src/backend/ETOmniverse.Api/obj/` and `bin/`**
- Purpose: Build artifacts (compiled DLLs, intermediate outputs)
- Generated: Yes (by dotnet build)
- Committed: No (in .gitignore)

**`src/frontend/ETOmniverse.Web/node_modules/`**
- Purpose: npm dependencies installed by pnpm
- Generated: Yes (by pnpm install)
- Committed: No (in .gitignore)

**`docker/compose/` and `docker/scripts/`**
- Purpose: Modular Docker Compose overlays and helper scripts
- Generated: No (source files)
- Committed: Yes

**`docs/decisions/`**
- Purpose: Architecture Decision Records (ADR) — one ADR per file, auto-indexed in DECISIONS.md
- Generated: No (manual ADR creation)
- Committed: Yes
- Pattern: `D-XX-<title>.md` (e.g., `D-10-use-mssql.md`)

**`docs/specs/`**
- Purpose: Feature specifications and SDD (Software Design Documents)
- Generated: No (from design process)
- Committed: Yes

**`scripts/`**
- Purpose: Build, test, deployment automation scripts
- Generated: No (source scripts)
- Committed: Yes
- Example: `build-adr-index.py` auto-generates ADR summary table in DECISIONS.md

---

*Structure analysis: 2026-05-08*
