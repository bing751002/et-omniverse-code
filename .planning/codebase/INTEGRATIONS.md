# External Integrations

**Analysis Date:** 2026-05-08

## APIs & External Services

**AI Video Generation:**
- **kie.ai** — AI VCR (video content) production
  - Supported engines: sora2, kling3, seedance2, wan27 (multi-engine support is core feature)
  - Used in: `ETOmniverse.Domain/AiVcr/` module
  - SDK/Client: Not yet visible; likely HTTP wrapper (CONVENTIONS.md requires external API client wrapper with auto-logging of latency)
  - Auth: API key (environment variable, not yet configured in codebase)
  - Phase: P1.0 — implemented

**LLM / Text Generation:**
- **Google Gemini** — AI narration/voiceover text generation (口白)
  - Used for: Generating script text for video voiceovers
  - SDK/Client: `Google.Cloud.ArtificialIntelligence.V1` or direct HTTP (not yet declared in .csproj)
  - Auth: API key (environment variable)
  - Phase: P1.0 — implemented

**Internal Services (On-Premise):**
- **大數據受眾 (Big Data Audience Service)** — Audience segment calculation
  - Used in: `ETOmniverse.Domain/Audience/` (read-only Phase 1)
  - Provides audience demographic/behavioral segments
  - Auth: Internal service discovery or direct endpoint
  - Integration: Likely REST HTTP call or message queue
  - Phase: P1.0

- **派報自動化 (Dispatch Automation Service)** — Trigger content dispatch
  - Used in: `ETOmniverse.Domain/Schedule/` (Step 5)
  - Pushes scheduled content to channels (SMS, email, etc.)
  - Auth: Internal service credential or API key
  - Phase: P1.0

**Communication / Notifications:**
- **SMTP** — Email notification delivery
  - Used in: `ETOmniverse.Domain/Notification/` module
  - Client: `System.Net.Mail.SmtpClient` or MailKit (not yet declared)
  - Config: SMTP host, port, credentials (environment variables)
  - Phase: P1.0

## Data Storage

**Databases:**
- **MSSQL Server 2022** — Primary application database
  - Connection: `ConnectionStrings__Default` (environment variable or appsettings)
    - Format: `Server=mssql,1433;Database=et_omniverse;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;`
    - Dev connection: Points to Docker service `mssql:1433`
    - Prod connection: Configured per environment
  - Client: Entity Framework Core 10
  - Usage: All domain entities (Identity, BatchWorkspace, ProductSchedule, MdPicks, Audience, AiVcr, MarketingLink, Schedule, Sms, Collaboration, Notification, Audit)
  - Schema: 12 module-based domain models (not yet created; currently P1.0 foundation only)

**File Storage:**
- **Local filesystem** (Docker host mount)
  - Media path: `/data/media/{batch-id}/{product-id}/{vcr-id}.mp4` (convention, awaiting implementation)
  - Purpose: Store generated AI VCR video files
  - Volume mapping: `media-data` (Docker Compose named volume or host mount)
  - Backup: Via host-level snapshot/backup procedures (not automated in Compose)

**Caching:**
- **Redis 7** — Session and distributed cache
  - Connection: `localhost:6379` (Docker service `redis:6379`)
  - Client: `StackExchange.Redis` (not yet declared; expected Phase 1 when sessions activated)
  - Purpose: Session storage, request-level caching, real-time collaboration state (SignalR)
  - Persistence: RDB snapshot to `et_redis_data` volume

## Authentication & Identity

**Auth Provider:**
- **Local User Store** — Custom in-app authentication (Phase 1)
  - Implementation: `ETOmniverse.Domain/Identity/` module
  - Approach: Password hash + RBAC roles stored in MSSQL
  - No external provider (Phase 1 scope)
  - JWT Bearer tokens: Implied by CONVENTIONS.md reference to `Microsoft.AspNetCore.Authentication.JwtBearer` (not yet declared)

**Authorization Model:**
- **RBAC (Role-Based Access Control)**
  - Scoped: Organization, Business Unit (事業群), Department (部門), External Company (供應商)
  - Details: See `docs/ACCESS-CONTROL.md`
  - Implementation: `ETOmniverse.Domain/Identity/` with Ports/UseCase pattern
  - Middleware: Injected via ASP.NET Core authentication pipeline

**Deferred (Phase 2):**
- **AD/LDAP** — Company directory integration (Decision D14)
  - Not implemented in P1.0
  - Will replace local user store when activated
  - Client library: Not yet declared

## Monitoring & Observability

**Error Tracking:**
- None detected — Structured errors via `Result<T>` type pattern (ARCHITECTURE.md)
  - No third-party error tracking (Sentry, Rollbar, etc.)
  - Errors logged to Seq

**Logs:**
- **Serilog** — Structured logging
  - Output format: JSON (structured)
  - Sink: Seq (`http://seq:5341` — Docker Compose)
  - Correlation ID: Auto-propagated per HTTP request (CONVENTIONS.md)
  - Log levels: `Information` default, `Warning` for ASP.NET Core internals
  - Rotation: Managed by Seq (retention policy configured in Seq UI, not code)

- **Seq 2024.3** — Log aggregation and visualization
  - Endpoint: `http://localhost:5341` (dev) or configured endpoint (prod)
  - Data retention: Configurable via Seq dashboard
  - Phase: P1.0 (foundation); EFK upgrade in P1.6

**APM / Distributed Tracing:**
- **Not deployed** — ARCHITECTURE.md: "no distributed tracing"
  - CorrelationId used for request correlation (not full distributed tracing)
  - APM (Application Performance Monitoring) stack deferred to P1.6
  - OpenTelemetry integration: Not yet active

## CI/CD & Deployment

**Hosting:**
- **On-Premise VM** — Single machine Docker Compose deployment
  - No cloud provider (AWS, Azure, GCP)
  - Customer-owned infrastructure
  - Single-node (no Kubernetes or orchestration)

**CI Pipeline:**
- **Jenkins** — Build and deployment orchestration (planned; not yet active)
  - Configuration: `ci/jenkins/` (skeleton; awaiting full pipeline definition)
  - Stages: Build, Test, Publish Docker images, Deploy to prod
  - Trigger: Git push to relevant branches (`feat/*` branches for feature work)

**Docker Registry:**
- Not yet specified — Images likely built locally or pushed to Harbor (mentioned in ARCHITECTURE.md as not-yet-active)
  - Future: Private registry for production image distribution

## Environment Configuration

**Required Environment Variables (Dev & Prod):**

| Variable | Purpose | Example / Notes |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Execution environment | `Development` (dev) / `Production` (prod) |
| `ASPNETCORE_URLS` | API host binding | `http://+:8080` (Docker) / `https://0.0.0.0:443` (prod) |
| `ConnectionStrings__Default` | MSSQL connection | `Server=mssql,1433;Database=...;User Id=sa;Password=...;` |
| `MSSQL_SA_PASSWORD` | SQL Server admin password | (stored securely; required) |
| `ACCEPT_EULA` | SQL Server license agreement | `Y` (Docker startup) |
| `KIE_API_KEY` | kie.ai authentication | (not yet exposed; expected) |
| `GEMINI_API_KEY` | Gemini API key | (not yet exposed; expected) |
| `REDIS_CONNECTION` | Redis connection string | `redis:6379` (dev) / configurable (prod) |
| `SEQ_URL` | Serilog Seq sink endpoint | `http://seq:5341` (dev) |

**Configuration Files:**
- `appsettings.json` — Base config (committed)
- `appsettings.Development.json` — Dev overrides (committed)
- `appsettings.Production.json` — (Not yet created; to be added pre-production)
- `.env` / `.env.local` — Secrets (gitignored; Docker Compose uses these)

**Secrets Storage:**
- **Local Development:** `dotnet user-secrets` (project-local encrypted store)
- **Docker / CI:** Environment variables passed via `docker-compose.yml` or CI secrets manager (Jenkins credentials)
- **Production:** Environment variables from host or orchestration system (Kubernetes/Docker Secrets if upgraded)

## Webhooks & Callbacks

**Incoming:**
- None detected — No external system currently calls ET-Omniverse webhooks
- Future: Dispatch automation may receive webhook callbacks (Phase 1 awaiting spec)

**Outgoing:**
- **Event-based triggers** (planned; not yet active)
  - Batch creation → internal Dispatch Service call
  - VCR completion → Notification Service (email, SMS)
  - Schedule executed → Dispatch Service

**SignalR** (Real-Time Collaboration):
- Hub endpoints: Not yet implemented
  - Will handle co-editing real-time sync (Collaboration module)
  - Uses WebSocket upgrade from HTTP
  - Connected clients broadcast state changes

## Data Flow: Request Lifecycle

1. **HTTP Request** → Minimal API endpoint (ETOmniverse.Api/Features/\*/Adapter/In/Endpoints/)
2. **Validation** → FluentValidation (Adapter/In/Validation/)
3. **Request Mapping** → Convert HTTP model to Domain command/query (Adapter/In/Extensions/)
4. **UseCase Invocation** → Domain business logic (Domain/\*/UseCase/)
5. **Database Access** → EF Core Repository queries (Infrastructure/Repositories/)
6. **External API Call** (if needed) → Wrapped HTTP client with logging (Infrastructure/ExternalApis/)
7. **Response Mapping** → Domain model to HTTP response model (Adapter/Out/Mapper/)
8. **Logging** → Serilog with CorrelationId (Infrastructure/Logging/)
9. **HTTP Response** → ProblemDetails (errors) or JSON body

---

*Integration audit: 2026-05-08*
