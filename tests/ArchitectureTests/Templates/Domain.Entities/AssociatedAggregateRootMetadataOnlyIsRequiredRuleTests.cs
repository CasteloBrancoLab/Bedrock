using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-057: Metadata de Aggregate Roots associadas deve ter apenas IsRequired.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class AssociatedAggregateRootMetadataOnlyIsRequiredRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public AssociatedAggregateRootMetadataOnlyIsRequiredRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metadata_de_ARs_associadas_deve_ter_apenas_IsRequired()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRule());
    }
}
