using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-024: Métodos públicos NUNCA chamam outros métodos públicos da mesma classe.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class PublicMethodNeverCallsPublicRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public PublicMethodNeverCallsPublicRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metodos_publicos_nao_devem_chamar_outros_publicos()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE024_PublicMethodNeverCallsPublicRule());
    }
}
