using Bedrock.ArchitectureTests.Templates.Infra.CrossCutting.Bootstrapper.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Infra.CrossCutting.Bootstrapper;

[Collection("Arch")]
public sealed class CodeStyleRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : CodeStyleRuleTestsBase<ArchFixture>(fixture, output);
