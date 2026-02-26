using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.Claims.Inputs;
using ShopDemo.Auth.Domain.Entities.Claims.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.Claims;

public sealed class Claim
    : EntityBase<Claim>,
    IClaim
{
    // Properties
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // Constructors
    private Claim()
    {
    }

    private Claim(
        EntityInfo entityInfo,
        string name,
        string? description
    ) : base(entityInfo)
    {
        Name = name;
        Description = description;
    }

    // Public Business Methods
    public static Claim? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewClaimInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new Claim(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetName(executionContext, input.Name)
                    & instance.SetDescription(executionContext, input.Description);
            }
        );
    }

    public static Claim CreateFromExistingInfo(
        CreateFromExistingInfoClaimInput input
    )
    {
        return new Claim(
            input.EntityInfo,
            input.Name,
            input.Description
        );
    }

    public override Claim Clone()
    {
        return new Claim(
            EntityInfo,
            Name,
            Description
        );
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? name,
        string? description
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateName(executionContext, name)
            & ValidateDescription(executionContext, description);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            Name,
            Description
        );
    }

    public static bool ValidateName(
        ExecutionContext executionContext,
        string? name
    )
    {
        bool nameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Claim>(propertyName: ClaimMetadata.NamePropertyName),
            isRequired: ClaimMetadata.NameIsRequired,
            value: name
        );

        if (!nameIsRequiredValidation)
            return false;

        bool nameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<Claim>(propertyName: ClaimMetadata.NamePropertyName),
            minLength: ClaimMetadata.NameMinLength,
            value: name!.Length
        );

        bool nameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<Claim>(propertyName: ClaimMetadata.NamePropertyName),
            maxLength: ClaimMetadata.NameMaxLength,
            value: name!.Length
        );

        return nameMinLengthValidation
            & nameMaxLengthValidation;
    }

    public static bool ValidateDescription(
        ExecutionContext executionContext,
        string? description
    )
    {
        if (!ClaimMetadata.DescriptionIsRequired && description is null)
            return true;

        if (description is not null)
        {
            bool descriptionMaxLengthValidation = ValidationUtils.ValidateMaxLength(
                executionContext,
                propertyName: CreateMessageCode<Claim>(propertyName: ClaimMetadata.DescriptionPropertyName),
                maxLength: ClaimMetadata.DescriptionMaxLength,
                value: description.Length
            );

            return descriptionMaxLengthValidation;
        }

        return true;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetName(
        ExecutionContext executionContext,
        string name
    )
    {
        bool isValid = ValidateName(
            executionContext,
            name
        );

        if (!isValid)
            return false;

        Name = name;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetDescription(
        ExecutionContext executionContext,
        string? description
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
    // Stryker restore all

    // Metadata
    public static class ClaimMetadata
    {
        private static readonly Lock _lockObject = new();

        // Name
        public static readonly string NamePropertyName = "Name";
        public static bool NameIsRequired { get; private set; } = true;
        public static int NameMinLength { get; private set; } = 1;
        public static int NameMaxLength { get; private set; } = 255;

        // Description
        public static readonly string DescriptionPropertyName = "Description";
        public static bool DescriptionIsRequired { get; private set; } = false;
        public static int DescriptionMaxLength { get; private set; } = 1000;

        public static void ChangeNameMetadata(
            bool isRequired,
            int minLength,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                NameIsRequired = isRequired;
                NameMinLength = minLength;
                NameMaxLength = maxLength;
            }
        }

        public static void ChangeDescriptionMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                DescriptionIsRequired = isRequired;
                DescriptionMaxLength = maxLength;
            }
        }
    }
}
