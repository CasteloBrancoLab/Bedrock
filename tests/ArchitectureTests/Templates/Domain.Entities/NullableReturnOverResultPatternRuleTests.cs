using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-007: Métodos de entidades devem retornar T? ao invés de Result&lt;T&gt;.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class NullableReturnOverResultPatternRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public NullableReturnOverResultPatternRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metodos_de_entidades_devem_retornar_nullable_ao_inves_de_result_pattern()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE007_NullableReturnOverResultPatternRule());
    }
}
