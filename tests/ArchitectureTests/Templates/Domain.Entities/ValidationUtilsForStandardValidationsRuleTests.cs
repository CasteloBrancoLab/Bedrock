using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-010: Métodos Validate* devem usar ValidationUtils para validações padrão
/// ao invés de implementar validação inline.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ValidationUtilsForStandardValidationsRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ValidationUtilsForStandardValidationsRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metodos_validate_devem_usar_validation_utils()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE010_ValidationUtilsForStandardValidationsRule());
    }
}
