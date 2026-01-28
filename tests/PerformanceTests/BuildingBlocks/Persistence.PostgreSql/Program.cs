using Bedrock.BuildingBlocks.Testing.Benchmarks;
using Bedrock.PerformanceTests.BuildingBlocks.Persistence.PostgreSql.Infrastructure;

// Start the PostgreSQL container before any benchmarks run
Console.WriteLine(">>> Initializing PostgreSQL container...");
await PostgresBenchmarkSetup.EnsureStartedAsync();
Console.WriteLine(">>> PostgreSQL container ready.");
Console.WriteLine();

// Run sustained benchmarks (custom runner, not BenchmarkDotNet)
// Supports: --filter * (all), --filter *Insert* (pattern)
await SustainedBenchmarkRunner.RunAsync(typeof(Program).Assembly, args);

// Cleanup container
await PostgresBenchmarkSetup.StopAsync();
