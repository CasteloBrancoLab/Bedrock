using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-034: Metodos publicos de instancia nao devem retornar void (usar Clone-Modify-Return).
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class NoVoidMutationMethodsRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public NoVoidMutationMethodsRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metodos_publicos_nao_devem_retornar_void()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE034_NoVoidMutationMethodsRule());
    }
}
