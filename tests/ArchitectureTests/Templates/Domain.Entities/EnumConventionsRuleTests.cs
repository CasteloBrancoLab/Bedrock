using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-046: Enumeracoes de dominio devem seguir convencoes padronizadas.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class EnumConventionsRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public EnumConventionsRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Enumeracoes_devem_seguir_convencoes()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE046_EnumConventionsRule());
    }
}
