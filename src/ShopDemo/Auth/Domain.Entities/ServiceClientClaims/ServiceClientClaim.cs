using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims.Inputs;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.ServiceClientClaims;

public sealed class ServiceClientClaim
    : EntityBase<ServiceClientClaim>,
    IServiceClientClaim
{
    // Properties
    public Id ServiceClientId { get; private set; }
    public Id ClaimId { get; private set; }
    public ClaimValue Value { get; private set; }

    // Constructors
    private ServiceClientClaim()
    {
    }

    private ServiceClientClaim(
        EntityInfo entityInfo,
        Id serviceClientId,
        Id claimId,
        ClaimValue value
    ) : base(entityInfo)
    {
        ServiceClientId = serviceClientId;
        ClaimId = claimId;
        Value = value;
    }

    // Public Business Methods
    public static ServiceClientClaim? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewServiceClientClaimInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new ServiceClientClaim(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetServiceClientId(executionContext, input.ServiceClientId)
                    & instance.SetClaimId(executionContext, input.ClaimId)
                    & instance.SetValue(executionContext, input.Value);
            }
        );
    }

    public static ServiceClientClaim CreateFromExistingInfo(
        CreateFromExistingInfoServiceClientClaimInput input
    )
    {
        return new ServiceClientClaim(
            input.EntityInfo,
            input.ServiceClientId,
            input.ClaimId,
            input.Value
        );
    }

    public override ServiceClientClaim Clone()
    {
        return new ServiceClientClaim(
            EntityInfo,
            ServiceClientId,
            ClaimId,
            Value
        );
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? serviceClientId,
        Id? claimId,
        ClaimValue? value
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateServiceClientId(executionContext, serviceClientId)
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
            ServiceClientId,
            ClaimId,
            Value
        );
    }

    public static bool ValidateServiceClientId(
        ExecutionContext executionContext,
        Id? serviceClientId
    )
    {
        bool serviceClientIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ServiceClientClaim>(propertyName: ServiceClientClaimMetadata.ServiceClientIdPropertyName),
            isRequired: ServiceClientClaimMetadata.ServiceClientIdIsRequired,
            value: serviceClientId
        );

        return serviceClientIdIsRequiredValidation;
    }

    public static bool ValidateClaimId(
        ExecutionContext executionContext,
        Id? claimId
    )
    {
        bool claimIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ServiceClientClaim>(propertyName: ServiceClientClaimMetadata.ClaimIdPropertyName),
            isRequired: ServiceClientClaimMetadata.ClaimIdIsRequired,
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
            propertyName: CreateMessageCode<ServiceClientClaim>(propertyName: ServiceClientClaimMetadata.ValuePropertyName),
            isRequired: ServiceClientClaimMetadata.ValueIsRequired,
            value: value
        );

        if (!valueIsRequiredValidation)
            return false;

        if (!ClaimValue.IsValidValue(value!.Value.Value))
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<ServiceClientClaim>(propertyName: ServiceClientClaimMetadata.ValuePropertyName)}.InvalidValue");
            return false;
        }

        return true;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetServiceClientId(
        ExecutionContext executionContext,
        Id serviceClientId
    )
    {
        bool isValid = ValidateServiceClientId(
            executionContext,
            serviceClientId
        );

        if (!isValid)
            return false;

        ServiceClientId = serviceClientId;

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

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
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
    // Stryker restore all

    // Metadata
    public static class ServiceClientClaimMetadata
    {
        private static readonly Lock _lockObject = new();

        // ServiceClientId
        public static readonly string ServiceClientIdPropertyName = "ServiceClientId";
        public static bool ServiceClientIdIsRequired { get; private set; } = true;

        // ClaimId
        public static readonly string ClaimIdPropertyName = "ClaimId";
        public static bool ClaimIdIsRequired { get; private set; } = true;

        // Value
        public static readonly string ValuePropertyName = "Value";
        public static bool ValueIsRequired { get; private set; } = true;

        public static void ChangeServiceClientIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                ServiceClientIdIsRequired = isRequired;
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
