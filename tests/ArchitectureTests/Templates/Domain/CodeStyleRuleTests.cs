using Bedrock.ArchitectureTests.Templates.Domain.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain;

[Collection("Arch")]
public sealed class CodeStyleRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : CodeStyleRuleTestsBase<ArchFixture>(fixture, output);
