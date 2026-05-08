# Architecture

**Analysis Date:** 2026-05-08

## Pattern Overview

**Overall:** Onion/Hexagonal (4-layer DDD) + Feature Slice

**Key Characteristics:**
- Domain layer is pure (no EF Core, Web, or external SDK dependencies)
- Infrastructure implements Ports interfaces from Domain
- Api is composition root with Minimal API endpoints and vertical feature folders
- Cross-module communication via Ports or domain events (not direct Service references)
- Modular by business domain (12 modules following 7-step process)

## Layers

**Domain (`ETOmniverse.Domain`):**
- Purpose: Pure business logic, independent of infrastructure and presentation
- Location: `src/backend/ETOmniverse.Domain/`
- Contains: 12 business modules with Entity/Enum/Model/Ports/UseCase/Service folders
- Depends on: ETOmniverse.Common only (for IClock, Result type)
- Used by: Infrastructure (implements Ports), Api (calls UseCases)

**Infrastructure (`ETOmniverse.Infrastructure`):**
- Purpose: Implementation of Domain Ports, EF Core DbContext, external API clients, background job hosting
- Location: `src/backend/ETOmniverse.Infrastructure/`
- Contains: Repository implementations, DbContext configuration, external service adapters, ServiceCollectionExtensions
- Depends on: Domain, Common
- Used by: Api (injected via ServiceCollectionExtensions)

**Api (`ETOmniverse.Api`):**
- Purpose: Web host, HTTP endpoint binding, request/response mapping, composition root
- Location: `src/backend/ETOmniverse.Api/`
- Contains: Features/<Feature>/Adapter/{In,Out}, Program.cs with service registration
- Depends on: Infrastructure, Domain, Common
- Used by: External HTTP clients, frontend

**Common (`ETOmniverse.Common`):**
- Purpose: Shared utilities, base classes for background jobs, no business logic
- Location: `src/backend/ETOmniverse.Common/`
- Contains: Result type, Job base classes, utility functions
- Depends on: Nothing (no Domain/Infrastructure dependency)
- Used by: Domain, Infrastructure, Api

**Frontend (`ETOmniverse.Web`):**
- Purpose: Vue 3 SPA with TypeScript, communicates with Api via HTTP
- Location: `src/frontend/ETOmniverse.Web/`
- Contains: Vue components, TypeScript code, Vite build config
- Depends on: Backend Api endpoints (via fetch/axios from OpenAPI client)
- Used by: Browser clients

## Data Flow

**Command/Write Flow (e.g., Create/Update):**

1. Http request arrives at Endpoint → `src/backend/ETOmniverse.Api/Features/<Feature>/Adapter/In/Endpoints/`
2. Endpoint validates with FluentValidator → `Adapter/In/Validation/`
3. Endpoint maps request to domain Command via extension → `Adapter/In/Extensions/`
4. UseCase executes business logic → `ETOmniverse.Domain/<Module>/UseCase/`
5. UseCase calls Domain Service or Port interface
6. Infrastructure implements Port, calls Repository or external API → `ETOmniverse.Infrastructure/`
7. Repository uses DbContext (EF Core) to persist
8. UseCase returns Result (success or error code/message)
9. Endpoint maps domain result to response model → `Adapter/Out/Model/` + `Adapter/Out/Mapper/`
10. Endpoint returns ProblemDetails or response JSON

**Query/Read Flow:**

1. Http request arrives at Endpoint
2. Endpoint validates request parameters
3. Endpoint calls Query handler (may use read repository directly, bypassing UseCase)
4. Query handler reads from Repository
5. Repository returns read model projection if needed
6. Response returned to client

**Background Job Flow:**

1. Job registered as IHostedService in Program.cs (Quartz.NET in host process, not separate Worker)
2. Job executes periodic/scheduled work
3. Job calls Domain Services via Ports
4. Job updates state via Repository/Infrastructure

**State Management:**

- Transactional source of truth: **MSSQL** (Phase 1 locked, via EF Core)
- Read models/projections: Separate from command side only where explicitly documented as CQRS exception
- External state: kie.ai (4 AI engines), Gemini, audience data service, SMS dispatch service
- Caching: Not yet implemented (Phase 1 foundation)

## Key Abstractions

**UseCase:**
- Purpose: Encapsulates single business action (one file = one use case)
- Examples: `src/backend/ETOmniverse.Domain/Identity/UseCase/` contains specific actions like login, user creation
- Pattern: Takes input (command/query), validates via Domain logic, calls Ports, returns Result

**Port (Interface):**
- Purpose: Contract between Domain and Infrastructure (dependency inversion)
- Examples: `src/backend/ETOmniverse.Domain/<Module>/Ports/` defines interfaces like `IUserRepository`, `IAiEngineClient`
- Pattern: Domain calls port, Infrastructure implements port

**Entity:**
- Purpose: POCO with identity and mutable state
- Examples: `src/backend/ETOmniverse.Domain/<Module>/Entity/` contains User, Batch, VcrProject entities
- Pattern: No business logic embedded; UseCase orchestrates entities

**Value Object:**
- Purpose: Immutable domain concept without identity
- Examples: `src/backend/ETOmniverse.Domain/<Module>/Model/` contains DTOs and value types
- Pattern: Declared as `record` in C#

**Repository:**
- Purpose: Abstraction over data access
- Examples: Infrastructure implements `IUserRepository`, `IBatchRepository` from Domain Ports
- Pattern: Repository is defined in Ports, implemented in Infrastructure with EF Core

**Feature Slice:**
- Purpose: Vertical module containing adapter (in/out), models, validation for single HTTP feature
- Examples: `src/backend/ETOmniverse.Api/Features/Identity/Adapter/In/Endpoints/` for login, register endpoints
- Pattern: Request → Validation → Extension (request-to-command mapping) → UseCase → Mapper → Response

## Entry Points

**Backend HTTP Entry Point:**
- Location: `src/backend/ETOmniverse.Api/Program.cs`
- Triggers: Application startup, listens on configured port
- Responsibilities: 
  - Builder setup (OpenApi, HealthChecks, ServiceCollectionExtensions)
  - Map endpoints (Health checks, Features endpoints via `MapETOmniverseHealthEndpoints()`)
  - Run web server

**Health Check Endpoint:**
- Location: `src/backend/ETOmniverse.Api/Features/Common/Health/HealthEndpointExtensions.cs`
- Triggers: GET /health request
- Responsibilities: Check database and external service connectivity

**Feature Endpoints:**
- Location: `src/backend/ETOmniverse.Api/Features/<Feature>/Adapter/In/Endpoints/`
- Triggers: HTTP requests (GET/POST/PUT/DELETE)
- Responsibilities: Parse request, validate, call UseCase, return response

**Frontend Entry Point:**
- Location: `src/frontend/ETOmniverse.Web/src/main.ts`
- Triggers: Browser load of index.html
- Responsibilities: Bootstrap Vue 3 app, mount to #app DOM element

## Error Handling

**Strategy:** Result type (not exceptions for business errors)

**Patterns:**
- Domain UseCase returns `Result(success: bool, errorCode?: string, errorMessage?: string)`
- Infrastructure throws exceptions only for infrastructure failures (DB unavailable, network timeout)
- Api Endpoint catches infrastructure exceptions, converts to ProblemDetails response
- Client receives either: `200 OK` with success result, or `400/500` ProblemDetails JSON

**Result Type Location:** `src/backend/ETOmniverse.Domain/Common/Model/Result.cs`

**No null-forgiving operator (!):** Nullable enable enforces explicit null checks

## Cross-Cutting Concerns

**Logging:** 
- Serilog JSON structured logging
- CorrelationId auto-attached to every log line
- Location: Infrastructure log setup in `ServiceCollectionExtensions.cs`

**Validation:**
- FluentValidation in `Adapter/In/Validation/` (Http layer, not Domain)
- Domain trusts inputs from validated Endpoints
- External inputs at boundary get sanitized (prevent SQL injection via EF parameterization)

**Authentication:**
- Phase 1: Local user store with RBAC (no AD/LDAP)
- Identity module handles user/role/permission logic: `src/backend/ETOmniverse.Domain/Identity/`
- OAuth/JWT token management in Infrastructure

**Dependency Injection:**
- All services registered in `ETOmniverse.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`
- Api's Program.cs calls `AddETOmniverseInfrastructure(builder.Configuration)` to chain registrations
- DbContext uses partial classes per module: `EtOmniverseDbContext.<Module>.cs`

**Background Jobs:**
- Hosted in API process (not separate Worker pod)
- Quartz.NET for scheduling, registered as IHostedService
- Job implementations call Domain Services via Ports

---

*Architecture analysis: 2026-05-08*
