using Bedrock.ArchitectureTests.ShopDemo.Auth.Application.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.ShopDemo.Auth.Application;

[Collection("Arch")]
public sealed class MessagesRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : RuleTestBase<ArchFixture>(fixture, output)
{
    [Fact]
    public void MS013_Tipos_em_Application_nao_devem_instanciar_eventos_diretamente()
    {
        AssertNoViolations(new MS013_EventCreationViaFactoryRule());
    }
}
