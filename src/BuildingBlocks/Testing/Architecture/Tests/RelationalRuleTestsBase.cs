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

    [Fact]
    public void RL002_ConfigureInternal_do_mapper_deve_chamar_MapTable()
    {
        AssertNoViolations(new RL002_MapperConfigureInternalCallsMapTableRule());
    }

    [Fact]
    public void RL003_Literais_SQL_proibidos_fora_de_Mappers()
    {
        AssertNoViolations(new RL003_NoSqlLiteralsOutsideMappersRule());
    }

    [Fact]
    public void RL004_DataModel_deve_ter_apenas_propriedades_primitivas()
    {
        AssertNoViolations(new RL004_DataModelOnlyPrimitivePropertiesRule());
    }
}
