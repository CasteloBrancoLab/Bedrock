using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-030: Métodos Validate* devem usar CreateMessageCode para códigos de mensagem.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class MessageCodesWithCreateMessageCodeRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public MessageCodesWithCreateMessageCodeRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metodos_Validate_devem_usar_CreateMessageCode()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE030_MessageCodesWithCreateMessageCodeRule());
    }
}
