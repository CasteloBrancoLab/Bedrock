using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies.Inputs;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.RoleHierarchies;

public sealed class RoleHierarchy
    : EntityBase<RoleHierarchy>,
    IRoleHierarchy
{
    // Properties
    public Id RoleId { get; private set; }
    public Id ParentRoleId { get; private set; }

    // Constructors
    private RoleHierarchy()
    {
    }

    private RoleHierarchy(
        EntityInfo entityInfo,
        Id roleId,
        Id parentRoleId
    ) : base(entityInfo)
    {
        RoleId = roleId;
        ParentRoleId = parentRoleId;
    }

    // Public Business Methods
    public static RoleHierarchy? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewRoleHierarchyInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new RoleHierarchy(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetRoleId(executionContext, input.RoleId)
                    & instance.SetParentRoleId(executionContext, input.ParentRoleId)
                    & CheckSelfReferenceProhibited(executionContext, input.RoleId, input.ParentRoleId);
            }
        );
    }

    public static RoleHierarchy CreateFromExistingInfo(
        CreateFromExistingInfoRoleHierarchyInput input
    )
    {
        return new RoleHierarchy(
            input.EntityInfo,
            input.RoleId,
            input.ParentRoleId
        );
    }

    public override RoleHierarchy Clone()
    {
        return new RoleHierarchy(
            EntityInfo,
            RoleId,
            ParentRoleId
        );
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? roleId,
        Id? parentRoleId
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateRoleId(executionContext, roleId)
            & ValidateParentRoleId(executionContext, parentRoleId);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            RoleId,
            ParentRoleId
        );
    }

    public static bool ValidateRoleId(
        ExecutionContext executionContext,
        Id? roleId
    )
    {
        bool roleIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<RoleHierarchy>(propertyName: RoleHierarchyMetadata.RoleIdPropertyName),
            isRequired: RoleHierarchyMetadata.RoleIdIsRequired,
            value: roleId
        );

        if (!roleIdIsRequiredValidation)
            return false;

        if (roleId!.Value.Value == Guid.Empty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<RoleHierarchy>(propertyName: RoleHierarchyMetadata.RoleIdPropertyName)}.IsRequired");
            return false;
        }

        return true;
    }

    public static bool ValidateParentRoleId(
        ExecutionContext executionContext,
        Id? parentRoleId
    )
    {
        bool parentRoleIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<RoleHierarchy>(propertyName: RoleHierarchyMetadata.ParentRoleIdPropertyName),
            isRequired: RoleHierarchyMetadata.ParentRoleIdIsRequired,
            value: parentRoleId
        );

        if (!parentRoleIdIsRequiredValidation)
            return false;

        if (parentRoleId!.Value.Value == Guid.Empty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<RoleHierarchy>(propertyName: RoleHierarchyMetadata.ParentRoleIdPropertyName)}.IsRequired");
            return false;
        }

        return true;
    }

    private static bool CheckSelfReferenceProhibited(
        ExecutionContext executionContext,
        Id roleId,
        Id parentRoleId
    )
    {
        if (roleId == parentRoleId)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<RoleHierarchy>(propertyName: RoleHierarchyMetadata.ParentRoleIdPropertyName)}.SelfReference");
            return false;
        }

        return true;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
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

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetParentRoleId(
        ExecutionContext executionContext,
        Id parentRoleId
    )
    {
        bool isValid = ValidateParentRoleId(
            executionContext,
            parentRoleId
        );

        if (!isValid)
            return false;

        ParentRoleId = parentRoleId;

        return true;
    }
    // Stryker restore all

    // Metadata
    public static class RoleHierarchyMetadata
    {
        private static readonly Lock _lockObject = new();

        // RoleId
        public static readonly string RoleIdPropertyName = "RoleId";
        public static bool RoleIdIsRequired { get; private set; } = true;

        // ParentRoleId
        public static readonly string ParentRoleIdPropertyName = "ParentRoleId";
        public static bool ParentRoleIdIsRequired { get; private set; } = true;

        public static void ChangeRoleIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                RoleIdIsRequired = isRequired;
            }
        }

        public static void ChangeParentRoleIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                ParentRoleIdIsRequired = isRequired;
            }
        }
    }
}
