using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Templates.Domain.Entities.AbstractAggregateRoots.Base;
using Templates.Domain.Entities.AbstractAggregateRoots.Enums;
using Templates.Domain.Entities.AbstractAggregateRoots.Inputs;

namespace Templates.Domain.Entities.AbstractAggregateRoots;

/// <summary>
/// Classe concreta (leaf) que herda de AbstractAggregateRoot para CategoryType.TypeB.
/// O CategoryType é fixo e definido automaticamente no RegisterNew.
/// </summary>
public sealed class LeafAggregateRootTypeB
    : AbstractAggregateRoot
{
    // Metadata (DE-012 a DE-016)
    public static class LeafAggregateRootTypeBMetadata
    {
        // Fields
        private static readonly Lock _lockObject = new();

        // LeafProperty
        public static readonly string LeafPropertyPropertyName = nameof(LeafProperty);
        public static bool LeafPropertyIsRequired { get; private set; } = true;
        public static int LeafPropertyMinLength { get; private set; } = 1;
        public static int LeafPropertyMaxLength { get; private set; } = 100;

        public static void ChangeLeafPropertyMetadata(
            bool isRequired,
            int minLength,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                LeafPropertyIsRequired = isRequired;
                LeafPropertyMinLength = minLength;
                LeafPropertyMaxLength = maxLength;
            }
        }
    }

    // Properties
    public string LeafProperty { get; private set; } = string.Empty;

    // Constructors (DE-020, DE-052)
    private LeafAggregateRootTypeB() : base()
    {
    }

    private LeafAggregateRootTypeB(
        EntityInfo entityInfo,
        string sampleProperty,
        CategoryType categoryType,
        string leafProperty
    ) : base(entityInfo, sampleProperty, categoryType)
    {
        LeafProperty = leafProperty;
    }

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_GUIDANCE: RegisterNew com CategoryType Fixo
    ───────────────────────────────────────────────────────────────────────────────

    Esta classe representa entidades do tipo CategoryType.TypeB.
    O CategoryType é definido automaticamente no baseInputFactory, não pelo caller.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: CategoryType Fixo Por Classe Concreta
    ───────────────────────────────────────────────────────────────────────────────

    Cada classe concreta que representa um tipo específico DEVE:
    ✅ Definir o CategoryType fixo no baseInputFactory
    ✅ NÃO expor CategoryType no Input do RegisterNew
    ✅ Garantir que apenas entidades do tipo correto sejam criadas

    ───────────────────────────────────────────────────────────────────────────────
    */
    public static LeafAggregateRootTypeB? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewLeafTypeBInput input
    )
    {
        return RegisterNewBase(
            executionContext,
            input,
            concreteTypeFactory: static (executionContext, input) => new LeafAggregateRootTypeB(),
            baseInputFactory: static (executionContext, input) => new Base.Inputs.RegisterNewAbstractAggregateRootInput(
                input.SampleProperty,
                CategoryType.TypeB  // CategoryType fixo para esta classe
            ),
            handler: static (executionContext, input, instance) =>
            {
                return
                    instance.ChangeLeafPropertyInternal(executionContext, input.LeafProperty);
            }
        );
    }

    // Reconstitution sem validação (DE-018)
    public static LeafAggregateRootTypeB CreateFromExistingInfo(
        CreateFromExistingInfoLeafTypeBInput input
    )
    {
        return new LeafAggregateRootTypeB(
            input.EntityInfo,
            input.SampleProperty,
            input.CategoryType,
            input.LeafProperty
        );
    }

    // Clone (DE-003)
    public override AbstractAggregateRoot Clone()
    {
        return new LeafAggregateRootTypeB(
            EntityInfo,
            SampleProperty,
            CategoryType,
            LeafProperty
        );
    }

    // Métodos públicos de negócio (DE-050)
    public LeafAggregateRootTypeB? ChangeLeafProperty(
        ExecutionContext executionContext,
        ChangeLeafPropertyInput input
    )
    {
        return RegisterChangeInternal<LeafAggregateRootTypeB, ChangeLeafPropertyInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeLeafPropertyInternal(executionContext, input.LeafProperty);
            }
        );
    }

    public LeafAggregateRootTypeB? ChangeSampleProperty(
        ExecutionContext executionContext,
        ChangeSamplePropertyInput input
    )
    {
        return RegisterChangeInternal<LeafAggregateRootTypeB, ChangeSamplePropertyInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeSamplePropertyInternal(executionContext, input.SampleProperty);
            }
        );
    }

    // Validation Methods (DE-009, DE-010, DE-011)
    public static bool ValidateLeafProperty(
        ExecutionContext executionContext,
        string? leafProperty
    )
    {
        bool leafPropertyIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<LeafAggregateRootTypeB>(propertyName: LeafAggregateRootTypeBMetadata.LeafPropertyPropertyName),
            isRequired: LeafAggregateRootTypeBMetadata.LeafPropertyIsRequired,
            value: leafProperty
        );

        if (!leafPropertyIsRequiredValidation)
            return false;

        bool leafPropertyMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<LeafAggregateRootTypeB>(propertyName: LeafAggregateRootTypeBMetadata.LeafPropertyPropertyName),
            minLength: LeafAggregateRootTypeBMetadata.LeafPropertyMinLength,
            value: leafProperty!.Length
        );

        bool leafPropertyMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<LeafAggregateRootTypeB>(propertyName: LeafAggregateRootTypeBMetadata.LeafPropertyPropertyName),
            maxLength: LeafAggregateRootTypeBMetadata.LeafPropertyMaxLength,
            value: leafProperty!.Length
        );

        return leafPropertyIsRequiredValidation
            && leafPropertyMinLengthValidation
            && leafPropertyMaxLengthValidation;
    }

    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? sampleProperty,
        CategoryType? categoryType,
        string? leafProperty
    )
    {
#pragma warning disable IDE0002 // Simplify Member Access
        return AbstractAggregateRoot.IsValid(executionContext, entityInfo, sampleProperty, categoryType)
            & ValidateLeafProperty(executionContext, leafProperty);
#pragma warning restore IDE0002 // Simplify Member Access
    }

    protected override bool IsValidConcreteInternal(ExecutionContext executionContext)
    {
        return ValidateLeafProperty(executionContext, LeafProperty);
    }

    // Internal Methods (DE-049)
    private bool ChangeLeafPropertyInternal(
        ExecutionContext executionContext,
        string leafProperty
    )
    {
        return SetLeafProperty(executionContext, leafProperty);
    }

    // Set Methods (DE-022, DE-047)
    private bool SetLeafProperty(
        ExecutionContext executionContext,
        string leafProperty
    )
    {
        bool isValid = ValidateLeafProperty(
            executionContext,
            leafProperty
        );

        if (!isValid)
            return false;

        LeafProperty = leafProperty;

        return true;
    }
}
