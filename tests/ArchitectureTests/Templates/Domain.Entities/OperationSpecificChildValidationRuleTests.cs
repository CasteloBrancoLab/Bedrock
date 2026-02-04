using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-041: Validacao de entidades filhas deve ser especifica por operacao.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class OperationSpecificChildValidationRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public OperationSpecificChildValidationRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Validacao_de_entidades_filhas_deve_ser_especifica_por_operacao()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE041_OperationSpecificChildValidationRule());
    }
}
