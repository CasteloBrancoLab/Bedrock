using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-038: Fields de colecao devem ser sempre inicializados como lista vazia.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class CollectionFieldAlwaysInitializedRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public CollectionFieldAlwaysInitializedRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Fields_de_colecao_devem_ser_inicializados()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE038_CollectionFieldAlwaysInitializedRule());
    }
}
