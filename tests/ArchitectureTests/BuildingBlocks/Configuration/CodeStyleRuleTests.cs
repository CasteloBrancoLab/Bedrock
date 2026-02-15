using Bedrock.ArchitectureTests.BuildingBlocks.Configuration.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.BuildingBlocks.Configuration;

[Collection("Arch")]
public sealed class CodeStyleRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : CodeStyleRuleTestsBase<ArchFixture>(fixture, output);
