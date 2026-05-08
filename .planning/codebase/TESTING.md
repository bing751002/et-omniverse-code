# Testing Patterns

**Analysis Date:** 2026-05-08

## Test Framework

**Framework Stack:**

| Layer | Framework | Status |
|-------|-----------|--------|
| Unit | xUnit | **Configured** (v2.9.3) |
| Integration | Testcontainers MSSQL | **Planned** (not yet in csproj) |
| API | WebApplicationFactory | **Planned** (ASP.NET built-in) |
| E2E | Playwright | **Planned** (not yet configured) |

**Current Configuration:**

Test projects use xUnit with coverage via coverlet:

**Domain.Tests csproj** (`tests/backend/ETOmniverse.Domain.Tests/ETOmniverse.Domain.Tests.csproj`):
```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
<PackageReference Include="coverlet.collector" Version="6.0.4" />
<Using Include="Xunit" />
```

**Api.Tests csproj** (same structure, mirrors Domain.Tests)

**Run Commands:**

```bash
# All tests (via dotnet test)
dotnet test ETOmniverse.sln

# Specific test project
dotnet test tests/backend/ETOmniverse.Domain.Tests/ETOmniverse.Domain.Tests.csproj

# Watch mode (via dotnet watch)
dotnet watch test

# Coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## Test File Organization

**Location Pattern:**
- Co-located with source: `tests/backend/<Project>.Tests/`
- Mirror source structure: for `src/backend/ETOmniverse.Domain/Identity/`, tests in `tests/backend/ETOmniverse.Domain.Tests/Identity/`

**Naming:**
- Source: `HealthEndpointExtensions.cs`
- Test: `HealthEndpointExtensionsTests.cs`
- Class name: `[SourceClassName]Tests`

**Current State (Placeholder):**
- `tests/backend/ETOmniverse.Domain.Tests/UnitTest1.cs` — empty placeholder
- `tests/backend/ETOmniverse.Api.Tests/UnitTest1.cs` — empty placeholder

**Structure to Apply:**

```
tests/
├── backend/
│   ├── ETOmniverse.Domain.Tests/
│   │   ├── UnitTest1.cs (placeholder - remove after first real test)
│   │   ├── Common/
│   │   │   └── Model/
│   │   │       └── ResultTests.cs
│   │   └── <Module>/
│   │       ├── UseCase/
│   │       │   └── <UseCaseName>Tests.cs
│   │       └── Service/
│   │           └── <ServiceName>Tests.cs
│   └── ETOmniverse.Api.Tests/
│       ├── UnitTest1.cs (placeholder - remove after first real test)
│       └── Features/
│           └── <Feature>/
│               ├── Endpoints/
│               │   └── <EndpointName>Tests.cs
│               └── Model/
│                   └── <ModelName>ValidationTests.cs
└── integration/ (future)
    └── ETOmniverse.Integration.Tests/
```

## Test Structure

**xUnit Test Class Pattern:**

```csharp
namespace ETOmniverse.Domain.Tests.Common.Model;

public class ResultTests
{
    [Fact]
    public void Success_ReturnsSuccessResult()
    {
        // Arrange
        
        // Act
        var result = Result.Success();
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorCode);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Failure_ReturnsFailureResultWithDetails()
    {
        // Arrange
        string expectedCode = "INVALID_INPUT";
        string expectedMessage = "Input validation failed";
        
        // Act
        var result = Result.Failure(expectedCode, expectedMessage);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    [Theory]
    [InlineData("ERROR_001", "Something went wrong")]
    [InlineData("ERROR_002", "Another failure")]
    public void Failure_AcceptsVariousCodes(string code, string message)
    {
        // Arrange & Act
        var result = Result.Failure(code, message);
        
        // Assert
        Assert.Equal(code, result.ErrorCode);
        Assert.Equal(message, result.ErrorMessage);
    }
}
```

**Patterns:**
- **Arrange-Act-Assert (AAA):** Structure every test with clear sections
- **One assertion focus:** Each test verifies one behavior (though multiple asserts on same object ok)
- **Descriptive names:** `[Fact]` method name explains what's being tested: `Success_ReturnsSuccessResult`
- **InlineData for parameterized tests:** Use `[Theory]` + `[InlineData(...)]` to test multiple inputs

## Mocking

**Framework:** **Moq** (not yet in csproj — add when first integration test needed)

**When to Add Moq:**
- First time you need to mock IRepository / IExternalService
- For UseCase tests that depend on Ports

**Add to test csproj:**
```xml
<PackageReference Include="Moq" Version="4.20.70" />
```

**Mocking Pattern (Example - use when real code exists):**

```csharp
[Fact]
public async Task GetUser_CallsRepositoryAndReturnsUser()
{
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var testUser = new User { Id = 1, Name = "Test" };
    mockRepo
        .Setup(r => r.GetUserAsync(1))
        .ReturnsAsync(testUser);
    
    var useCase = new GetUserUseCase(mockRepo.Object);
    
    // Act
    var result = await useCase.ExecuteAsync(1);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(testUser.Id, result.Id);
    mockRepo.Verify(r => r.GetUserAsync(1), Times.Once);
}
```

**What to Mock:**
- External service clients (APIs, databases, message queues)
- Repository interfaces (when testing UseCase logic, not repository implementation)
- Time providers (IClock interface for deterministic testing)

**What NOT to Mock:**
- Domain value objects (records, simple data structures)
- Domain services with pure logic
- Result type behavior

## Fixtures and Factories

**Test Data Pattern:**

Location: Create `tests/backend/ETOmniverse.Domain.Tests/Common/Fixtures/` for shared test data

```csharp
// tests/backend/ETOmniverse.Domain.Tests/Common/Fixtures/UserFixtures.cs
public static class UserFixtures
{
    public static User CreateValidUser(
        int id = 1,
        string name = "Test User",
        string email = "test@example.com")
    {
        return new User
        {
            Id = id,
            Name = name,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

**Usage in Tests:**
```csharp
[Fact]
public void UserIsValid_WhenCreatedWithFixture()
{
    var user = UserFixtures.CreateValidUser();
    Assert.NotNull(user);
}
```

**Fixture Location:**
- `tests/backend/ETOmniverse.Domain.Tests/Common/Fixtures/`
- One file per domain entity: `UserFixtures.cs`, `BatchFixtures.cs`, etc.

## API Testing (WebApplicationFactory)

**Pattern (Example - to be implemented):**

Location: `tests/backend/ETOmniverse.Api.Tests/`

```csharp
public class HealthEndpointTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task HealthLiveEndpoint_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("live", content);
    }
}
```

**WebApplicationFactory Setup:**
- Inherit `WebApplicationFactory<Program>` for custom app configuration
- Override `ConfigureWebHost()` for dependency injection replacement in tests
- Use `IAsyncLifetime` for proper async setup/teardown

**Best Practices:**
- Test happy path first (endpoint returns 200 with expected data)
- Test validation errors (400 Bad Request)
- Test authorization (401/403 responses)
- Test error handling (500 with ProblemDetails)

## Integration Testing (Testcontainers - Future)

**Not yet implemented. When adding:**

Add to test csproj:
```xml
<PackageReference Include="Testcontainers" Version="3.x.x" />
<PackageReference Include="Testcontainers.MsSql" Version="3.x.x" />
<PackageReference Include="EFCore.Testcontainers" Version="1.x.x" />
```

**Pattern (Future):**
```csharp
[Collection("Integration")]
public class RepositoryIntegrationTests
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    [Fact]
    public async Task SaveUser_StoresInDatabase()
    {
        await _container.StartAsync();
        
        var options = new DbContextOptionsBuilder<EtOmniverseDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;

        using var context = new EtOmniverseDbContext(options);
        await context.Database.MigrateAsync();

        // Act
        // Assert
    }
}
```

## Coverage

**Requirements:** No enforced coverage target yet (Phase 1)

**View Coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
# Coverage reports generated in: TestResults/*/coverage.cobertura.xml
```

**When coverage target enforced (Phase 1.1):**
- Domain: 80%+ (business logic critical)
- UseCase: 90%+ (orchestration layer)
- Api Endpoints: 70%+ (happy path + error cases)
- Infrastructure: 60%+ (mostly integration)

## Test Types

**Unit Tests (Domain.Tests):**
- **Scope:** Single class/method in isolation
- **Approach:** Test domain logic (UseCase, Service, Entity behavior)
- **Data:** Test fixtures, in-memory data
- **Speed:** Milliseconds

**Example target:** `src/backend/ETOmniverse.Domain/Common/Model/Result.cs`
- Unit test: `tests/backend/ETOmniverse.Domain.Tests/Common/Model/ResultTests.cs`

**API Tests (Api.Tests):**
- **Scope:** Full request-response cycle via WebApplicationFactory
- **Approach:** Test endpoint contract (status code, response shape, validation)
- **Data:** Use SeededDbContext or factory fixtures
- **Speed:** Seconds (includes middleware, routing, serialization)

**Integration Tests (Future):**
- **Scope:** Repository + EF Core + actual MSSQL
- **Approach:** Testcontainers spins up Docker MSSQL, run migrations, assert state
- **Data:** Fresh database per test
- **Speed:** Seconds-to-minutes (container startup)

**E2E Tests (Future - Playwright):**
- **Scope:** Full application flow (frontend + backend + database)
- **Approach:** Playwright automates browser, verifies UI + backend state
- **Data:** Real/test database
- **Speed:** Minutes

## Common Patterns

**Async Testing:**

```csharp
[Fact]
public async Task GetUserAsync_ReturnsUser()
{
    // Arrange
    var service = new UserService();
    
    // Act
    var result = await service.GetUserAsync(1);
    
    // Assert
    Assert.NotNull(result);
}
```

**Theory Tests with Multiple Cases:**

```csharp
[Theory]
[InlineData(true, "Success")]
[InlineData(false, "Failure")]
public void Result_BehaviorVariesByIsSuccess(bool isSuccess, string expected)
{
    var result = new Result(isSuccess);
    Assert.Equal(isSuccess, result.IsSuccess);
}
```

**Error Testing (expected exceptions):**

```csharp
[Fact]
public void InvalidInput_ThrowsValidationException()
{
    // Arrange
    var invalidData = new { Name = "" }; // Empty name
    
    // Act & Assert
    Assert.Throws<ValidationException>(() => 
        new User { Name = invalidData.Name });
}
```

**Testing with Result Type (no exceptions):**

```csharp
[Fact]
public void ValidateUser_ReturnsFailureForEmptyName()
{
    // Arrange
    var user = new User { Name = "" };
    
    // Act
    var result = UserValidator.Validate(user);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal("EMPTY_NAME", result.ErrorCode);
}
```

## Feature Implementation Checklist

**When adding new feature, test requirements:**

- [ ] WriteFirst UseCase unit test
- [ ] Implement UseCase in Domain
- [ ] Write API endpoint test (WebApplicationFactory)
- [ ] Implement Minimal API endpoint
- [ ] Write FluentValidation rules + tests
- [ ] For query endpoints: write integration test if touching database

**Minimum for "feature complete":**
- Unit test for UseCase ✓
- API happy-path test ✓
- Validation test (if validation present) ✓
- No uncovered branches in business logic

---

*Testing analysis: 2026-05-08*
