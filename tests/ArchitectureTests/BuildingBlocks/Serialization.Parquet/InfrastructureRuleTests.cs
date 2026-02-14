using Bedrock.ArchitectureTests.BuildingBlocks.Serialization.Parquet.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.BuildingBlocks.Serialization.Parquet;

[Collection("Arch")]
public sealed class InfrastructureRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : InfrastructureRuleTestsBase<ArchFixture>(fixture, output);
