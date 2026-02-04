using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-022: Métodos Set* devem ser privados.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class SetMethodsPrivateRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public SetMethodsPrivateRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metodos_Set_devem_ser_privados()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE022_SetMethodsPrivateRule());
    }
}
