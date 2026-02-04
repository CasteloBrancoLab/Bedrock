using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-058: Entidades com ARs associadas devem ter Process/Validate/Set correspondentes.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ProcessValidateSetForAssociatedAggregateRootsRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ProcessValidateSetForAssociatedAggregateRootsRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void ARs_associadas_devem_ter_Process_Validate_Set()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE058_ProcessValidateSetForAssociatedAggregateRootsRule());
    }
}
