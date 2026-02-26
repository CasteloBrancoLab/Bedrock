using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.UserRoles.Inputs;
using ShopDemo.Auth.Domain.Entities.UserRoles.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.UserRoles;

public sealed class UserRole
    : EntityBase<UserRole>,
    IUserRole
{
    // Properties
    public Id UserId { get; private set; }
    public Id RoleId { get; private set; }

    // Constructors
    private UserRole()
    {
    }

    private UserRole(
        EntityInfo entityInfo,
        Id userId,
        Id roleId
    ) : base(entityInfo)
    {
        UserId = userId;
        RoleId = roleId;
    }

    // Public Business Methods
    public static UserRole? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewUserRoleInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new UserRole(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetUserId(executionContext, input.UserId)
                    & instance.SetRoleId(executionContext, input.RoleId);
            }
        );
    }

    public static UserRole CreateFromExistingInfo(
        CreateFromExistingInfoUserRoleInput input
    )
    {
        return new UserRole(
            input.EntityInfo,
            input.UserId,
            input.RoleId
        );
    }

    public override UserRole Clone()
    {
        return new UserRole(
            EntityInfo,
            UserId,
            RoleId
        );
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? userId,
        Id? roleId
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUserId(executionContext, userId)
            & ValidateRoleId(executionContext, roleId);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            UserId,
            RoleId
        );
    }

    public static bool ValidateUserId(
        ExecutionContext executionContext,
        Id? userId
    )
    {
        bool userIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<UserRole>(propertyName: UserRoleMetadata.UserIdPropertyName),
            isRequired: UserRoleMetadata.UserIdIsRequired,
            value: userId
        );

        if (!userIdIsRequiredValidation)
            return false;

        if (userId!.Value.Value == Guid.Empty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<UserRole>(propertyName: UserRoleMetadata.UserIdPropertyName)}.IsRequired");
            return false;
        }

        return true;
    }

    public static bool ValidateRoleId(
        ExecutionContext executionContext,
        Id? roleId
    )
    {
        bool roleIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<UserRole>(propertyName: UserRoleMetadata.RoleIdPropertyName),
            isRequired: UserRoleMetadata.RoleIdIsRequired,
            value: roleId
        );

        if (!roleIdIsRequiredValidation)
            return false;

        if (roleId!.Value.Value == Guid.Empty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<UserRole>(propertyName: UserRoleMetadata.RoleIdPropertyName)}.IsRequired");
            return false;
        }

        return true;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetUserId(
        ExecutionContext executionContext,
        Id userId
    )
    {
        bool isValid = ValidateUserId(
            executionContext,
            userId
        );

        if (!isValid)
            return false;

        UserId = userId;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetRoleId(
        ExecutionContext executionContext,
        Id roleId
    )
    {
        bool isValid = ValidateRoleId(
            executionContext,
            roleId
        );

        if (!isValid)
            return false;

        RoleId = roleId;

        return true;
    }
    // Stryker restore all

    // Metadata
    public static class UserRoleMetadata
    {
        private static readonly Lock _lockObject = new();

        // UserId
        public static readonly string UserIdPropertyName = "UserId";
        public static bool UserIdIsRequired { get; private set; } = true;

        // RoleId
        public static readonly string RoleIdPropertyName = "RoleId";
        public static bool RoleIdIsRequired { get; private set; } = true;

        public static void ChangeUserIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                UserIdIsRequired = isRequired;
            }
        }

        public static void ChangeRoleIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                RoleIdIsRequired = isRequired;
            }
        }
    }
}
