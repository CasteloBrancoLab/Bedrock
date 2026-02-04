using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-013: Membros de metadados devem seguir a convenção {PropertyName}{ConstraintType}.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class MetadataNamingConventionRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public MetadataNamingConventionRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metadados_devem_seguir_convencao_PropertyName_ConstraintType()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE013_MetadataNamingConventionRule());
    }
}
