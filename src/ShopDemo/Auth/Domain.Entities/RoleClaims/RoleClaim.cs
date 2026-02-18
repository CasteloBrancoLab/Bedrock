using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.RoleClaims.Inputs;
using ShopDemo.Auth.Domain.Entities.RoleClaims.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.RoleClaims;

public sealed class RoleClaim
    : EntityBase<RoleClaim>,
    IRoleClaim
{
    // Properties
    public Id RoleId { get; private set; }
    public Id ClaimId { get; private set; }
    public ClaimValue Value { get; private set; }

    // Constructors
    private RoleClaim()
    {
    }

    private RoleClaim(
        EntityInfo entityInfo,
        Id roleId,
        Id claimId,
        ClaimValue value
    ) : base(entityInfo)
    {
        RoleId = roleId;
        ClaimId = claimId;
        Value = value;
    }

    // Public Business Methods
    public static RoleClaim? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewRoleClaimInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new RoleClaim(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetRoleId(executionContext, input.RoleId)
                    & instance.SetClaimId(executionContext, input.ClaimId)
                    & instance.SetValue(executionContext, input.Value);
            }
        );
    }

    public static RoleClaim CreateFromExistingInfo(
        CreateFromExistingInfoRoleClaimInput input
    )
    {
        return new RoleClaim(
            input.EntityInfo,
            input.RoleId,
            input.ClaimId,
            input.Value
        );
    }

    public RoleClaim? ChangeValue(
        ExecutionContext executionContext,
        ChangeRoleClaimValueInput input
    )
    {
        return RegisterChangeInternal<RoleClaim, ChangeRoleClaimValueInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeValueInternal(executionContext, input.NewValue);
            }
        );
    }

    public override RoleClaim Clone()
    {
        return new RoleClaim(
            EntityInfo,
            RoleId,
            ClaimId,
            Value
        );
    }

    // Private Business Methods
    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool ChangeValueInternal(
        ExecutionContext executionContext,
        ClaimValue newValue
    )
    {
        return SetValue(executionContext, newValue);
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? roleId,
        Id? claimId,
        ClaimValue? value
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateRoleId(executionContext, roleId)
            & ValidateClaimId(executionContext, claimId)
            & ValidateValue(executionContext, value);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            RoleId,
            ClaimId,
            Value
        );
    }

    public static bool ValidateRoleId(
        ExecutionContext executionContext,
        Id? roleId
    )
    {
        bool roleIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<RoleClaim>(propertyName: RoleClaimMetadata.RoleIdPropertyName),
            isRequired: RoleClaimMetadata.RoleIdIsRequired,
            value: roleId
        );

        return roleIdIsRequiredValidation;
    }

    public static bool ValidateClaimId(
        ExecutionContext executionContext,
        Id? claimId
    )
    {
        bool claimIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<RoleClaim>(propertyName: RoleClaimMetadata.ClaimIdPropertyName),
            isRequired: RoleClaimMetadata.ClaimIdIsRequired,
            value: claimId
        );

        return claimIdIsRequiredValidation;
    }

    public static bool ValidateValue(
        ExecutionContext executionContext,
        ClaimValue? value
    )
    {
        bool valueIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<RoleClaim>(propertyName: RoleClaimMetadata.ValuePropertyName),
            isRequired: RoleClaimMetadata.ValueIsRequired,
            value: value
        );

        if (!valueIsRequiredValidation)
            return false;

        if (!ClaimValue.IsValidValue(value!.Value.Value))
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<RoleClaim>(propertyName: RoleClaimMetadata.ValuePropertyName)}.InvalidValue");
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
    private bool SetClaimId(
        ExecutionContext executionContext,
        Id claimId
    )
    {
        bool isValid = ValidateClaimId(
            executionContext,
            claimId
        );

        if (!isValid)
            return false;

        ClaimId = claimId;

        return true;
    }
    // Stryker restore all

    private bool SetValue(
        ExecutionContext executionContext,
        ClaimValue value
    )
    {
        bool isValid = ValidateValue(
            executionContext,
            value
        );

        if (!isValid)
            return false;

        Value = value;

        return true;
    }

    // Metadata
    public static class RoleClaimMetadata
    {
        private static readonly Lock _lockObject = new();

        // RoleId
        public static readonly string RoleIdPropertyName = "RoleId";
        public static bool RoleIdIsRequired { get; private set; } = true;

        // ClaimId
        public static readonly string ClaimIdPropertyName = "ClaimId";
        public static bool ClaimIdIsRequired { get; private set; } = true;

        // Value
        public static readonly string ValuePropertyName = "Value";
        public static bool ValueIsRequired { get; private set; } = true;

        public static void ChangeRoleIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                RoleIdIsRequired = isRequired;
            }
        }

        public static void ChangeClaimIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                ClaimIdIsRequired = isRequired;
            }
        }

        public static void ChangeValueMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                ValueIsRequired = isRequired;
            }
        }
    }
}
