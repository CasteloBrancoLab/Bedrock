using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

[Collection("Arch")]
public sealed class InfrastructureRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : InfrastructureRuleTestsBase<ArchFixture>(fixture, output);
