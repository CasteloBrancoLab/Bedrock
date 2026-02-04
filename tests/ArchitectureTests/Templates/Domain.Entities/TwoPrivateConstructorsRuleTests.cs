using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-020: Entidades devem ter exatamente dois construtores privados: vazio e completo.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class TwoPrivateConstructorsRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public TwoPrivateConstructorsRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Entidades_devem_ter_dois_construtores_privados()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE020_TwoPrivateConstructorsRule());
    }
}
