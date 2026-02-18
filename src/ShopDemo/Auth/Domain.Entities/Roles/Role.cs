using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.Roles.Inputs;
using ShopDemo.Auth.Domain.Entities.Roles.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.Roles;

public sealed class Role
    : EntityBase<Role>,
    IRole
{
    // Properties
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // Constructors
    private Role()
    {
    }

    private Role(
        EntityInfo entityInfo,
        string name,
        string? description
    ) : base(entityInfo)
    {
        Name = name;
        Description = description;
    }

    // Public Business Methods
    public static Role? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewRoleInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new Role(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetName(executionContext, input.Name)
                    & instance.SetDescription(executionContext, input.Description);
            }
        );
    }

    public static Role CreateFromExistingInfo(
        CreateFromExistingInfoRoleInput input
    )
    {
        return new Role(
            input.EntityInfo,
            input.Name,
            input.Description
        );
    }

    public Role? Change(
        ExecutionContext executionContext,
        ChangeRoleInput input
    )
    {
        return RegisterChangeInternal<Role, ChangeRoleInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeInternal(executionContext, input.Name, input.Description);
            }
        );
    }

    public override Role Clone()
    {
        return new Role(
            EntityInfo,
            Name,
            Description
        );
    }

    // Private Business Methods
    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool ChangeInternal(
        ExecutionContext executionContext,
        string newName,
        string? newDescription
    )
    {
        bool isSuccess = SetName(executionContext, newName)
            & SetDescription(executionContext, newDescription);

        return isSuccess;
    }
    // Stryker restore all

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
            propertyName: CreateMessageCode<Role>(propertyName: RoleMetadata.NamePropertyName),
            isRequired: RoleMetadata.NameIsRequired,
            value: name
        );

        if (!nameIsRequiredValidation)
            return false;

        bool nameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<Role>(propertyName: RoleMetadata.NamePropertyName),
            minLength: RoleMetadata.NameMinLength,
            value: name!.Length
        );

        bool nameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<Role>(propertyName: RoleMetadata.NamePropertyName),
            maxLength: RoleMetadata.NameMaxLength,
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
        if (!RoleMetadata.DescriptionIsRequired && description is null)
            return true;

        if (description is not null)
        {
            bool descriptionMaxLengthValidation = ValidationUtils.ValidateMaxLength(
                executionContext,
                propertyName: CreateMessageCode<Role>(propertyName: RoleMetadata.DescriptionPropertyName),
                maxLength: RoleMetadata.DescriptionMaxLength,
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
    public static class RoleMetadata
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
