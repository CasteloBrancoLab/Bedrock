using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using Bedrock.BuildingBlocks.Testing.Benchmarks;
using Bedrock.PerformanceTests.BuildingBlocks.Persistence.PostgreSql.Infrastructure;
using Npgsql;

namespace Bedrock.PerformanceTests.BuildingBlocks.Persistence.PostgreSql.Benchmarks;

/// <summary>
/// Sustained-loop benchmark for PostgreSQL insert operations.
/// Runs continuously for <see cref="BenchmarkBase.DefaultDuration"/> to measure
/// throughput and detect memory leaks over time.
/// </summary>
public class RepositoryInsertBenchmark : BenchmarkBase
{
    private const int BatchSize = 100;
    private static readonly TimeSpan LogInterval = TimeSpan.FromSeconds(30);

    private string _connectionString = null!;
    private RuntimeMetricsTracker _tracker = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Container is already started by Program.cs before BenchmarkSwitcher runs
        _connectionString = PostgresBenchmarkSetup.GetAppUserConnectionString();
        _tracker = new RuntimeMetricsTracker();
    }

    [Benchmark(Description = "Insert entities (sustained loop)")]
    public async Task InsertEntitiesSustained()
    {
        var sw = Stopwatch.StartNew();
        var ops = 0L;
        var lastLog = TimeSpan.Zero;

        while (sw.Elapsed < DefaultDuration)
        {
            var tenantCode = Guid.NewGuid();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            for (var i = 0; i < BatchSize; i++)
            {
                await using var command = new NpgsqlCommand(
                    """
                    INSERT INTO test_entities (id, tenant_code, name, created_by, created_at,
                        created_correlation_id, created_execution_origin, created_business_operation_code, entity_version)
                    VALUES (@id, @tenantCode, @name, @createdBy, @createdAt,
                        @createdCorrelationId, @createdExecutionOrigin, @createdBusinessOperationCode, @entityVersion)
                    """,
                    connection,
                    transaction);

                command.Parameters.AddWithValue("id", Guid.NewGuid());
                command.Parameters.AddWithValue("tenantCode", tenantCode);
                command.Parameters.AddWithValue("name", $"BenchEntity_{i}");
                command.Parameters.AddWithValue("createdBy", "benchmark_user");
                command.Parameters.AddWithValue("createdAt", DateTimeOffset.UtcNow);
                command.Parameters.AddWithValue("createdCorrelationId", Guid.NewGuid());
                command.Parameters.AddWithValue("createdExecutionOrigin", "Benchmarks");
                command.Parameters.AddWithValue("createdBusinessOperationCode", "BENCHMARK_INSERT");
                command.Parameters.AddWithValue("entityVersion", 1L);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            ops += BatchSize;

            if (sw.Elapsed - lastLog >= LogInterval)
            {
                var opsPerSec = ops / sw.Elapsed.TotalSeconds;
                Console.WriteLine($"  [{sw.Elapsed:mm\\:ss}] {ops:N0} inserts ({opsPerSec:F0} ops/sec)");
                lastLog = sw.Elapsed;
            }
        }

        Console.WriteLine($"  TOTAL: {ops:N0} inserts in {sw.Elapsed:mm\\:ss} ({ops / sw.Elapsed.TotalSeconds:F0} ops/sec)");

        // Cleanup benchmark data
        await using var cleanConn = new NpgsqlConnection(PostgresBenchmarkSetup.GetAdminConnectionString());
        await cleanConn.OpenAsync();
        await using var cleanCmd = new NpgsqlCommand(
            "DELETE FROM test_entities WHERE created_by = 'benchmark_user'", cleanConn);
        await cleanCmd.ExecuteNonQueryAsync();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        // Container cleanup is handled by Program.cs after BenchmarkSwitcher finishes
        var result = _tracker.Analyze();
        RuntimeMetricsStore.Record(nameof(RepositoryInsertBenchmark), result);
        _tracker.Dispose();
    }
}
