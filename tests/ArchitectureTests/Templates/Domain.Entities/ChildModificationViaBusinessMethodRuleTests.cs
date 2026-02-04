using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-043: Modificacao de entidade filha deve ser via metodo de negocio dela.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ChildModificationViaBusinessMethodRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ChildModificationViaBusinessMethodRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Modificacao_de_filha_deve_ser_via_metodo_de_negocio()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE043_ChildModificationViaBusinessMethodRule());
    }
}
