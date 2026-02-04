using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-037: Propriedades publicas de colecao devem retornar IReadOnlyList&lt;T&gt; via AsReadOnly().
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class PublicPropertyIReadOnlyListRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public PublicPropertyIReadOnlyListRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Propriedades_publicas_de_colecao_devem_retornar_IReadOnlyList()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE037_PublicPropertyIReadOnlyListRule());
    }
}
