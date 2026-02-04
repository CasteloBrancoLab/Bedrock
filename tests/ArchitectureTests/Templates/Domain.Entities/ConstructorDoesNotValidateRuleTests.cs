using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-035: Construtores nao devem conter logica de validacao.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ConstructorDoesNotValidateRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ConstructorDoesNotValidateRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Construtores_nao_devem_validar()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE035_ConstructorDoesNotValidateRule());
    }
}
