using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-009: Métodos Validate* e IsValid devem ser públicos e estáticos
/// para permitir validação antecipada em camadas externas.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ValidateMethodsPublicStaticRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ValidateMethodsPublicStaticRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metodos_validate_devem_ser_publicos_e_estaticos()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE009_ValidateMethodsPublicStaticRule());
    }
}
