using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-001: Classes concretas sem herdeiros devem ser sealed.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class SealedClassRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public SealedClassRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Classes_sem_herdeiros_devem_ser_sealed()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE001_SealedClassRule());
    }
}
