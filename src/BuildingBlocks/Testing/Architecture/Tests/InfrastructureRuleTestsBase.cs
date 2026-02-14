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

    [Fact]
    public void IN002_Domain_Entities_devem_ter_zero_dependencias_externas()
    {
        AssertNoViolations(new IN002_DomainEntitiesZeroExternalDependenciesRule());
    }

    [Fact]
    public void IN003_Domain_deve_depender_apenas_de_DomainEntities_Configuration_e_framework()
    {
        AssertNoViolations(new IN003_DomainZeroExternalDependenciesRule());
    }

    [Fact]
    public void IN006_Infra_Data_Tech_deve_ter_marker_interface_de_conexao()
    {
        AssertNoViolations(new IN006_ConnectionMarkerInterfaceRule());
    }

    [Fact]
    public void IN007_Infra_Data_Tech_deve_ter_marker_interface_de_UnitOfWork()
    {
        AssertNoViolations(new IN007_UnitOfWorkMarkerInterfaceRule());
    }

    [Fact]
    public void IN008_Implementacao_de_conexao_deve_ser_sealed_e_herdar_base()
    {
        AssertNoViolations(new IN008_ConnectionImplementationSealedRule());
    }

    [Fact]
    public void IN009_Implementacao_de_UnitOfWork_deve_ser_sealed_e_herdar_base()
    {
        AssertNoViolations(new IN009_UnitOfWorkImplementationSealedRule());
    }

    [Fact]
    public void IN010_DataModel_deve_herdar_DataModelBase()
    {
        AssertNoViolations(new IN010_DataModelInheritsDataModelBaseRule());
    }

    [Fact]
    public void IN011_DataModelRepository_deve_implementar_IDataModelRepository_e_herdar_base()
    {
        AssertNoViolations(new IN011_DataModelRepositoryImplementsBaseRule());
    }

    [Fact]
    public void IN012_Repositorio_tecnologico_deve_implementar_IRepository_e_ser_sealed()
    {
        AssertNoViolations(new IN012_TechRepositoryImplementsIRepositoryRule());
    }

    [Fact]
    public void IN013_DataModel_deve_ter_factories_bidirecionais()
    {
        AssertNoViolations(new IN013_BidirectionalFactoriesRule());
    }

    [Fact]
    public void IN014_DataModel_deve_ter_adapter_para_atualizacao()
    {
        AssertNoViolations(new IN014_DataModelAdapterRule());
    }

    [Fact]
    public void IN015_Projeto_Infra_Data_Tech_deve_ter_estrutura_canonica_de_pastas()
    {
        AssertNoViolations(new IN015_CanonicalFolderStructureRule());
    }
}
