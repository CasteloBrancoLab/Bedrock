using Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Tests;

/// <summary>
/// Classe base com os [Fact] de regras Code Style (CS001-CS004).
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

    [Fact]
    public void CS004a_Factories_devem_retornar_apenas_um_tipo()
    {
        AssertNoViolations(new CS004a_FactorySingleReturnTypeRule());
    }

    [Fact]
    public void CS004b_Nome_da_factory_deve_corresponder_ao_tipo_de_retorno()
    {
        AssertNoViolations(new CS004b_FactoryNamingConventionRule());
    }

    [Fact]
    public void CS004c_Factories_devem_estar_no_namespace_correto_conforme_tipo_de_retorno()
    {
        AssertNoViolations(new CS004c_FactoryNamespaceRule());
    }
}
