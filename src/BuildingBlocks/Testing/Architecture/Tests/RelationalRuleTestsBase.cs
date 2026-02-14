using Bedrock.BuildingBlocks.Testing.Architecture.Rules.RelationalRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Tests;

/// <summary>
/// Classe base com [Fact]s de regras de banco relacional (RL).
/// Projetos de ArchitectureTests de Infra.Data.{Tech} relacional herdam esta classe.
/// </summary>
public abstract class RelationalRuleTestsBase<TFixture> : RuleTestBase<TFixture>
    where TFixture : RuleFixture
{
    protected RelationalRuleTestsBase(TFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void RL001_Mapper_deve_herdar_DataModelMapperBase_e_sobrescrever_metodos()
    {
        AssertNoViolations(new RL001_MapperInheritsDataModelMapperBaseRule());
    }
}
