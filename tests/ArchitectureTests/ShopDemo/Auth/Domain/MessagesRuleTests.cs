using Bedrock.ArchitectureTests.ShopDemo.Auth.Domain.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.ShopDemo.Auth.Domain;

[Collection("Arch")]
public sealed class MessagesRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : RuleTestBase<ArchFixture>(fixture, output)
{
    [Fact]
    public void MS012b_Tipos_em_Domain_Services_nao_devem_usar_sufixo_Result_nem_namespace_Results()
    {
        AssertNoViolations(new MS012b_OutputNamingInServicesRule());
    }
}
