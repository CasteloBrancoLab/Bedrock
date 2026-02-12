using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Templates.Domain.Entities.CompositeAggregateRoots.Inputs;

namespace Templates.Domain.Entities.CompositeAggregateRoots;

public sealed class CompositeChildEntity
    : EntityBase<CompositeChildEntity>
{
    // Metadata
    public static class CompositeChildEntityMetadata
    {
        // Fields
        private static readonly Lock _lockObject = new();

        // Title
        public static readonly string TitlePropertyName = nameof(Title);
        public static bool TitleIsRequired { get; private set; } = true;
        public static int TitleMinLength { get; private set; } = 1;
        public static int TitleMaxLength { get; private set; } = 255;

        // Description
        public static readonly string DescriptionPropertyName = nameof(Description);
        public static bool DescriptionIsRequired { get; private set; }
        public static int DescriptionMinLength { get; private set; }
        public static int DescriptionMaxLength { get; private set; } = 1000;

        // Priority
        public static readonly string PriorityPropertyName = nameof(Priority);
        public static bool PriorityIsRequired { get; private set; } = true;
        public static int PriorityMinValue { get; private set; } = 1;
        public static int PriorityMaxValue { get; private set; } = 10;

        // Public Methods
        public static void ChangeTitleMetadata(
            bool isRequired,
            int minLength,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                TitleIsRequired = isRequired;
                TitleMinLength = minLength;
                TitleMaxLength = maxLength;
            }
        }

        public static void ChangeDescriptionMetadata(
            bool isRequired,
            int minLength,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                DescriptionIsRequired = isRequired;
                DescriptionMinLength = minLength;
                DescriptionMaxLength = maxLength;
            }
        }

        public static void ChangePriorityMetadata(
            bool isRequired,
            int minValue,
            int maxValue
        )
        {
            lock (_lockObject)
            {
                PriorityIsRequired = isRequired;
                PriorityMinValue = minValue;
                PriorityMaxValue = maxValue;
            }
        }
    }

    // Properties
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Priority { get; private set; }

    // Constructors
    private CompositeChildEntity()
    {
    }

    private CompositeChildEntity(
        EntityInfo entityInfo,
        string title,
        string description,
        int priority
    ) : base(entityInfo)
    {
        Title = title;
        Description = description;
        Priority = priority;
    }

    // Public Business Methods
    public static CompositeChildEntity? RegisterNew(
        ExecutionContext executionContext,
        ChildRegisterNewInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: (executionContext, input) => new CompositeChildEntity(),
            handler: (executionContext, input, instance) =>
            {
                return
                    instance.ChangeTitleInternal(executionContext, input.Title)
                    & instance.ChangeDescriptionInternal(executionContext, input.Description)
                    & instance.ChangePriorityInternal(executionContext, input.Priority);
            }
        );
    }

    public static CompositeChildEntity CreateFromExistingInfo(
        ChildCreateFromExistingInfoInput input
    )
    {
        return new CompositeChildEntity(
            input.EntityInfo,
            input.Title,
            input.Description,
            input.Priority
        );
    }

    public CompositeChildEntity? ChangeTitle(
        ExecutionContext executionContext,
        ChildChangeTitleInput input
    )
    {
        return RegisterChangeInternal<CompositeChildEntity, ChildChangeTitleInput>(
            executionContext,
            instance: this,
            input,
            handler: (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeTitleInternal(executionContext, input.Title);
            }
        );
    }

    public CompositeChildEntity? ChangeDescription(
        ExecutionContext executionContext,
        ChildChangeDescriptionInput input
    )
    {
        return RegisterChangeInternal<CompositeChildEntity, ChildChangeDescriptionInput>(
            executionContext,
            instance: this,
            input,
            handler: (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeDescriptionInternal(executionContext, input.Description);
            }
        );
    }

    public CompositeChildEntity? ChangePriority(
        ExecutionContext executionContext,
        ChildChangePriorityInput input
    )
    {
        return RegisterChangeInternal<CompositeChildEntity, ChildChangePriorityInput>(
            executionContext,
            instance: this,
            input,
            handler: (executionContext, input, newInstance) =>
            {
                return newInstance.ChangePriorityInternal(executionContext, input.Priority);
            }
        );
    }

    public override CompositeChildEntity Clone()
    {
        return new CompositeChildEntity(
            EntityInfo,
            Title,
            Description,
            Priority
        );
    }

    // Private Business Methods
    private bool ChangeTitleInternal(
        ExecutionContext executionContext,
        string title
    )
    {
        bool isSuccess = SetTitle(executionContext, title);

        return isSuccess;
    }

    private bool ChangeDescriptionInternal(
        ExecutionContext executionContext,
        string description
    )
    {
        bool isSuccess = SetDescription(executionContext, description);

        return isSuccess;
    }

    private bool ChangePriorityInternal(
        ExecutionContext executionContext,
        int priority
    )
    {
        bool isSuccess = SetPriority(executionContext, priority);

        return isSuccess;
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? title,
        string? description,
        int? priority
    )
    {
        return
            EntityBaseIsValid(executionContext, entityInfo)
            & ValidateTitle(executionContext, title)
            & ValidateDescription(executionContext, description)
            & ValidatePriority(executionContext, priority);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            Title,
            Description,
            Priority
        );
    }

    public static bool ValidateTitle(
        ExecutionContext executionContext,
        string? title
    )
    {
        bool titleIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<CompositeChildEntity>(propertyName: CompositeChildEntityMetadata.TitlePropertyName),
            isRequired: CompositeChildEntityMetadata.TitleIsRequired,
            value: title
        );

        if (!titleIsRequiredValidation)
            return false;

        bool titleMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<CompositeChildEntity>(propertyName: CompositeChildEntityMetadata.TitlePropertyName),
            minLength: CompositeChildEntityMetadata.TitleMinLength,
            value: title!.Length
        );

        bool titleMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<CompositeChildEntity>(propertyName: CompositeChildEntityMetadata.TitlePropertyName),
            maxLength: CompositeChildEntityMetadata.TitleMaxLength,
            value: title!.Length
        );

        return titleIsRequiredValidation
            && titleMinLengthValidation
            && titleMaxLengthValidation;
    }

    public static bool ValidateDescription(
        ExecutionContext executionContext,
        string? description
    )
    {
        bool descriptionIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<CompositeChildEntity>(propertyName: CompositeChildEntityMetadata.DescriptionPropertyName),
            isRequired: CompositeChildEntityMetadata.DescriptionIsRequired,
            value: description
        );

        if (!descriptionIsRequiredValidation)
            return false;

        if (description is null)
            return true;

        bool descriptionMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<CompositeChildEntity>(propertyName: CompositeChildEntityMetadata.DescriptionPropertyName),
            minLength: CompositeChildEntityMetadata.DescriptionMinLength,
            value: description.Length
        );

        bool descriptionMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<CompositeChildEntity>(propertyName: CompositeChildEntityMetadata.DescriptionPropertyName),
            maxLength: CompositeChildEntityMetadata.DescriptionMaxLength,
            value: description.Length
        );

        return descriptionIsRequiredValidation
            && descriptionMinLengthValidation
            && descriptionMaxLengthValidation;
    }

    public static bool ValidatePriority(
        ExecutionContext executionContext,
        int? priority
    )
    {
        bool priorityIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<CompositeChildEntity>(propertyName: CompositeChildEntityMetadata.PriorityPropertyName),
            isRequired: CompositeChildEntityMetadata.PriorityIsRequired,
            value: priority
        );

        if (!priorityIsRequiredValidation)
            return false;

        bool priorityMinValueValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<CompositeChildEntity>(propertyName: CompositeChildEntityMetadata.PriorityPropertyName),
            minLength: CompositeChildEntityMetadata.PriorityMinValue,
            value: priority!.Value
        );

        bool priorityMaxValueValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<CompositeChildEntity>(propertyName: CompositeChildEntityMetadata.PriorityPropertyName),
            maxLength: CompositeChildEntityMetadata.PriorityMaxValue,
            value: priority!.Value
        );

        return priorityIsRequiredValidation
            && priorityMinValueValidation
            && priorityMaxValueValidation;
    }

    // Set Methods
    private bool SetTitle(
        ExecutionContext executionContext,
        string title
    )
    {
        bool isValid = ValidateTitle(
            executionContext,
            title
        );

        if (!isValid)
            return false;

        Title = title;

        return true;
    }

    private bool SetDescription(
        ExecutionContext executionContext,
        string description
    )
    {
        bool isValid = ValidateDescription(
            executionContext,
            description
        );

        if (!isValid)
            return false;

        Description = description;

        return true;
    }

    private bool SetPriority(
        ExecutionContext executionContext,
        int priority
    )
    {
        bool isValid = ValidatePriority(
            executionContext,
            priority
        );

        if (!isValid)
            return false;

        Priority = priority;

        return true;
    }
}
