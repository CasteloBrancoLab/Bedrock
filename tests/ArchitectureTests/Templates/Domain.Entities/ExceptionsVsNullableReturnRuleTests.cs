using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-008: Entidades não devem lançar exceções para validação de negócio.
/// Usar retorno nullable + mensagens no ExecutionContext.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ExceptionsVsNullableReturnRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ExceptionsVsNullableReturnRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Entidades_nao_devem_lancar_excecoes_para_validacao_de_negocio()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE008_ExceptionsVsNullableReturnRule());
    }
}
