using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-056: Classes abstratas nao devem ter CreateFromExistingInfo.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class NoCreateFromExistingInfoInAbstractClassesRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public NoCreateFromExistingInfoInAbstractClassesRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Classes_abstratas_nao_devem_ter_CreateFromExistingInfo()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE056_NoCreateFromExistingInfoInAbstractClassesRule());
    }
}
