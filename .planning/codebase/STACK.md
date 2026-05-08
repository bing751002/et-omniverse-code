# Technology Stack

**Analysis Date:** 2026-05-08

## Languages

**Primary:**
- **C# 12** (with .NET 10) — Backend API, business logic, domain models
- **TypeScript 5.9** — Frontend application logic and type safety
- **YAML** — Docker Compose configuration
- **HTML / CSS** — Static assets (produced from Vue builds)

**Secondary:**
- **Shell (PowerShell/Bash)** — CI/CD scripts, Docker entry points

## Runtime

**Environment:**
- **.NET 10.0.102** (SDK + Runtime) — Backend runtime, specified in `global.json`
  - ASP.NET Core 10 — Web framework for API
  - C# language version: `latest` (auto-update)
- **Node 22 Alpine** — Frontend build environment (Docker image `node:22-alpine`)
- **On-prem VM (Docker Compose)** — Production deployment target

**Package Manager:**
- **NuGet** (implicit via .NET SDK) — C# / .NET dependencies
  - Lockfile: `Directory.Packages.props` pattern not detected; using implicit package resolution per csproj
  - No explicit `packages.lock.json`
- **pnpm** (inferred; package.json exists) — Frontend JavaScript dependencies
  - Lockfile: Not committed (only `package.json` present in repo; `pnpm-lock.yaml` likely gitignored)

## Frameworks

**Core:**
- **ASP.NET Core 10** — Web API host, Minimal APIs, routing, health checks
  - `Microsoft.AspNetCore.OpenApi` v10.0.2 — OpenAPI documentation and Swagger
  - Framework reference via `<FrameworkReference Include="Microsoft.AspNetCore.App" />`

**Data & ORM:**
- **Entity Framework Core 10** (implicit dependency from ASP.NET Core)
  - Used in `ETOmniverse.Infrastructure` layer
  - Target: MSSQL Server 2022
  - Connection string: Environment-configured, not hardcoded

**Database:**
- **MSSQL Server 2022** — Primary data store
  - Image: `mcr.microsoft.com/mssql/server:2022-latest`
  - Port: `1433` (internal Docker network + exposed to host)
  - Volume: `et_mssql_data` (persistent)
  - Credentials: `sa` account with `MSSQL_SA_PASSWORD` env var (required)
  - Health check: `sqlcmd` ping every 10s

**Frontend Framework:**
- **Vue 3.5** — Component framework
  - `@vitejs/plugin-vue` v6.0.0 — Vue SFC compiler plugin
  - Language: TypeScript (`<script setup lang="ts">`)
  - State management: Pinia (not Vuex; inferred from CONVENTIONS.md)
  - Composition API — Functional component pattern

**Build/Dev:**
- **Vite 7.0.0** — Frontend bundler and dev server
  - Config: `src/frontend/ETOmniverse.Web/vite.config.ts`
  - Dev server: Port 5173 (or 5173 in Docker)
  - Build output: `dist/` (optimized static assets)
- **vue-tsc 3.0.0** — Vue TypeScript type checker
  - Run via `npm run build` (type-check before Vite build)
- **Dotnet CLI** — Backend build orchestration
  - `dotnet restore` → `dotnet publish` in multi-stage Docker build
  - Published to Release configuration

**Testing:**
- **xUnit** — Backend unit testing framework (C#)
  - Config file: Not yet committed; awaiting test project setup
  - Test projects: `tests/backend/ETOmniverse.Domain.Tests/`, `tests/backend/ETOmniverse.Api.Tests/`
- **Testcontainers MSSQL** — Integration test database (Docker containers)
  - Brings up ephemeral MSSQL for repo + EF Core tests
- **WebApplicationFactory** — ASP.NET Core API integration testing
  - In-memory host for endpoint/feature tests
- **Playwright** — E2E browser testing (Vue + API)
  - Not yet active; awaiting first E2E phase

**Background Jobs:**
- **Quartz.NET** (implicit; referenced in ARCHITECTURE.md)
  - Runs within API host (no separate Worker process)
  - Scheduled job execution + background task registration

**Caching:**
- **Redis 7 Alpine** — In-memory cache and session store
  - Image: `redis:7-alpine`
  - Port: `6379`
  - Volume: `et_redis_data` (persistent)
  - Health check: `redis-cli ping` every 10s
  - Used for: Session state, distributed caching (inferred; not yet configured in code)

**Logging:**
- **Serilog** — Structured logging framework
  - JSON output format (structured logs)
  - CorrelationId propagation per request
- **Seq 2024.3** — Log ingestion and visualization
  - Docker image: `datalust/seq:2024.3`
  - Port: `5341` (Seq UI)
  - Volume: `et_seq_data`
  - Current phase: Seq ingests logs; EFK (Elasticsearch + Fluent Bit + Kibana) deferred to P1.6

## Key Dependencies

**Critical (explicitly declared):**
- `Microsoft.AspNetCore.OpenApi` v10.0.2 — API documentation generation (declared in `ETOmniverse.Api.csproj`)

**Infrastructure (implicit from ASP.NET Core):**
- `Microsoft.EntityFrameworkCore` v10.x — ORM for MSSQL
- `Microsoft.AspNetCore.Authentication.JwtBearer` (likely present; Identity module needs it)
- FluentValidation — Request validation (inferred from feature slice convention; not yet declared)
- Azure.Identity / Google.Cloud.Client (not yet visible; kie.ai + Gemini clients likely use HTTP directly)

**Framework Dependencies:**
- `Microsoft.AspNetCore.SignalR` — Real-time features (Collaboration module)
- `Quartz` — Scheduled job library

**Not Present (Deferred):**
- `Qdrant.Client` — Vector DB (Phase 2/RAG; explicitly deferred)
- Active Directory / LDAP clients (Phase 2; local user store Phase 1)
- `StackExchange.Redis` (Redis client; Phase 1 uses Docker but code not yet present)

## Configuration

**Environment:**
- **.env file(s)** — Present but contents not exposed
  - Required vars: `MSSQL_SA_PASSWORD`, `ACCEPT_EULA` (SQL Server specific)
  - Inferred additional vars for API endpoints (kie.ai, Gemini, internal services) — not yet committed
- **appsettings.json** — Base application configuration
  - Location: `src/backend/ETOmniverse.Api/appsettings.json`
  - Currently minimal (Logging + AllowedHosts)
  - No external service keys hardcoded
- **appsettings.Development.json** — Local development overrides
  - Location: `src/backend/ETOmniverse.Api/appsettings.Development.json`
  - Currently mirrors base (minimal)
- **user-secrets** — Local secrets during development
  - Invoked via `dotnet user-secrets` (ASP.NET Core built-in)
  - Stores credentials locally without committing to repo

**Build:**
- **Directory.Build.props** (`Directory.Build.props`)
  - Central configuration for all .csproj files
  - Settings:
    - `<Nullable>enable</Nullable>` — Enforce nullable reference types
    - `<ImplicitUsings>enable</ImplicitUsings>` — Use global using directives
    - `<LangVersion>latest</LangVersion>` — Auto-update C# language version
    - `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>` — Warnings logged, not blocking
    - Dev Container detection for local vs. Docker artifact paths

- **tsconfig.json** — TypeScript compiler options (Frontend)
  - Location: `src/frontend/ETOmniverse.Web/tsconfig.json`
  - Target: ES2022
  - Module: ESNext
  - Resolution: Bundler
  - Strict mode: enabled
  - JSX: preserve (Vue handles JSX as templates)
  - Source maps enabled

- **.csproj files** — Per-project references
  - `ETOmniverse.Api.csproj` — Web host, declares `Microsoft.AspNetCore.OpenApi`
  - `ETOmniverse.Infrastructure.csproj` — EF Core context, repositories, external API clients
  - `ETOmniverse.Domain.csproj` — Pure business logic, no external dependencies
  - `ETOmniverse.Common.csproj` — Shared utilities (time, errors, Job base classes)
  - All target `net10.0`

## Platform Requirements

**Development:**
- **.NET 10 SDK** (10.0.102+) — Local compilation, testing, hot reload
  - Or: Dev Container with Docker (automatic dotnet environment)
- **Node 22+** — Frontend development
  - Or: Dev Container with Node Alpine
- **Docker + Docker Compose** — Local multi-service environment
  - Services: MSSQL, Redis, Seq (logging), API, Web
- **MSSQL Server 2022** — Via Docker container or installed locally
  - Port 1433 required
- **Redis** — Cache during dev
  - Port 6379
- **pnpm** (or npm) — Frontend dependency management
  - Suggested over npm for workspace consistency

**Production:**
- **Docker + Docker Compose** — Single-machine on-prem deployment
  - API container (.NET 10 aspnet image)
  - Web container (nginx:1.27-alpine + Vue build)
  - MSSQL 2022 container
  - Redis 7 container
  - Seq 2024.3 container (logging)
  - Fluent Bit + Elasticsearch + Kibana (P1.6 onwards)
- **Persistent volumes** for data:
  - `et_mssql_data` — Database files
  - `et_redis_data` — Cache data
  - `et_seq_data` — Log storage
  - `media-data` — Video/media files (host mount expected at `/data/media/{batch-id}/{product-id}/`)
- **Environment variables** — Passed to containers via `.env` or docker-compose overrides
  - `MSSQL_SA_PASSWORD` (required)
  - `ASPNETCORE_ENVIRONMENT` (set to production)
  - API connection strings and external service endpoints
- **Network** — Docker Compose internal network for inter-service communication
  - API reaches MSSQL via `mssql:1433` (Docker DNS)
  - External clients reach API via `localhost:5080` or reverse proxy

## Deployment Topology

```
Host VM (On-Premise)
├── Docker Daemon
│   ├── et-omniverse-api
│   │   └── ASP.NET Core 10 (api:5080 → container:8080)
│   ├── et-omniverse-web
│   │   └── nginx:1.27-alpine (web:5173 → container:80)
│   ├── et-omniverse-mssql
│   │   └── MSSQL Server 2022 (port 1433)
│   ├── et-omniverse-redis
│   │   └── Redis 7 (port 6379)
│   └── et-omniverse-seq
│       └── Seq 2024.3 (port 5341)
├── Volumes
│   ├── et_mssql_data
│   ├── et_redis_data
│   ├── et_seq_data
│   └── /data/media/ (host mount for VCR video files)
└── Network: et-omniverse (internal Docker network)
```

---

*Stack analysis: 2026-05-08*
