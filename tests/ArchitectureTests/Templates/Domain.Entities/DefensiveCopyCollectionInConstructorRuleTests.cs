using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-039: Construtores devem fazer copia defensiva de colecoes.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class DefensiveCopyCollectionInConstructorRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public DefensiveCopyCollectionInConstructorRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Construtores_devem_fazer_copia_defensiva_de_colecoes()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE039_DefensiveCopyCollectionInConstructorRule());
    }
}
