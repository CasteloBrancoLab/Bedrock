using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-047: Metodos Set* em classes abstratas devem ser privados.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class SetMethodPrivateInAbstractClassesRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public SetMethodPrivateInAbstractClassesRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Set_devem_ser_privados_em_classes_abstratas()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE047_SetMethodPrivateInAbstractClassesRule());
    }
}
