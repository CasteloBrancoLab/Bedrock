using Bedrock.ArchitectureTests.Templates.Infra.Data.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Infra.Data;

[Collection("Arch")]
public sealed class CodeStyleRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : CodeStyleRuleTestsBase<ArchFixture>(fixture, output);
