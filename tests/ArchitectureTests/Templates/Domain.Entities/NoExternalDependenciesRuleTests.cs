using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-027: Entidades de domínio não devem ter dependências externas.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class NoExternalDependenciesRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public NoExternalDependenciesRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Entidades_nao_devem_ter_dependencias_externas()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE027_NoExternalDependenciesRule());
    }
}
