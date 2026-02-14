using Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Tests;

/// <summary>
/// Classe base com os 3 [Fact] de regras Code Style (CS001-CS003).
/// Todos os projetos de ArchitectureTests herdam esta classe.
/// </summary>
public abstract class CodeStyleRuleTestsBase<TFixture> : RuleTestBase<TFixture>
    where TFixture : RuleFixture
{
    protected CodeStyleRuleTestsBase(TFixture fixture, ITestOutputHelper output)
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

    [Fact]
    public void CS003_Logging_deve_usar_variantes_ForDistributedTracing()
    {
        AssertNoViolations(new CS003_LoggingWithDistributedTracingRule());
    }
}
