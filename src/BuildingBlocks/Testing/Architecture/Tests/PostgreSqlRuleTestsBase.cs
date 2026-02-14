using Bedrock.BuildingBlocks.Testing.Architecture.Rules.PostgreSqlRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Tests;

/// <summary>
/// Classe base com [Fact]s de regras especificas do PostgreSQL (PG).
/// Projetos de ArchitectureTests de Infra.Data.PostgreSql herdam esta classe.
/// </summary>
public abstract class PostgreSqlRuleTestsBase<TFixture> : RuleTestBase<TFixture>
    where TFixture : RuleFixture
{
    protected PostgreSqlRuleTestsBase(TFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void PG001_MapBinaryImporter_deve_escrever_todas_as_colunas_mapeadas()
    {
        AssertNoViolations(new PG001_MapBinaryImporterWritesAllColumnsRule());
    }

    [Fact]
    public void PG002_ConfigureInternal_da_conexao_deve_validar_connection_string()
    {
        AssertNoViolations(new PG002_ConnectionValidatesConnectionStringRule());
    }
}
