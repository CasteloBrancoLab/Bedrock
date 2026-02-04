using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-003: Métodos públicos de instância em entidades devem seguir o padrão Clone-Modify-Return.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class CloneModifyReturnRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public CloneModifyReturnRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metodos_publicos_de_instancia_devem_seguir_clone_modify_return()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE003_CloneModifyReturnRule());
    }
}
