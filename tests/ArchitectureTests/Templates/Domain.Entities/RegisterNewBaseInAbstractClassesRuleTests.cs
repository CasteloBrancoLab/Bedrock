using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-055: Classes abstratas devem ter RegisterNewBase para controlar registro.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class RegisterNewBaseInAbstractClassesRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public RegisterNewBaseInAbstractClassesRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Classes_abstratas_devem_ter_RegisterNewBase()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE055_RegisterNewBaseInAbstractClassesRule());
    }
}
