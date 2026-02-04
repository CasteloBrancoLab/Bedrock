using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-049: Metodos *Internal em classes abstratas devem ser protegidos.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class InternalMethodProtectedInAbstractClassesRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public InternalMethodProtectedInAbstractClassesRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Internal_devem_ser_protegidos_em_classes_abstratas()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE049_InternalMethodProtectedInAbstractClassesRule());
    }
}
