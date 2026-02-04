using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-036: Colecoes de entidades filhas devem ser field privado List&lt;T&gt;.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ChildCollectionPrivateListFieldRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ChildCollectionPrivateListFieldRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Colecoes_filhas_devem_ser_field_privado()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE036_ChildCollectionPrivateListFieldRule());
    }
}
