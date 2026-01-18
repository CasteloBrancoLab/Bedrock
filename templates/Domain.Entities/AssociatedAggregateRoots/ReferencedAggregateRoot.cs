using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Templates.Domain.Entities.AssociatedAggregateRoots.Inputs;

namespace Templates.Domain.Entities.AssociatedAggregateRoots;

public sealed class ReferencedAggregateRoot
    : EntityBase<ReferencedAggregateRoot>,
    IAggregateRoot
{
    public static class ReferencedAggregateRootMetadata
    {
        // Fields
        private static readonly Lock _lockObject = new();

        // SampleName
        public static readonly string SampleNamePropertyName = nameof(SampleName);
        public static bool SampleNameIsRequired { get; private set; } = true;
        public static int SampleNameMinLength { get; private set; } = 1;
        public static int SampleNameMaxLength { get; private set; } = 255;

        public static void ChangeSampleNameMetadata(
            bool isRequired,
            int minLength,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                SampleNameIsRequired = isRequired;
                SampleNameMinLength = minLength;
                SampleNameMaxLength = maxLength;
            }
        }
    }

    // Properties
    public string SampleName { get; private set; } = string.Empty;

    // Constructors
    private ReferencedAggregateRoot()
    {
    }

    private ReferencedAggregateRoot(
        EntityInfo entityInfo,
        string sampleName
    ) : base(entityInfo)
    {
        SampleName = sampleName;
    }

    // Public Business Methods
    public static ReferencedAggregateRoot? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: (executionContext, input) => new ReferencedAggregateRoot(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.ChangeSampleNameInternal(executionContext, input.SampleName);
            }
        );
    }

    public static ReferencedAggregateRoot CreateFromExistingInfo(
        CreateFromExistingInfoInput input
    )
    {
        return new ReferencedAggregateRoot(
            input.EntityInfo,
            input.SampleName
        );
    }

    public ReferencedAggregateRoot? ChangeSampleName(
        ExecutionContext executionContext,
        ChangeSampleNameInput input
    )
    {
        return RegisterChangeInternal<ReferencedAggregateRoot, ChangeSampleNameInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeSampleNameInternal(executionContext, input.SampleName);
            }
        );
    }

    public override ReferencedAggregateRoot Clone()
    {
        return new ReferencedAggregateRoot(
            EntityInfo,
            SampleName
        );
    }

    // Private Business Methods
    private bool ChangeSampleNameInternal(
        ExecutionContext executionContext,
        string sampleName
    )
    {
        return SetSampleName(executionContext, sampleName);
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? sampleName
    )
    {
        return
            EntityBaseIsValid(executionContext, entityInfo)
            & ValidateSampleName(executionContext, sampleName);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            SampleName
        );
    }

    public static bool ValidateSampleName(
        ExecutionContext executionContext,
        string? sampleName
    )
    {
        bool sampleNameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ReferencedAggregateRoot>(propertyName: ReferencedAggregateRootMetadata.SampleNamePropertyName),
            isRequired: ReferencedAggregateRootMetadata.SampleNameIsRequired,
            value: sampleName
        );

        if (!sampleNameIsRequiredValidation)
            return false;

        bool sampleNameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<ReferencedAggregateRoot>(propertyName: ReferencedAggregateRootMetadata.SampleNamePropertyName),
            minLength: ReferencedAggregateRootMetadata.SampleNameMinLength,
            value: sampleName!.Length
        );

        bool sampleNameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<ReferencedAggregateRoot>(propertyName: ReferencedAggregateRootMetadata.SampleNamePropertyName),
            maxLength: ReferencedAggregateRootMetadata.SampleNameMaxLength,
            value: sampleName!.Length
        );

        return sampleNameIsRequiredValidation
            && sampleNameMinLengthValidation
            && sampleNameMaxLengthValidation;
    }

    // Set Methods
    private bool SetSampleName(
        ExecutionContext executionContext,
        string sampleName
    )
    {
        bool isValid = ValidateSampleName(
            executionContext,
            sampleName
        );

        if (!isValid)
            return false;

        SampleName = sampleName;

        return true;
    }
}
