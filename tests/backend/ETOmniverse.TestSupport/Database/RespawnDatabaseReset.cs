namespace ETOmniverse.TestSupport.Database;

using Microsoft.Data.SqlClient;
using Respawn;
using Respawn.Graph;

/// <summary>
/// Cross-class data reset utility for E2E test suites (per F-007 / D-21).
/// Truncates all user tables but preserves <c>__EFMigrationsHistory</c> so the schema
/// (already migrated by <see cref="MsSqlContainerFixture"/>.<c>InitializeAsync</c>) is not re-applied.
/// Integration test routes typically use <see cref="TransactionalTestBase"/> (faster);
/// reach for <see cref="ResetAsync"/> only when transaction rollback isn't viable
/// (e.g. cross-process E2E via Playwright).
/// </summary>
public sealed class RespawnDatabaseReset
{
    private static readonly RespawnerOptions Options = new()
    {
        DbAdapter = DbAdapter.SqlServer,
        TablesToIgnore = new Table[] { new("__EFMigrationsHistory") },
    };

    public async Task ResetAsync(string connectionString, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        var respawner = await Respawner.CreateAsync(connection, Options);
        await respawner.ResetAsync(connection);
    }
}
