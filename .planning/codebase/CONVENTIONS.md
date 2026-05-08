# Coding Conventions

**Analysis Date:** 2026-05-08

## Naming Patterns

**C# Files:**
- PascalCase for class/interface/record names: `Result.cs`, `HealthEndpointExtensions.cs`
- Feature files organized hierarchically: `Features/<Feature>/Adapter/In/Endpoints/`, `Features/<Feature>/Model/`
- Domain modules: `Domain/<ModuleName>/` (e.g., `Domain/Identity/`, `Domain/BatchWorkspace/`)

**C# Functions & Methods:**
- PascalCase for all public/private methods: `MapETOmniverseHealthEndpoints()`, `Success()`
- PascalCase for properties: `IsSuccess`, `ErrorCode`, `ErrorMessage`

**C# Variables & Parameters:**
- camelCase for local variables: `builder`, `app`, `errorCode`
- Parameter names match usage context: `errorCode`, `errorMessage`

**Database:**
- snake_case with plural table names: `users`, `batches`, `products`

**TypeScript/Vue:**
- camelCase for functions, variables: `useUser`, `createApp`
- PascalCase for components, types: `App.vue`, `BatchCard.vue`, `HealthEndpointExtensions`

**Modules (aligned to 7-step):**
- Identity, BatchWorkspace, ProductSchedule, MdPicks, Audience, AiVcr, MarketingLink, Schedule, Sms, Collaboration, Notification, Audit

## Code Style

**C# Formatting:**
- Implicit usings enabled globally via `Directory.Build.props`
- Nullable reference types enabled: `<Nullable>enable</Nullable>`
- No null-forgiving operator (`!`) without documented reason
- Records (`record`) for DTO / value objects; `class` for entities
- File-scoped namespaces (implicit in modern .NET)

**TypeScript Formatting:**
- Target: ES2022 (`compilerOptions.target`)
- Module resolution: Bundler (`moduleResolution`)
- Strict mode enabled in `tsconfig.json`
- sourceMap enabled for debugging

**Vue (Composition API):**
- Use `<script setup lang="ts">` in all components
- Pinia for state management (not Vuex)
- API clients auto-generated from OpenAPI (never hand-written)

## Import Organization

**C# Order:**
1. System namespaces
2. Framework namespaces (Microsoft.*)
3. Local project references (ETOmniverse.*)
4. Global using declarations via `ImplicitUsings`

**Example from codebase (`HealthEndpointExtensions.cs`):**
```csharp
using ETOmniverse.Api.Features.Common.Health;
using ETOmniverse.Infrastructure.DependencyInjection;
```

**TypeScript/Vue Order:**
1. Vue framework imports
2. Local app imports
3. Type imports (separate from runtime imports)

**Path Aliases:**
- Frontend Vue setup supports path aliases (configured in `tsconfig.json`)

## Error Handling

**Pattern: Result Type (Domain Model)**

Location: `src/backend/ETOmniverse.Domain/Common/Model/Result.cs`

```csharp
public sealed record Result(bool IsSuccess, string? ErrorCode = null, string? ErrorMessage = null)
{
  public static Result Success() => new(true);
  public static Result Failure(string errorCode, string errorMessage) =>
    new(false, errorCode, errorMessage);
}
```

**Rules:**
- Use Result type for domain operations instead of throwing exceptions
- Exceptions reserved for boundary violations (authentication, security, structural failures)
- API endpoints return `ProblemDetails` (ASP.NET standard) for HTTP error responses
- No `try-catch` blocks for expected errors; use Result pattern

**Structured Logging with CorrelationId:**
- Every log line automatically carries CorrelationId for request tracing
- Logs emitted as JSON via Serilog (configured in Infrastructure layer)

## Logging

**Framework:** Serilog (configured in `ETOmniverse.Infrastructure`)

**Patterns:**
- Structured logging with contextual properties (CorrelationId, UserId, etc.)
- JSON output format for ELK/EFK stack
- Log at boundaries (before/after external service calls, auth decisions)
- Avoid logging sensitive data (passwords, API keys, tokens)

**When to Log:**
- API endpoint entry/exit
- External service latency (wrapped in Infrastructure adapters)
- Business rule violations
- State transitions (workflow steps)

## Comments

**When to Comment:**
- Only document *why*, never *what* the code does
- If the intention is obvious from code, no comment needed
- Link to related spec/ADR when referencing non-obvious decisions

**JSDoc/XML Comments:**
- Not required for well-named functions (names are self-documenting)
- Use only for public APIs that need consumer guidance

**Pattern from Conventions:**
- No comments describing implementation details
- Exceptions: placeholder markers for incomplete features (see Placeholder Rule)

## Function Design

**Size:**
- Single responsibility — one function should do one thing
- Keep functions short enough to understand at a glance (~20-30 lines)

**Parameters:**
- Prefer explicit parameters over builder patterns for simple cases
- Use records/classes to bundle related parameters when count > 3
- Always use named parameters for Result type: `Result.Failure(errorCode: "...", errorMessage: "...")`

**Return Values:**
- Return Result type from domain UseCase
- Return records (DTOs) from query repositories
- Use nullable types (enable strict null checks) instead of exceptions for "not found"

## Module Design

**Feature Slice Structure (Api Layer):**

Location: `src/backend/ETOmniverse.Api/Features/<Feature>/`

```
Adapter/In/Endpoints/          # Minimal API endpoint registration
Adapter/In/Model/              # Request / response DTO
Adapter/In/Validation/         # FluentValidation rules
Adapter/In/Extensions/         # Request → Domain command/query mapper
Adapter/Out/Model/             # Query/read model (when CQRS exception needed)
Adapter/Out/Mapper/            # Domain → read model mapper
```

**Domain Module Structure (Domain Layer):**

Location: `src/backend/ETOmniverse.Domain/<Module>/`

```
Entity/                        # POCO entities
Enum/                          # Enumerations
Model/                         # Value objects, domain DTOs
Ports/                         # Interfaces (contracts for Infrastructure)
UseCase/                       # Business logic (one file = one use case)
Service/                       # Cross-use-case domain logic
```

**Dependency Rules:**
- Domain never depends on EF Core / ASP.NET / external SDKs
- All external dependencies via Ports (interfaces)
- Infrastructure implements Ports
- Api is composition root (DI container setup)
- Common layer contains only utilities (no Domain/Infrastructure dependencies)

**Exports (Barrel Files):**
- Use when grouping related public types
- Avoid deep nesting (keep imports readable)

## Null Safety

**Rules:**
- `Nullable` enabled in all projects
- Disable null-forgiving operator (`!`) by default
- If `!` is necessary, include comment explaining why
- Optional values represented as `T?` not `null` sentinel

**Pattern:**
```csharp
// Good
public string? OptionalField { get; set; }

// Bad (avoid)
public string OptionalField { get; set; } = null!;
```

## Immutability Preference

**Use Records for:**
- Data Transfer Objects (DTOs)
- Value Objects (domain concept with no identity)
- Result types

**Use Classes for:**
- Entities (have identity, mutable state)
- Services
- Use Cases

## Async/Await

**Rule:** Avoid synchronous I/O

- All I/O operations must be async
- Database queries via EF Core async methods (`.ToListAsync()`, `.FirstOrDefaultAsync()`)
- External API calls always async
- No `.Result` or `.Wait()` blocking

## Testing Conventions (See TESTING.md)

At feature implementation time, enforce:
- First endpoint of a feature must include: Unit test for UseCase + API WebApplicationFactory happy-path test
- Validation tests for FluentValidation rules
- No testing of infrastructure without Testcontainers (for DB-dependent code)

---

*Convention analysis: 2026-05-08*
