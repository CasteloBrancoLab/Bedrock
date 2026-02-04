using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-025: Métodos *Internal com múltiplos Set* devem usar variável intermediária isSuccess.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class IntermediateVariablesInValidationRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public IntermediateVariablesInValidationRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metodos_Internal_com_multiplos_Set_devem_usar_isSuccess()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE025_IntermediateVariablesInValidationRule());
    }
}
