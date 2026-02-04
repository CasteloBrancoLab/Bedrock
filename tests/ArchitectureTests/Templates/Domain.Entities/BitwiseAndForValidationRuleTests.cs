using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-006: Métodos *Internal devem usar operador &amp; ao invés de &amp;&amp; para validação completa.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class BitwiseAndForValidationRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public BitwiseAndForValidationRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metodos_Internal_devem_usar_operador_bitwise_and()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE006_BitwiseAndForValidationRule());
    }
}
