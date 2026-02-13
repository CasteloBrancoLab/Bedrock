using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Tests;

/// <summary>
/// Classe base com os 60 [Fact] de regras Domain.Entities (DE001-DE060).
/// Projetos Domain.Entities herdam esta classe com seu fixture especifico.
/// </summary>
public abstract class DomainEntitiesRuleTestsBase<TFixture> : RuleTestBase<TFixture>
    where TFixture : RuleFixture
{
    protected DomainEntitiesRuleTestsBase(TFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    #region DE001-DE010

    [Fact]
    public void DE001_Classes_sem_herdeiros_devem_ser_sealed()
    {
        AssertNoViolations(new DE001_SealedClassRule());
    }

    [Fact]
    public void DE002_Classes_devem_ter_apenas_construtores_privados()
    {
        AssertNoViolations(new DE002_PrivateConstructorRule());
    }

    [Fact]
    public void DE003_Metodos_publicos_de_instancia_devem_seguir_clone_modify_return()
    {
        AssertNoViolations(new DE003_CloneModifyReturnRule());
    }

    [Fact]
    public void DE004_Entidades_concretas_devem_ter_factory_method_RegisterNew_retornando_nullable()
    {
        AssertNoViolations(new DE004_InvalidStateNeverExistsRule());
    }

    [Fact]
    public void DE005_Aggregate_roots_devem_implementar_IAggregateRoot()
    {
        AssertNoViolations(new DE005_AggregateRootInterfaceRule());
    }

    [Fact]
    public void DE006_Metodos_Internal_devem_usar_operador_bitwise_and()
    {
        AssertNoViolations(new DE006_BitwiseAndForValidationRule());
    }

    [Fact]
    public void DE007_Metodos_de_entidades_devem_retornar_nullable_ao_inves_de_result_pattern()
    {
        AssertNoViolations(new DE007_NullableReturnOverResultPatternRule());
    }

    [Fact]
    public void DE008_Entidades_nao_devem_lancar_excecoes_para_validacao_de_negocio()
    {
        AssertNoViolations(new DE008_ExceptionsVsNullableReturnRule());
    }

    [Fact]
    public void DE009_Metodos_validate_devem_ser_publicos_e_estaticos()
    {
        AssertNoViolations(new DE009_ValidateMethodsPublicStaticRule());
    }

    [Fact]
    public void DE010_Metodos_validate_devem_usar_validation_utils()
    {
        AssertNoViolations(new DE010_ValidationUtilsForStandardValidationsRule());
    }

    #endregion

    #region DE011-DE020

    [Fact]
    public void DE011_Parametros_validate_devem_ser_nullable()
    {
        AssertNoViolations(new DE011_ValidateParametersNullableRule());
    }

    [Fact]
    public void DE012_Entidades_nao_devem_usar_data_annotations()
    {
        AssertNoViolations(new DE012_StaticMetadataOverDataAnnotationsRule());
    }

    [Fact]
    public void DE013_Metadados_devem_seguir_convencao_PropertyName_ConstraintType()
    {
        AssertNoViolations(new DE013_MetadataNamingConventionRule());
    }

    [Fact]
    public void DE014_Metadados_devem_ser_inicializados_inline()
    {
        AssertNoViolations(new DE014_InlineMetadataInitializationRule());
    }

    [Fact]
    public void DE015_Metodos_ChangeMetadata_devem_usar_lock()
    {
        AssertNoViolations(new DE015_ChangeMetadataUsesLockRule());
    }

    [Fact]
    public void DE016_Metodos_validate_devem_referenciar_metadata()
    {
        AssertNoViolations(new DE016_ValidateUsesMetadataRule());
    }

    [Fact]
    public void DE017_Entidades_devem_ter_CreateFromExistingInfo_retornando_non_nullable()
    {
        AssertNoViolations(new DE017_RegisterNewAndCreateFromExistingInfoRule());
    }

    [Fact]
    public void DE018_CreateFromExistingInfo_nao_deve_chamar_Validate()
    {
        AssertNoViolations(new DE018_ReconstitutionDoesNotValidateRule());
    }

    [Fact]
    public void DE019_Factory_methods_devem_receber_input_objects()
    {
        AssertNoViolations(new DE019_InputObjectsPatternRule());
    }

    [Fact]
    public void DE020_Entidades_devem_ter_dois_construtores_privados()
    {
        AssertNoViolations(new DE020_TwoPrivateConstructorsRule());
    }

    #endregion

    #region DE021-DE030

    [Fact]
    public void DE021_Metodos_publicos_Change_devem_ter_Internal_correspondente()
    {
        AssertNoViolations(new DE021_PublicMethodsDelegateToInternalRule());
    }

    [Fact]
    public void DE022_Metodos_Set_devem_ser_privados()
    {
        AssertNoViolations(new DE022_SetMethodsPrivateRule());
    }

    [Fact]
    public void DE023_RegisterInternal_deve_ser_chamado_uma_unica_vez()
    {
        AssertNoViolations(new DE023_RegisterInternalCalledOnceRule());
    }

    [Fact]
    public void DE024_Metodos_publicos_nao_devem_chamar_outros_publicos()
    {
        AssertNoViolations(new DE024_PublicMethodNeverCallsPublicRule());
    }

    [Fact]
    public void DE025_Metodos_Internal_com_multiplos_Set_devem_usar_isSuccess()
    {
        AssertNoViolations(new DE025_IntermediateVariablesInValidationRule());
    }

    [Fact]
    public void DE026_Propriedades_publicas_nao_devem_usar_expression_body()
    {
        AssertNoViolations(new DE026_DerivedPropertiesStoredRule());
    }

    [Fact]
    public void DE027_Entidades_nao_devem_ter_dependencias_externas()
    {
        AssertNoViolations(new DE027_NoExternalDependenciesRule());
    }

    [Fact]
    public void DE028_ExecutionContext_deve_ser_primeiro_parametro()
    {
        AssertNoViolations(new DE028_ExecutionContextFirstParameterRule());
    }

    [Fact]
    public void DE029_Entidades_nao_devem_usar_DateTime_Now_diretamente()
    {
        AssertNoViolations(new DE029_TimeProviderViaExecutionContextRule());
    }

    [Fact]
    public void DE030_Metodos_Validate_devem_usar_CreateMessageCode()
    {
        AssertNoViolations(new DE030_MessageCodesWithCreateMessageCodeRule());
    }

    #endregion

    #region DE031-DE040

    [Fact]
    public void DE031_Metadados_de_infraestrutura_devem_vir_de_EntityInfo()
    {
        AssertNoViolations(new DE031_EntityInfoManagedByBaseRule());
    }

    [Fact]
    public void DE032_Construtores_com_parametros_devem_receber_EntityInfo()
    {
        AssertNoViolations(new DE032_OptimisticLockingViaEntityInfoRule());
    }

    [Fact]
    public void DE033_Entidades_nao_devem_ser_readonly_struct()
    {
        AssertNoViolations(new DE033_NotReadonlyStructRule());
    }

    [Fact]
    public void DE034_Metodos_publicos_nao_devem_retornar_void()
    {
        AssertNoViolations(new DE034_NoVoidMutationMethodsRule());
    }

    [Fact]
    public void DE035_Construtores_nao_devem_validar()
    {
        AssertNoViolations(new DE035_ConstructorDoesNotValidateRule());
    }

    [Fact]
    public void DE036_Colecoes_filhas_devem_ser_field_privado()
    {
        AssertNoViolations(new DE036_ChildCollectionPrivateListFieldRule());
    }

    [Fact]
    public void DE037_Propriedades_publicas_de_colecao_devem_retornar_IReadOnlyList()
    {
        AssertNoViolations(new DE037_PublicPropertyIReadOnlyListRule());
    }

    [Fact]
    public void DE038_Fields_de_colecao_devem_ser_inicializados()
    {
        AssertNoViolations(new DE038_CollectionFieldAlwaysInitializedRule());
    }

    [Fact]
    public void DE039_Construtores_devem_fazer_copia_defensiva_de_colecoes()
    {
        AssertNoViolations(new DE039_DefensiveCopyCollectionInConstructorRule());
    }

    [Fact]
    public void DE040_Entidades_filhas_devem_ser_processadas_uma_a_uma()
    {
        AssertNoViolations(new DE040_ChildEntityProcessedOneByOneRule());
    }

    #endregion

    #region DE041-DE050

    [Fact]
    public void DE041_Validacao_de_entidades_filhas_deve_ser_especifica_por_operacao()
    {
        AssertNoViolations(new DE041_OperationSpecificChildValidationRule());
    }

    [Fact]
    public void DE042_Operacoes_de_modificacao_devem_localizar_filha_por_Id()
    {
        AssertNoViolations(new DE042_ChildEntityLookupByIdRule());
    }

    [Fact]
    public void DE043_Modificacao_de_filha_deve_ser_via_metodo_de_negocio()
    {
        AssertNoViolations(new DE043_ChildModificationViaBusinessMethodRule());
    }

    [Fact]
    public void DE044_Colecoes_nao_devem_ter_metodo_Set()
    {
        AssertNoViolations(new DE044_NoSetMethodForCollectionsRule());
    }

    [Fact]
    public void DE045_Validacao_de_duplicidade_deve_ignorar_propria_entidade()
    {
        AssertNoViolations(new DE045_DuplicateValidationIgnoresSelfRule());
    }

    [Fact]
    public void DE046_Enumeracoes_devem_seguir_convencoes()
    {
        AssertNoViolations(new DE046_EnumConventionsRule());
    }

    [Fact]
    public void DE047_Set_devem_ser_privados_em_classes_abstratas()
    {
        AssertNoViolations(new DE047_SetMethodPrivateInAbstractClassesRule());
    }

    [Fact]
    public void DE048_Validate_devem_ser_publicos_e_estaticos_em_classes_abstratas()
    {
        AssertNoViolations(new DE048_ValidateMethodPublicInAbstractClassesRule());
    }

    [Fact]
    public void DE049_Internal_devem_ser_protegidos_em_classes_abstratas()
    {
        AssertNoViolations(new DE049_InternalMethodProtectedInAbstractClassesRule());
    }

    [Fact]
    public void DE050_Classes_abstratas_nao_devem_ter_metodos_publicos_de_negocio()
    {
        AssertNoViolations(new DE050_NoPublicBusinessMethodsInAbstractClassesRule());
    }

    #endregion

    #region DE051-DE060

    [Fact]
    public void DE051_Classes_abstratas_devem_ter_IsValidConcreteInternal_abstrato()
    {
        AssertNoViolations(new DE051_IsValidHierarchyInAbstractClassesRule());
    }

    [Fact]
    public void DE052_Construtores_devem_ser_protegidos_em_classes_abstratas()
    {
        AssertNoViolations(new DE052_ProtectedConstructorsInAbstractClassesRule());
    }

    [Fact]
    public void DE053_Classes_abstratas_com_validacao_devem_ter_Metadata()
    {
        AssertNoViolations(new DE053_MetadataInAbstractClassesRule());
    }

    [Fact]
    public void DE054_Hierarquia_de_heranca_nao_deve_ser_profunda()
    {
        AssertNoViolations(new DE054_MaxInheritanceDepthRule());
    }

    [Fact]
    public void DE055_Classes_abstratas_devem_ter_RegisterNewBase()
    {
        AssertNoViolations(new DE055_RegisterNewBaseInAbstractClassesRule());
    }

    [Fact]
    public void DE056_Classes_abstratas_nao_devem_ter_CreateFromExistingInfo()
    {
        AssertNoViolations(new DE056_NoCreateFromExistingInfoInAbstractClassesRule());
    }

    [Fact]
    public void DE057_Metadata_de_ARs_associadas_deve_ter_apenas_IsRequired()
    {
        AssertNoViolations(new DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRule());
    }

    [Fact]
    public void DE058_ARs_associadas_devem_ter_Process_Validate_Set()
    {
        AssertNoViolations(new DE058_ProcessValidateSetForAssociatedAggregateRootsRule());
    }

    [Fact]
    public void DE059_Metadata_deve_ser_classe_aninhada_da_entidade()
    {
        AssertNoViolations(new DE059_NestedMetadataClassRule());
    }

    [Fact]
    public void DE060_Interface_de_dominio_de_aggregate_root_deve_herdar_IAggregateRoot()
    {
        AssertNoViolations(new DE060_DomainInterfaceMustDeclareAggregateRootRule());
    }

    #endregion
}
