using Bedrock.BuildingBlocks.Testing.Architecture.Rules.OutboxRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Tests;

/// <summary>
/// Classe base com os [Fact] de regras Outbox (OB006-OB011).
/// Projetos de ArchitectureTests que analisam outbox herdam esta classe.
/// </summary>
public abstract class OutboxRuleTestsBase<TFixture> : RuleTestBase<TFixture>
    where TFixture : RuleFixture
{
    protected OutboxRuleTestsBase(TFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void OB006a_Repositorios_outbox_devem_herdar_de_OutboxPostgreSqlRepositoryBase()
    {
        AssertNoViolations(new OB006a_OutboxRepositoryInheritsBaseRule());
    }

    [Fact]
    public void OB006b_Repositorios_outbox_devem_ser_sealed()
    {
        AssertNoViolations(new OB006b_OutboxRepositorySealedRule());
    }

    [Fact]
    public void OB008_Writers_outbox_devem_usar_composicao_nao_heranca()
    {
        AssertNoViolations(new OB008_OutboxWriterCompositionRule());
    }

    [Fact]
    public void OB009_ConfigureInternal_deve_chamar_WithTableName()
    {
        AssertNoViolations(new OB009_OutboxRepositoryWithTableNameRule());
    }

    [Fact]
    public void OB011a_Repositorios_outbox_devem_implementar_marker_interface()
    {
        AssertNoViolations(new OB011a_OutboxRepositoryMarkerInterfaceRule());
    }

    [Fact]
    public void OB011b_Writers_outbox_devem_implementar_marker_interface()
    {
        AssertNoViolations(new OB011b_OutboxWriterMarkerInterfaceRule());
    }
}
