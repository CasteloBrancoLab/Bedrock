using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using Bedrock.BuildingBlocks.Testing.Benchmarks;
using Bedrock.PerformanceTests.BuildingBlocks.Persistence.PostgreSql.Infrastructure;
using Npgsql;

namespace Bedrock.PerformanceTests.BuildingBlocks.Persistence.PostgreSql.Benchmarks;

/// <summary>
/// Sustained-loop benchmark for PostgreSQL query operations.
/// Seeds test data once, then runs queries continuously for <see cref="BenchmarkBase.DefaultDuration"/>
/// to measure throughput and detect memory leaks over time.
/// </summary>
public class RepositoryQueryBenchmark : BenchmarkBase
{
    private static readonly TimeSpan LogInterval = TimeSpan.FromSeconds(30);

    private string _connectionString = null!;
    private RuntimeMetricsTracker _tracker = null!;
    private Guid _tenantCode;
    private readonly List<Guid> _seededIds = [];

    private const int SeedCount = 100;

    [GlobalSetup]
    public void Setup()
    {
        // Container is already started by Program.cs before BenchmarkSwitcher runs
        _connectionString = PostgresBenchmarkSetup.GetAppUserConnectionString();
        _tracker = new RuntimeMetricsTracker();
        _tenantCode = Guid.NewGuid();

        // Seed test data synchronously (InProcess toolchain may not handle async [GlobalSetup])
        SeedTestData().GetAwaiter().GetResult();
    }

    private async Task SeedTestData()
    {
        await using var connection = new NpgsqlConnection(PostgresBenchmarkSetup.GetAdminConnectionString());
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        for (var i = 0; i < SeedCount; i++)
        {
            var id = Guid.NewGuid();
            _seededIds.Add(id);

            await using var command = new NpgsqlCommand(
                """
                INSERT INTO test_entities (id, tenant_code, name, created_by, created_at,
                    created_correlation_id, created_execution_origin, created_business_operation_code, entity_version)
                VALUES (@id, @tenantCode, @name, @createdBy, @createdAt,
                    @createdCorrelationId, @createdExecutionOrigin, @createdBusinessOperationCode, @entityVersion)
                """,
                connection,
                transaction);

            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("tenantCode", _tenantCode);
            command.Parameters.AddWithValue("name", $"SeedEntity_{i}");
            command.Parameters.AddWithValue("createdBy", "benchmark_seed");
            command.Parameters.AddWithValue("createdAt", DateTimeOffset.UtcNow);
            command.Parameters.AddWithValue("createdCorrelationId", Guid.NewGuid());
            command.Parameters.AddWithValue("createdExecutionOrigin", "Benchmarks");
            command.Parameters.AddWithValue("createdBusinessOperationCode", "BENCHMARK_SEED");
            command.Parameters.AddWithValue("entityVersion", 1L);

            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    [Benchmark(Description = "Query by ID (sustained loop)")]
    public async Task QueryByIdSustained()
    {
        var sw = Stopwatch.StartNew();
        var ops = 0L;
        var lastLog = TimeSpan.Zero;

        while (sw.Elapsed < DefaultDuration)
        {
            var targetId = _seededIds[Random.Shared.Next(_seededIds.Count)];

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(
                """
                SELECT id, tenant_code, name, created_by, created_at, entity_version
                FROM test_entities
                WHERE id = @id AND tenant_code = @tenantCode
                """,
                connection);

            command.Parameters.AddWithValue("id", targetId);
            command.Parameters.AddWithValue("tenantCode", _tenantCode);

            await using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();
            ops++;

            if (sw.Elapsed - lastLog >= LogInterval)
            {
                var opsPerSec = ops / sw.Elapsed.TotalSeconds;
                Console.WriteLine($"  [{sw.Elapsed:mm\\:ss}] {ops:N0} queries ({opsPerSec:F0} ops/sec)");
                lastLog = sw.Elapsed;
            }
        }

        Console.WriteLine($"  TOTAL: {ops:N0} queries in {sw.Elapsed:mm\\:ss} ({ops / sw.Elapsed.TotalSeconds:F0} ops/sec)");
    }

    [Benchmark(Description = "Query by tenant (sustained loop)")]
    public async Task QueryByTenantSustained()
    {
        var sw = Stopwatch.StartNew();
        var ops = 0L;
        var lastLog = TimeSpan.Zero;

        while (sw.Elapsed < DefaultDuration)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(
                """
                SELECT id, tenant_code, name, created_by, created_at, entity_version
                FROM test_entities
                WHERE tenant_code = @tenantCode
                """,
                connection);

            command.Parameters.AddWithValue("tenantCode", _tenantCode);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // consume all rows
            }

            ops++;

            if (sw.Elapsed - lastLog >= LogInterval)
            {
                var opsPerSec = ops / sw.Elapsed.TotalSeconds;
                Console.WriteLine($"  [{sw.Elapsed:mm\\:ss}] {ops:N0} queries ({opsPerSec:F0} ops/sec)");
                lastLog = sw.Elapsed;
            }
        }

        Console.WriteLine($"  TOTAL: {ops:N0} queries in {sw.Elapsed:mm\\:ss} ({ops / sw.Elapsed.TotalSeconds:F0} ops/sec)");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        // Container cleanup is handled by Program.cs after BenchmarkSwitcher finishes
        var result = _tracker.Analyze();
        RuntimeMetricsStore.Record(nameof(RepositoryQueryBenchmark), result);
        _tracker.Dispose();

        // Clean up seeded data synchronously
        CleanupSeededData().GetAwaiter().GetResult();
    }

    private async Task CleanupSeededData()
    {
        await using var connection = new NpgsqlConnection(PostgresBenchmarkSetup.GetAdminConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "DELETE FROM test_entities WHERE created_by = 'benchmark_seed'",
            connection);
        await command.ExecuteNonQueryAsync();
    }
}
