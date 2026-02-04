using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-045: Validacao de duplicidade em operacoes de alteracao deve ignorar a propria entidade.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class DuplicateValidationIgnoresSelfRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public DuplicateValidationIgnoresSelfRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Validacao_de_duplicidade_deve_ignorar_propria_entidade()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE045_DuplicateValidationIgnoresSelfRule());
    }
}
