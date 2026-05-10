namespace ETOmniverse.Infrastructure.Tests.TestSupport;

using System.Diagnostics;
using ETOmniverse.TestSupport.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// AC-B3 + AC-B6: 50 transactional facts complete in &lt; 5 sec dev / 10 sec CI, and per-test rollback
/// keeps each invocation isolated. Each invocation inserts row idx into dbo.tx_smoke (created on-demand
/// inside the transaction); within the same test we observe count=1 for our idx; rollback at DisposeAsync
/// means the next invocation starts clean. If rollback failed we'd observe accumulating rows
/// (the visible.Should().Be(1) assertion would fail on the 2nd+ idx).
/// </summary>
[Collection("Database")]
public sealed class TransactionalTestBaseSmokeTests : TransactionalTestBase
{
    private static readonly Stopwatch s_sharedTimer = new();
    private static int s_completedCount;
    private const int TotalFacts = 50;
    private const int DevBudgetMs = 5_000;
    private const int CiBudgetMs = 10_000;

    public TransactionalTestBaseSmokeTests(MsSqlContainerFixture fixture) : base(fixture) { }

    [DockerTheory]
    [InlineData(0)]  [InlineData(1)]  [InlineData(2)]  [InlineData(3)]  [InlineData(4)]
    [InlineData(5)]  [InlineData(6)]  [InlineData(7)]  [InlineData(8)]  [InlineData(9)]
    [InlineData(10)] [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)]
    [InlineData(15)] [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)]
    [InlineData(20)] [InlineData(21)] [InlineData(22)] [InlineData(23)] [InlineData(24)]
    [InlineData(25)] [InlineData(26)] [InlineData(27)] [InlineData(28)] [InlineData(29)]
    [InlineData(30)] [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(34)]
    [InlineData(35)] [InlineData(36)] [InlineData(37)] [InlineData(38)] [InlineData(39)]
    [InlineData(40)] [InlineData(41)] [InlineData(42)] [InlineData(43)] [InlineData(44)]
    [InlineData(45)] [InlineData(46)] [InlineData(47)] [InlineData(48)] [InlineData(49)]
    public async Task FiftyTransactionalInsertsAreFastAndIsolated(int idx)
    {
        if (idx == 0)
        {
            s_sharedTimer.Restart();
            s_completedCount = 0;
        }

        // Throwaway table（schema 由首次 invocation create；後續 invocation 因 rollback 看不到 → 重 create OK）
        var setup = """
            IF OBJECT_ID('dbo.tx_smoke') IS NULL
                CREATE TABLE dbo.tx_smoke (id int);
            """;
        await DbContext.Database.ExecuteSqlRawAsync(setup);

        var insert = $"INSERT INTO dbo.tx_smoke (id) VALUES ({idx});";
        await DbContext.Database.ExecuteSqlRawAsync(insert);

        // 同 test 看得到自己 insert（rollback 隔離 → 永遠是 1）
        // SqlQuery (FormattableString overload) 自動參數化 idx，避免 EF1002。
        var visible = await DbContext.Database
            .SqlQuery<int>($"SELECT COUNT(*) AS Value FROM dbo.tx_smoke WHERE id = {idx}")
            .FirstAsync();
        visible.Should().Be(1, "transactional insert 必須在同一 test method 內可見");

        var done = System.Threading.Interlocked.Increment(ref s_completedCount);

        if (done == TotalFacts)
        {
            s_sharedTimer.Stop();
            var budget = System.Environment.GetEnvironmentVariable("CI") == "true" ? CiBudgetMs : DevBudgetMs;
            s_sharedTimer.ElapsedMilliseconds.Should().BeLessThan(budget,
                $"50 transactional facts must complete < {budget}ms (per F-007 AC-B6)");
        }
    }
}
