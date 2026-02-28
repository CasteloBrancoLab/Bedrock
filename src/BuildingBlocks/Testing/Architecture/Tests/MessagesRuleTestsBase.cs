using Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Tests;

/// <summary>
/// Classe base com os 10 [Fact] de regras Messages (MS001-MS008).
/// Projetos de ArchitectureTests que analisam mensagens herdam esta classe.
/// </summary>
public abstract class MessagesRuleTestsBase<TFixture> : RuleTestBase<TFixture>
    where TFixture : RuleFixture
{
    protected MessagesRuleTestsBase(TFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void MS001_Mensagens_concretas_nao_devem_duplicar_campos_de_MessageMetadata()
    {
        AssertNoViolations(new MS001_NoMetadataDuplicationRule());
    }

    [Fact]
    public void MS003_MessageMetadata_deve_ser_sealed_record_com_primitivos()
    {
        AssertNoViolations(new MS003_MetadataSealedRecordRule());
    }

    [Fact]
    public void MS005_Tipos_base_devem_ser_abstract_records()
    {
        AssertNoViolations(new MS005_BaseTypesAbstractRecordRule());
    }

    [Fact]
    public void MS006a_Commands_devem_terminar_com_sufixo_Command()
    {
        AssertNoViolations(new MS006a_CommandNamingSuffixRule());
    }

    [Fact]
    public void MS006b_Events_devem_terminar_com_sufixo_Event()
    {
        AssertNoViolations(new MS006b_EventNamingSuffixRule());
    }

    [Fact]
    public void MS006c_Queries_devem_terminar_com_sufixo_Query()
    {
        AssertNoViolations(new MS006c_QueryNamingSuffixRule());
    }

    [Fact]
    public void MS007a_Concretos_devem_herdar_de_base_tipada_nao_MessageBase()
    {
        AssertNoViolations(new MS007a_ConcreteInheritsTypedBaseRule());
    }

    [Fact]
    public void MS007b_Concretos_devem_ser_sealed_record()
    {
        AssertNoViolations(new MS007b_ConcreteSealedRecordRule());
    }

    [Fact]
    public void MS007c_Primeiro_parametro_deve_ser_MessageMetadata_Metadata()
    {
        AssertNoViolations(new MS007c_FirstParameterMetadataRule());
    }

    [Fact]
    public void MS008_Parametros_devem_usar_apenas_primitivos_e_record_structs()
    {
        AssertNoViolations(new MS008_PrimitivesOnlyRule());
    }
}
