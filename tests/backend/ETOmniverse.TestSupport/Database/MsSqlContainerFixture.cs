namespace ETOmniverse.TestSupport.Database;

using ETOmniverse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Xunit;

/// <summary>
/// Shared MSSQL Testcontainer for the [Collection("Database")] xUnit collection (per F-007 / D-21).
/// Container is started once per collection (InitializeAsync) and EF Core migration runs once
/// via the F-005 design-time factory. DisposeAsync stops and disposes the container.
/// Per-test data isolation is the responsibility of TransactionalTestBase (07-03) — this fixture
/// only guarantees container lifecycle and that the schema is migrated.
/// </summary>
public sealed class MsSqlContainerFixture : IAsyncLifetime
{
    // Lazy: 在 InitializeAsync 才 Build()，避免 ctor 階段就觸發 Docker endpoint validation。
    // 若不 lazy 則 Docker 不可用環境下 fixture ctor 即拋 DockerUnavailableException，
    // 導致所有 [Collection("Database")] test 連 skip path 都進不了（xUnit 在跑 test method 前建構 fixture）。
    private MsSqlContainer? _container;

    public string ConnectionString =>
        _container?.GetConnectionString()
        ?? throw new InvalidOperationException(
            "MsSqlContainerFixture not initialized — InitializeAsync must run before ConnectionString is queried.");

    /// <summary>
    /// True iff the container is alive and migrated. False when Docker is unavailable on the host
    /// (per Phase 05 慣例：本機無 Docker daemon 時 skip 不 fake-pass).
    /// </summary>
    public bool IsContainerAvailable => _container is not null;

    public async Task InitializeAsync()
    {
        if (!IsDockerAvailable())
        {
            // Phase 05 慣例：Docker 不可用 → fixture 不 throw，留給 test class 自行 skip。
            return;
        }
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
        await _container.StartAsync();
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    private static bool IsDockerAvailable()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("docker", "info")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return false;
            }
            return process.WaitForExit(2000) && process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    public EtOmniverseDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EtOmniverseDbContext>();
        EtOmniverseDbContextFactory.ConfigureSqlServer(options, ConnectionString);
        return new EtOmniverseDbContext(options.Options);
    }
}
