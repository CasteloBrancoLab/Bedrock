using Bedrock.ArchitectureTests.BuildingBlocks.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.BuildingBlocks.Domain.Entities;

[Collection("Arch")]
public sealed class DomainEntitiesRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : DomainEntitiesRuleTestsBase<ArchFixture>(fixture, output);
