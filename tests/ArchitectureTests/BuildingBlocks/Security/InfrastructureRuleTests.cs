using Bedrock.ArchitectureTests.BuildingBlocks.Security.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.BuildingBlocks.Security;

[Collection("Arch")]
public sealed class InfrastructureRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : InfrastructureRuleTestsBase<ArchFixture>(fixture, output);
