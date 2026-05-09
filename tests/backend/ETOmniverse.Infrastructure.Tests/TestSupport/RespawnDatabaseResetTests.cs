namespace ETOmniverse.Infrastructure.Tests.TestSupport;

using ETOmniverse.TestSupport.Database;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

/// <summary>
/// AC-B2: RespawnDatabaseReset.ResetAsync truncates user tables, preserves __EFMigrationsHistory.
/// Per Phase 05 慣例：Docker 不可用 → skip 不 fake-pass。
/// </summary>
[Collection("Database")]
public sealed class RespawnDatabaseResetTests
{
    private readonly MsSqlContainerFixture _fixture;

    public RespawnDatabaseResetTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ResetAsync_TruncatesUserTables_PreservesEFMigrationsHistory()
    {
        if (!_fixture.IsContainerAvailable)
        {
            return; // Docker 不可用 skip
        }

        // Arrange: 建一張 throwaway user table + 插一筆，避免依賴 F-005 具體 aggregate
        await using (var conn = new SqlConnection(_fixture.ConnectionString))
        {
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                IF OBJECT_ID('dbo.respawn_smoke') IS NULL
                    CREATE TABLE dbo.respawn_smoke (id int);
                INSERT INTO dbo.respawn_smoke (id) VALUES (1);
                """;
            await cmd.ExecuteNonQueryAsync();
        }

        // Act
        await new RespawnDatabaseReset().ResetAsync(_fixture.ConnectionString);

        // Assert
        await using var assertConn = new SqlConnection(_fixture.ConnectionString);
        await assertConn.OpenAsync();

        await using var cmdRows = assertConn.CreateCommand();
        cmdRows.CommandText = "SELECT COUNT(*) FROM dbo.respawn_smoke";
        var rowsAfterReset = (int)(await cmdRows.ExecuteScalarAsync())!;
        rowsAfterReset.Should().Be(0, "Respawn 應截斷 user table");

        await using var cmdMig = assertConn.CreateCommand();
        cmdMig.CommandText = "SELECT COUNT(*) FROM __EFMigrationsHistory";
        var migRows = (int)(await cmdMig.ExecuteScalarAsync())!;
        migRows.Should().BeGreaterThan(0, "Respawn 應保留 __EFMigrationsHistory");
    }
}
