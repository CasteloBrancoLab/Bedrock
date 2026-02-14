using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

[Collection("Arch")]
public sealed class DomainEntitiesRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : DomainEntitiesRuleTestsBase<ArchFixture>(fixture, output);
