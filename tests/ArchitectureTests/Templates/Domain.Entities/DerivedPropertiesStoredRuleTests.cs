using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-026: Propriedades derivadas devem ser persistidas, não calculadas via expression body.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class DerivedPropertiesStoredRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public DerivedPropertiesStoredRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Propriedades_publicas_nao_devem_usar_expression_body()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE026_DerivedPropertiesStoredRule());
    }
}
