using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-012: Entidades de domínio devem usar metadados estáticos ao invés de Data Annotations.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class StaticMetadataOverDataAnnotationsRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public StaticMetadataOverDataAnnotationsRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Entidades_nao_devem_usar_data_annotations()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE012_StaticMetadataOverDataAnnotationsRule());
    }
}
