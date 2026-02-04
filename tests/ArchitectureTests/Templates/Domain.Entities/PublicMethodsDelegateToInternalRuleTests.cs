using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-021: Métodos públicos Change* devem delegar lógica para métodos *Internal.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class PublicMethodsDelegateToInternalRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public PublicMethodsDelegateToInternalRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metodos_publicos_Change_devem_ter_Internal_correspondente()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE021_PublicMethodsDelegateToInternalRule());
    }
}
