using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Tests;

/// <summary>
/// Classe base com o [Fact] de regra de infraestrutura (IN001).
/// Todos os projetos de ArchitectureTests herdam esta classe.
/// </summary>
public abstract class InfrastructureRuleTestsBase<TFixture> : RuleTestBase<TFixture>
    where TFixture : RuleFixture
{
    protected InfrastructureRuleTestsBase(TFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void IN001_Camadas_de_bounded_context_devem_seguir_grafo_de_dependencias()
    {
        AssertNoViolations(new IN001_CanonicalLayerDependenciesRule());
    }
}
