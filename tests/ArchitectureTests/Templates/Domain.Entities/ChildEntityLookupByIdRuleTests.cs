using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-042: Operacoes de modificacao devem localizar entidade filha por Id.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ChildEntityLookupByIdRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ChildEntityLookupByIdRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Operacoes_de_modificacao_devem_localizar_filha_por_Id()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE042_ChildEntityLookupByIdRule());
    }
}
