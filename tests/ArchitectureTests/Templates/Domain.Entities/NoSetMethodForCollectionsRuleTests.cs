using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-044: Colecoes de entidades filhas nao devem ter metodo Set*.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class NoSetMethodForCollectionsRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public NoSetMethodForCollectionsRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Colecoes_nao_devem_ter_metodo_Set()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE044_NoSetMethodForCollectionsRule());
    }
}
