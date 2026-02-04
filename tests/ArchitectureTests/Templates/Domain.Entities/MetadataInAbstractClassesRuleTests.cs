using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-053: Classes abstratas com propriedades validaveis devem ter classe Metadata.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class MetadataInAbstractClassesRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public MetadataInAbstractClassesRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Classes_abstratas_com_validacao_devem_ter_Metadata()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE053_MetadataInAbstractClassesRule());
    }
}
