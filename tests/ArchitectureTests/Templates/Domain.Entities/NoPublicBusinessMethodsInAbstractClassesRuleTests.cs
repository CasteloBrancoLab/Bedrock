using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-050: Classes abstratas nao devem expor metodos publicos de negocio.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class NoPublicBusinessMethodsInAbstractClassesRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public NoPublicBusinessMethodsInAbstractClassesRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Classes_abstratas_nao_devem_ter_metodos_publicos_de_negocio()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE050_NoPublicBusinessMethodsInAbstractClassesRule());
    }
}
