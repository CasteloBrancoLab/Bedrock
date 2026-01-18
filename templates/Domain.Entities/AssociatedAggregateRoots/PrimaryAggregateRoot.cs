using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Templates.Domain.Entities.AssociatedAggregateRoots.Inputs;

namespace Templates.Domain.Entities.AssociatedAggregateRoots;

public sealed class PrimaryAggregateRoot
    : EntityBase<PrimaryAggregateRoot>,
    IAggregateRoot
{
    // Metadata
    public static class PrimaryAggregateRootMetadata
    {
        // Fields
        private static readonly Lock _lockObject = new();

        // Quantity
        public static readonly string QuantityPropertyName = nameof(Quantity);
        public static bool QuantityIsRequired { get; private set; } = true;
        public static int QuantityMinValue { get; private set; }
        public static int QuantityMaxValue { get; private set; } = 1000;

        /*
        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Metadata de Aggregate Roots Associadas - Apenas IsRequired
        ───────────────────────────────────────────────────────────────────────────────

        Para associações entre Aggregate Roots, a ÚNICA validação no metadata é IsRequired.

        DIFERENÇA PARA PROPRIEDADES SIMPLES:
        - Propriedades simples (string, int): MinLength, MaxLength, MinValue, MaxValue
        - Aggregate Roots associadas: APENAS IsRequired

        RAZÃO:
        Aggregate Roots têm ciclo de vida INDEPENDENTE e validações PRÓPRIAS.
        A entidade associada valida a si mesma via seu próprio IsValid().
        A entidade principal valida apenas SE a associação é obrigatória.

        VALIDAÇÕES ESPECÍFICAS POR OPERAÇÃO:
        Regras de negócio contextuais (ex: "não pode associar AR inativa") são
        tratadas nos métodos Validate*For*Internal, NÃO no metadata.

        ───────────────────────────────────────────────────────────────────────────────
        */

        // ReferencedAggregateRoot
        public static readonly string ReferencedAggregateRootPropertyName = nameof(ReferencedAggregateRoot);
        public static bool ReferencedAggregateRootIsRequired { get; private set; } = true;

        // Public Methods
        public static void ChangeQuantityMetadata(
            bool isRequired,
            int minValue,
            int maxValue
        )
        {
            lock (_lockObject)
            {
                QuantityIsRequired = isRequired;
                QuantityMinValue = minValue;
                QuantityMaxValue = maxValue;
            }
        }

        public static void ChangeReferencedAggregateRootMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                ReferencedAggregateRootIsRequired = isRequired;
            }
        }
    }

    // Properties
    public int Quantity { get; private set; }
    public ReferencedAggregateRoot? ReferencedAggregateRoot { get; private set; }

    // Constructors
    private PrimaryAggregateRoot()
    {
    }

    private PrimaryAggregateRoot(
        EntityInfo entityInfo,
        int quantity,
        ReferencedAggregateRoot? referencedAggregateRoot
    ) : base(entityInfo)
    {
        Quantity = quantity;
        ReferencedAggregateRoot = referencedAggregateRoot;
    }

    // Public Business Methods
    public static PrimaryAggregateRoot? RegisterNew(
        ExecutionContext executionContext,
        PrimaryRegisterNewInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: (executionContext, input) => new PrimaryAggregateRoot(),
            handler: static (executionContext, input, instance) =>
            {
                bool isValid = instance.ChangeQuantityInternal(executionContext, input.Quantity);

                isValid &= instance.ProcessReferencedAggregateRootForRegisterNewInternal(
                    executionContext,
                    input.ReferencedAggregateRoot
                );

                return isValid;
            }
        );
    }

    public static PrimaryAggregateRoot CreateFromExistingInfo(
        PrimaryCreateFromExistingInfoInput input
    )
    {
        return new PrimaryAggregateRoot(
            input.EntityInfo,
            input.Quantity,
            input.ReferencedAggregateRoot
        );
    }

    public PrimaryAggregateRoot? ChangeQuantity(
        ExecutionContext executionContext,
        PrimaryChangeQuantityInput input
    )
    {
        return RegisterChangeInternal<PrimaryAggregateRoot, PrimaryChangeQuantityInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeQuantityInternal(executionContext, input.Quantity);
            }
        );
    }

    public PrimaryAggregateRoot? ChangeReferencedAggregateRoot(
        ExecutionContext executionContext,
        PrimaryChangeReferencedAggregateRootInput input
    )
    {
        return RegisterChangeInternal<PrimaryAggregateRoot, PrimaryChangeReferencedAggregateRootInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ProcessReferencedAggregateRootForChangeInternal(
                    executionContext,
                    input.ReferencedAggregateRoot
                );
            }
        );
    }

    public override PrimaryAggregateRoot Clone()
    {
        return new PrimaryAggregateRoot(
            EntityInfo,
            Quantity,
            ReferencedAggregateRoot
        );
    }

    // Private Business Methods
    private bool ChangeQuantityInternal(
        ExecutionContext executionContext,
        int quantity
    )
    {
        return SetQuantity(executionContext, quantity);
    }


    private bool ProcessReferencedAggregateRootForRegisterNewInternal(
        ExecutionContext executionContext,
        ReferencedAggregateRoot? referencedAggregateRoot
    )
    {
        bool isValid = ValidateReferencedAggregateRootForRegisterNewInternal(
            executionContext,
            referencedAggregateRoot
        );

        if (!isValid)
            return false;

        return SetReferencedAggregateRoot(executionContext, referencedAggregateRoot);
    }

    private bool ProcessReferencedAggregateRootForChangeInternal(
        ExecutionContext executionContext,
        ReferencedAggregateRoot? referencedAggregateRoot
    )
    {
        bool isValid = ValidateReferencedAggregateRootForChangeInternal(
            executionContext,
            referencedAggregateRoot
        );

        if (!isValid)
            return false;

        return SetReferencedAggregateRoot(executionContext, referencedAggregateRoot);
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        int? quantity,
        ReferencedAggregateRoot? referencedAggregateRoot = null
    )
    {
        bool isValid =
            EntityBaseIsValid(executionContext, entityInfo)
            & ValidateQuantity(executionContext, quantity);

        if (referencedAggregateRoot is not null)
        {
            isValid &= referencedAggregateRoot.IsValid(executionContext);
        }

        return isValid;
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            Quantity,
            ReferencedAggregateRoot
        );
    }

    public static bool ValidateQuantity(
        ExecutionContext executionContext,
        int? quantity
    )
    {
        bool quantityIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<PrimaryAggregateRoot>(propertyName: PrimaryAggregateRootMetadata.QuantityPropertyName),
            isRequired: PrimaryAggregateRootMetadata.QuantityIsRequired,
            value: quantity
        );

        if (!quantityIsRequiredValidation)
            return false;

        bool quantityMinValueValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<PrimaryAggregateRoot>(propertyName: PrimaryAggregateRootMetadata.QuantityPropertyName),
            minLength: PrimaryAggregateRootMetadata.QuantityMinValue,
            value: quantity!.Value
        );

        bool quantityMaxValueValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<PrimaryAggregateRoot>(propertyName: PrimaryAggregateRootMetadata.QuantityPropertyName),
            maxLength: PrimaryAggregateRootMetadata.QuantityMaxValue,
            value: quantity!.Value
        );

        return quantityIsRequiredValidation
            && quantityMinValueValidation
            && quantityMaxValueValidation;
    }

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Validação de IsRequired para Entidade Associada (Aggregate Root)
    ───────────────────────────────────────────────────────────────────────────────

    Para Aggregate Roots associadas, a única validação no metadata é IsRequired.
    Este método verifica se a entidade associada é obrigatória e se foi fornecida.

    DIFERENÇA PARA ENTIDADES FILHAS:
    ✅ Recebe a instância já criada (não o input de criação)
    ✅ Aggregate Root associada tem ciclo de vida independente

    ───────────────────────────────────────────────────────────────────────────────
    */
    public static bool ValidateReferencedAggregateRootIsRequired(
        ExecutionContext executionContext,
        ReferencedAggregateRoot? referencedAggregateRoot
    )
    {
        bool isRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<PrimaryAggregateRoot>(propertyName: PrimaryAggregateRootMetadata.ReferencedAggregateRootPropertyName),
            isRequired: PrimaryAggregateRootMetadata.ReferencedAggregateRootIsRequired,
            value: referencedAggregateRoot
        );

        return isRequiredValidation;
    }

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Validação de Entidade Associada Específica por Operação
    ───────────────────────────────────────────────────────────────────────────────

    Toda entidade associada DEVE ser validada individualmente durante o processamento.

    POR QUE VALIDAÇÃO ESPECÍFICA POR OPERAÇÃO:
    ✅ Regras de validação podem variar conforme a operação de negócio (ex: RegisterNew vs Change)
    ✅ Método privado garante encapsulamento das regras específicas
    ✅ Aggregate Root mantém controle total sobre validação do agregado

    PADRÃO DE NOMENCLATURA:
    Validate[NomeDaEntidadeAssociada]For[NomeDaOperação]Internal
    Exemplo: ValidateReferencedAggregateRootForRegisterNewInternal

    ───────────────────────────────────────────────────────────────────────────────
    */
    private static bool ValidateReferencedAggregateRootForRegisterNewInternal(
        ExecutionContext executionContext,
        ReferencedAggregateRoot? referencedAggregateRoot
    )
    {
        // Se a entidade for null, a validação de IsRequired será feita no Set*
        if (referencedAggregateRoot is null)
            return true;

        // Aqui podem ser adicionadas validações específicas para RegisterNew
        // Ex: verificar se o SampleName não conflita com alguma regra específica

        return referencedAggregateRoot.IsValid(executionContext);
    }

    private static bool ValidateReferencedAggregateRootForChangeInternal(
        ExecutionContext executionContext,
        ReferencedAggregateRoot? referencedAggregateRoot
    )
    {
        // Se a entidade for null, a validação de IsRequired será feita no Set*
        if (referencedAggregateRoot is null)
            return true;

        // Aqui podem ser adicionadas validações específicas para Change
        // Ex: verificar se a mudança é permitida dado o estado atual

        return referencedAggregateRoot.IsValid(executionContext);
    }

    // Set Methods
    private bool SetQuantity(
        ExecutionContext executionContext,
        int quantity
    )
    {
        bool isValid = ValidateQuantity(
            executionContext,
            quantity
        );

        if (!isValid)
            return false;

        Quantity = quantity;

        return true;
    }

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Método Set* para Aggregate Root Associada
    ───────────────────────────────────────────────────────────────────────────────

    Diferente de coleções de entidades filhas, uma Aggregate Root associada singular
    DEVE ter um método Set* privado que:
    ✅ Valida IsRequired (única validação no metadata para associações)
    ✅ Atribui a instância à propriedade

    DIFERENÇA PARA COLEÇÕES:
    - Coleções: NÃO têm Set* (gerenciadas via Add/Remove)
    - Associação singular: TEM Set* privado (validação de IsRequired)

    ───────────────────────────────────────────────────────────────────────────────
    */
    private bool SetReferencedAggregateRoot(
        ExecutionContext executionContext,
        ReferencedAggregateRoot? referencedAggregateRoot
    )
    {
        bool isValid = ValidateReferencedAggregateRootIsRequired(
            executionContext,
            referencedAggregateRoot
        );

        if (!isValid)
            return false;

        ReferencedAggregateRoot = referencedAggregateRoot;

        return true;
    }
}
