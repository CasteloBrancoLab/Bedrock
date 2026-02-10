using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

[Collection("CodeStyleArch")]
public sealed class CodeStyleRuleTests : RuleTestBase<CodeStyleArchFixture>
{
    public CodeStyleRuleTests(CodeStyleArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void CS001_Interfaces_devem_residir_em_subpasta_Interfaces()
    {
        AssertNoViolations(new CS001_InterfacesInInterfacesNamespaceRule());
    }

    [Fact]
    public void CS002_Lambdas_inline_devem_ser_static_em_metodos_do_projeto()
    {
        AssertNoViolations(new CS002_StaticLambdasInProjectMethodsRule());
    }
}
