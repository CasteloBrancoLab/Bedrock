using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-002: Classes concretas devem ter apenas construtores privados.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class PrivateConstructorRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public PrivateConstructorRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Classes_devem_ter_apenas_construtores_privados()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE002_PrivateConstructorRule());
    }
}
