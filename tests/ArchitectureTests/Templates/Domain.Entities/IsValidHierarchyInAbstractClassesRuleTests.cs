using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-051: Classes abstratas devem ter hierarquia de IsValid com tres metodos.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class IsValidHierarchyInAbstractClassesRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public IsValidHierarchyInAbstractClassesRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Classes_abstratas_devem_ter_IsValidConcreteInternal_abstrato()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE051_IsValidHierarchyInAbstractClassesRule());
    }
}
