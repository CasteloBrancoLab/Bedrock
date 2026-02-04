using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-052: Construtores em classes abstratas devem ser protegidos.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ProtectedConstructorsInAbstractClassesRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ProtectedConstructorsInAbstractClassesRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Construtores_devem_ser_protegidos_em_classes_abstratas()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE052_ProtectedConstructorsInAbstractClassesRule());
    }
}
